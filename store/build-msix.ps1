# Arma el paquete MSIX de PadBox para la Microsoft Store: dist\PadBox-<versión>.msix
# Antes: ejecuta build.bat (compila PadBox.exe y genera los sonidos). Usa makeappx/makepri del paquete oficial
# Microsoft.Windows.SDK.BuildTools, descomprimido en tools\buildtools (no hace falta instalar el SDK).
# El paquete NO se firma: la Store lo firma al publicarlo.
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$id = Get-Content (Join-Path $PSScriptRoot 'identity.json') -Raw | ConvertFrom-Json
$app = Get-ChildItem (Join-Path $repo 'dist') -Directory -Filter 'PadBox-*' | Sort-Object Name | Select-Object -Last 1
if (-not $app) { throw 'Primero ejecuta build.bat' }

$tools = Get-ChildItem (Join-Path $repo 'tools\buildtools') -Recurse -Filter makeappx.exe -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match '\\x64\\' } | Select-Object -First 1
if (-not $tools) { throw 'Falta tools\buildtools (Microsoft.Windows.SDK.BuildTools de NuGet)' }
$bin = $tools.DirectoryName

# 1. Carpeta del paquete: la app, sus sonidos, los íconos y el manifiesto
$layout = Join-Path $repo 'dist\msix'
if (Test-Path $layout) { Remove-Item $layout -Recurse -Force }
New-Item -ItemType Directory -Force $layout | Out-Null
Copy-Item (Join-Path $app.FullName '*') $layout -Recurse
# El LEEME es de la versión portable (habla de abrir el .exe y de la falta de firma); en la Store no aplica.
Remove-Item (Join-Path $layout 'LEEME.txt') -Force
# Nada de configuraciones ni sonidos personales dentro del paquete
Get-ChildItem $layout -Filter '*config*.json' | Remove-Item -Force
Copy-Item (Join-Path $PSScriptRoot 'Assets') (Join-Path $layout 'Assets') -Recurse
$manifest = (Get-Content (Join-Path $PSScriptRoot 'AppxManifest.xml') -Raw).
    Replace('{{NAME}}', $id.name).Replace('{{PUBLISHER}}', $id.publisher).
    Replace('{{PUBLISHER_DISPLAY_NAME}}', $id.publisherDisplayName).Replace('{{VERSION}}', $id.version)
[IO.File]::WriteAllText((Join-Path $layout 'AppxManifest.xml'), $manifest, (New-Object Text.UTF8Encoding $false))

# 2. Índice de recursos (para que Windows elija el tamaño de ícono correcto en cada pantalla)
$cfg = Join-Path $repo 'dist\priconfig.xml'
& (Join-Path $bin 'makepri.exe') createconfig /cf $cfg /dq es /pv 10.0.0 /o | Out-Null
# Un solo índice dentro del paquete (sin separar los íconos de 200 % en otro archivo)
$x = [xml](Get-Content $cfg -Raw)
$x.SelectNodes('//packaging') | ForEach-Object { $_.ParentNode.RemoveChild($_) | Out-Null }
$x.Save($cfg)
& (Join-Path $bin 'makepri.exe') new /pr $layout /cf $cfg /mn (Join-Path $layout 'AppxManifest.xml') /of (Join-Path $layout 'resources.pri') /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'makepri falló' }

# 3. Paquete
$out = Join-Path $repo ("dist\PadBox-{0}.msix" -f $id.version)
& (Join-Path $bin 'makeappx.exe') pack /d $layout /p $out /o
if ($LASTEXITCODE -ne 0) { throw 'makeappx falló' }
"Listo: $out"
