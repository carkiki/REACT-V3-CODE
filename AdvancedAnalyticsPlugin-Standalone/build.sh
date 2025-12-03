#!/bin/bash

# Advanced Analytics Plugin - Standalone Build Script
# This script builds the plugin as a standalone DLL

echo "========================================="
echo "Advanced Analytics Plugin - Standalone Build"
echo "========================================="
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo "❌ Error: .NET SDK is not installed"
    echo "Please install .NET 10.0 SDK or higher from:"
    echo "https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET SDK found"
dotnet --version
echo ""

# Clean previous build
echo "[1/4] Cleaning previous build..."
rm -rf bin obj
echo "✅ Clean complete"
echo ""

# Restore NuGet packages
echo "[2/4] Restoring NuGet packages..."
dotnet restore AdvancedAnalyticsPlugin.csproj --nologo

if [ $? -ne 0 ]; then
    echo "❌ Error: Failed to restore NuGet packages"
    exit 1
fi

echo "✅ NuGet packages restored"
echo ""

# Build plugin in Release mode
echo "[3/4] Building Advanced Analytics Plugin..."
dotnet build AdvancedAnalyticsPlugin.csproj -c Release --nologo --no-restore

if [ $? -ne 0 ]; then
    echo "❌ Error: Failed to build plugin"
    exit 1
fi

echo "✅ Plugin built successfully"
echo ""

# Check if merged DLL exists
echo "[4/4] Verifying output..."
PLUGIN_DLL="bin/Release/net10.0-windows7.0/AdvancedAnalyticsPlugin.dll"

if [ -f "$PLUGIN_DLL" ]; then
    echo "✅ Plugin DLL created successfully:"
    echo "   $PLUGIN_DLL"
    echo ""

    # Get file size
    SIZE=$(du -h "$PLUGIN_DLL" | cut -f1)
    echo "📊 File size: $SIZE"
    echo ""

    # Create output directory
    mkdir -p output

    # Copy to output directory
    echo "📋 Copying plugin to output folder..."
    cp "$PLUGIN_DLL" output/AdvancedAnalyticsPlugin.dll

    if [ $? -eq 0 ]; then
        echo "✅ Plugin copied to: output/AdvancedAnalyticsPlugin.dll"
        echo ""
        echo "========================================="
        echo "BUILD SUCCESSFUL!"
        echo "========================================="
        echo ""
        echo "Plugin ready for deployment:"
        echo "  - Location: $(pwd)/output/AdvancedAnalyticsPlugin.dll"
        echo "  - Size: $SIZE"
        echo ""
        echo "Next steps:"
        echo "  1. Copy output/AdvancedAnalyticsPlugin.dll to REACT CRM's /plugins/ folder"
        echo "  2. Run REACT CRM application"
        echo "  3. Navigate to Plugin Management"
        echo "  4. Enable and execute the plugin"
        echo ""
    else
        echo "⚠️  Warning: Could not copy plugin to output folder"
        echo "   Please copy manually from: $PLUGIN_DLL"
    fi
else
    echo "❌ Error: Plugin DLL not found at $PLUGIN_DLL"
    exit 1
fi

echo ""
echo "========================================="
