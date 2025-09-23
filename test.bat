@echo off
setlocal

echo Exporting project...
godot --headless --export-debug "Windows Desktop" ./build/a.exe

if %ERRORLEVEL% neq 0 (
    echo Build failed with error code %ERRORLEVEL%.
    exit /b %ERRORLEVEL%
)

echo Build succeeded. Launching two instances...

start "" "./build/a.exe"
timeout /t 1 >nul
start "" "./build/a.exe"

endlocal
