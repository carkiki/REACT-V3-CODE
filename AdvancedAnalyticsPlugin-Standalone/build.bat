@echo off
REM Advanced Analytics Plugin - Standalone Build Script for Windows
REM This script builds the plugin as a standalone DLL

echo =========================================
echo Advanced Analytics Plugin - Standalone Build
echo =========================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed
    echo Please install .NET 10.0 SDK or higher from:
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [OK] .NET SDK found
dotnet --version
echo.

REM Clean previous build
echo [1/4] Cleaning previous build...
if exist bin rd /s /q bin
if exist obj rd /s /q obj
echo [OK] Clean complete
echo.

REM Restore NuGet packages
echo [2/4] Restoring NuGet packages...
dotnet restore AdvancedAnalyticsPlugin.csproj --nologo
if errorlevel 1 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)
echo [OK] NuGet packages restored
echo.

REM Build plugin in Release mode
echo [3/4] Building Advanced Analytics Plugin...
dotnet build AdvancedAnalyticsPlugin.csproj -c Release --nologo --no-restore
if errorlevel 1 (
    echo ERROR: Failed to build plugin
    pause
    exit /b 1
)
echo [OK] Plugin built successfully
echo.

REM Check if merged DLL exists
echo [4/4] Verifying output...
set PLUGIN_DLL=bin\Release\net10.0-windows7.0\AdvancedAnalyticsPlugin.dll

if exist "%PLUGIN_DLL%" (
    echo [OK] Plugin DLL created successfully:
    echo      %PLUGIN_DLL%
    echo.

    REM Display file size
    for %%F in ("%PLUGIN_DLL%") do (
        set size=%%~zF
        set /a sizeKB=%%~zF/1024
        set /a sizeMB=%%~zF/1048576
    )
    echo [INFO] File size: %sizeMB% MB
    echo.

    REM Create output folder
    if not exist "output" mkdir "output"

    REM Copy DLL to output folder
    echo [INFO] Copying plugin to output folder...
    copy /Y "%PLUGIN_DLL%" "output\AdvancedAnalyticsPlugin.dll" >nul

    if errorlevel 0 (
        echo [OK] Plugin copied to: output\AdvancedAnalyticsPlugin.dll
        echo.
        echo =========================================
        echo BUILD SUCCESSFUL!
        echo =========================================
        echo.
        echo Plugin ready for deployment:
        echo   - Location: %cd%\output\AdvancedAnalyticsPlugin.dll
        echo   - Size: %sizeMB% MB
        echo.
        echo Next steps:
        echo   1. Copy output\AdvancedAnalyticsPlugin.dll to REACT CRM's /plugins/ folder
        echo   2. Run REACT CRM application
        echo   3. Navigate to Plugin Management
        echo   4. Enable and execute the plugin
        echo.
    ) else (
        echo WARNING: Could not copy plugin to output folder
    )
) else (
    echo ERROR: Plugin DLL not found at %PLUGIN_DLL%
    pause
    exit /b 1
)

echo.
pause
