# Junta out\video.mp4 (imagen) y out\musica.wav (sonido) en out\PadBox-promo.mp4 (H.264 + AAC, 1080p)
# usando el editor de video que trae Windows (Windows.Media.Editing). No necesita ffmpeg.
param([string]$Dir = (Join-Path $PSScriptRoot 'out'), [string]$Name = 'PadBox-promo.mp4', [int]$Bitrate = 12000000)

Add-Type -AssemblyName System.Runtime.WindowsRuntime
$null = [Windows.Storage.StorageFile, Windows.Storage, ContentType = WindowsRuntime]
$null = [Windows.Storage.StorageFolder, Windows.Storage, ContentType = WindowsRuntime]
$null = [Windows.Media.Editing.MediaComposition, Windows.Media.Editing, ContentType = WindowsRuntime]
$null = [Windows.Media.MediaProperties.MediaEncodingProfile, Windows.Media.MediaProperties, ContentType = WindowsRuntime]

$ext = [System.WindowsRuntimeSystemExtensions].GetMethods()
$asTaskOp = $ext | Where-Object { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' } | Select-Object -First 1
$asTaskProg = $ext | Where-Object { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperationWithProgress`2' } | Select-Object -First 1

function Await($op, [type]$result) {
    $task = $asTaskOp.MakeGenericMethod($result).Invoke($null, @($op))
    $task.Wait(-1) | Out-Null
    $task.Result
}

$video = Await ([Windows.Storage.StorageFile]::GetFileFromPathAsync((Join-Path $Dir 'video.mp4'))) ([Windows.Storage.StorageFile])
$audio = Await ([Windows.Storage.StorageFile]::GetFileFromPathAsync((Join-Path $Dir 'musica.wav'))) ([Windows.Storage.StorageFile])
$clip = Await ([Windows.Media.Editing.MediaClip]::CreateFromFileAsync($video)) ([Windows.Media.Editing.MediaClip])
$music = Await ([Windows.Media.Editing.BackgroundAudioTrack]::CreateFromFileAsync($audio)) ([Windows.Media.Editing.BackgroundAudioTrack])

$comp = New-Object Windows.Media.Editing.MediaComposition
# PowerShell no ve el método Add de las listas de WinRT: se llama por la interfaz ICollection<T> de .NET
[System.Collections.Generic.ICollection[Windows.Media.Editing.MediaClip]].GetMethod('Add').Invoke($comp.Clips, @($clip)) | Out-Null
[System.Collections.Generic.ICollection[Windows.Media.Editing.BackgroundAudioTrack]].GetMethod('Add').Invoke($comp.BackgroundAudioTracks, @($music)) | Out-Null
"duración: " + $comp.Duration

$folder = Await ([Windows.Storage.StorageFolder]::GetFolderFromPathAsync($Dir)) ([Windows.Storage.StorageFolder])
$out = Await ($folder.CreateFileAsync($Name, [Windows.Storage.CreationCollisionOption]::ReplaceExisting)) ([Windows.Storage.StorageFile])
$profile = [Windows.Media.MediaProperties.MediaEncodingProfile]::CreateMp4([Windows.Media.MediaProperties.VideoEncodingQuality]::HD1080p)
$profile.Video.Bitrate = $Bitrate   # la Store acepta hasta 10 MB por archivo al subirlo desde aquí: usa -Bitrate 2000000
$profile.Video.FrameRate.Numerator = 30
$profile.Video.FrameRate.Denominator = 1

$render = $comp.RenderToFileAsync($out, [Windows.Media.Editing.MediaTrimmingPreference]::Precise, $profile)
$task = $asTaskProg.MakeGenericMethod([Windows.Media.Transcoding.TranscodeFailureReason], [double]).Invoke($null, @($render))
$task.Wait(-1) | Out-Null
"resultado: $($task.Result)"
"archivo: " + (Join-Path $Dir $Name)
