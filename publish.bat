@echo off
REM ============================================================
REM  NWD Updater - Self-contained publish script
REM  Output: NWDUpdater\publish\NWDUpdater.exe  (~55 MB)
REM  Requirements: .NET 8 SDK  (https://dotnet.microsoft.com)
REM ============================================================

echo.
echo  Building NWD Updater - self-contained single file...
echo.

dotnet publish "%~dp0NWDUpdater\NWDUpdater.csproj" ^
  /p:PublishSingleFile=true ^
  /p:SelfContained=true ^
  /p:RuntimeIdentifier=win-x64 ^
  /p:Configuration=Release ^
  /p:EnableCompressionInSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:DebugType=embedded ^
  --output "%~dp0publish"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  ERROR: Build failed. Make sure .NET 8 SDK is installed.
    pause
    exit /b 1
)

echo.
echo  Done! Output: %~dp0publish\NWDUpdater.exe
echo.
echo  Copy NWDUpdater.exe to any Windows 10/11 machine and run it.
echo  No .NET installation required on the target machine.
echo.
pause
