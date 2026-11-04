@echo off
rem Compila PadBox con el compilador de C# que trae Windows (.NET Framework 4.8), genera los sonidos
rem originales del preset "Drum Pad EDM" y arma dist\PadBox-<version>.zip listo para publicar.
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set VER=1.0.0
set OUT=%~dp0dist\PadBox-%VER%

if exist "%OUT%" rmdir /s /q "%OUT%"
mkdir "%OUT%" || exit /b 1

"%CSC%" /nologo /target:winexe /optimize+ /out:"%OUT%\PadBox.exe" /win32icon:"%~dp0src\PadBox.ico" /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0src\PadBox.cs" || exit /b 1
"%CSC%" /nologo /optimize+ /out:"%~dp0dist\GenSonidos.exe" "%~dp0src\GenSonidos.cs" || exit /b 1
"%~dp0dist\GenSonidos.exe" "%OUT%" || exit /b 1

copy /y "%~dp0LEEME.txt" "%OUT%\LEEME.txt" >nul
copy /y "%~dp0LICENSE" "%OUT%\LICENSE.txt" >nul
copy /y "%~dp0AVISOS.txt" "%OUT%\AVISOS.txt" >nul

powershell -NoProfile -Command "Compress-Archive -Path '%OUT%\*' -DestinationPath '%~dp0dist\PadBox-%VER%.zip' -Force" || exit /b 1
echo.
echo Listo: dist\PadBox-%VER%.zip
