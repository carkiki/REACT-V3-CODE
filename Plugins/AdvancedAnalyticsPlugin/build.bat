@echo off
REM Advanced Analytics Plugin - Build Script for Windows
REM This script builds the plugin and packages it as a single DLL

echo =========================================
echo Advanced Analytics Plugin - Build Script
echo =========================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed
    echo Please install .NET 10.0 SDK or higher from:
    echo https://dotnet.microsoft.com/download
    exit /b 1
)

echo ✓ .NET SDK found
dotnet --version
echo.

REM Build main CRM project first
echo Building main REACT CRM project...
cd ..\..
dotnet build "REACT CRM.csproj" -c Release --nologo

if errorlevel 1 (
    echo ERROR: Failed to build main CRM project
    exit /b 1
)

echo ✓ Main CRM project built successfully
echo.

REM Build plugin
echo Building Advanced Analytics Plugin...
cd Plugins\AdvancedAnalyticsPlugin
dotnet build AdvancedAnalyticsPlugin.csproj -c Release --nologo

if errorlevel 1 (
    echo ERROR: Failed to build plugin
    exit /b 1
)

echo ✓ Plugin built successfully
echo.

REM Check if merged DLL exists
set PLUGIN_DLL=bin\Release\net10.0-windows7.0\AdvancedAnalyticsPlugin.dll

if exist "%PLUGIN_DLL%" (
    echo ✓ Plugin DLL created successfully:
    echo    %PLUGIN_DLL%
    echo.

    REM Create plugins directory if it doesn't exist
    if not exist "..\..\plugins" mkdir "..\..\plugins"

    REM Copy to plugins directory
    echo Copying plugin to \plugins\ directory...
    copy /Y "%PLUGIN_DLL%" "..\..\plugins\"

    if errorlevel 0 (
        echo ✓ Plugin copied successfully to \plugins\
        echo.
        echo Build complete!
        echo.
        echo Next steps:
        echo 1. Run REACT CRM application
        echo 2. Navigate to Plugin Management
        echo 3. The plugin should appear automatically
        echo 4. Enable and execute the plugin
    ) else (
        echo WARNING: Could not copy plugin to \plugins\ directory
        echo Please copy manually from: %PLUGIN_DLL%
    )
) else (
    echo ERROR: Plugin DLL not found at %PLUGIN_DLL%
    exit /b 1
)

echo.
echo =========================================
pause
