#!/bin/bash

# Advanced Analytics Plugin - Build Script
# This script builds the plugin and packages it as a single DLL

echo "========================================="
echo "Advanced Analytics Plugin - Build Script"
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

# Build main CRM project first
echo "📦 Building main REACT CRM project..."
cd ../..
dotnet build "REACT CRM.csproj" -c Release --nologo

if [ $? -ne 0 ]; then
    echo "❌ Error building main CRM project"
    exit 1
fi

echo "✅ Main CRM project built successfully"
echo ""

# Build plugin
echo "📦 Building Advanced Analytics Plugin..."
cd Plugins/AdvancedAnalyticsPlugin
dotnet build AdvancedAnalyticsPlugin.csproj -c Release --nologo

if [ $? -ne 0 ]; then
    echo "❌ Error building plugin"
    exit 1
fi

echo "✅ Plugin built successfully"
echo ""

# Check if merged DLL exists
PLUGIN_DLL="bin/Release/net10.0-windows7.0/AdvancedAnalyticsPlugin.dll"

if [ -f "$PLUGIN_DLL" ]; then
    echo "✅ Plugin DLL created successfully:"
    echo "   $PLUGIN_DLL"
    echo ""

    # Get file size
    SIZE=$(du -h "$PLUGIN_DLL" | cut -f1)
    echo "📊 File size: $SIZE"
    echo ""

    # Create plugins directory if it doesn't exist
    mkdir -p ../../plugins

    # Copy to plugins directory
    echo "📋 Copying plugin to /plugins/ directory..."
    cp "$PLUGIN_DLL" ../../plugins/

    if [ $? -eq 0 ]; then
        echo "✅ Plugin copied successfully to /plugins/"
        echo ""
        echo "🎉 Build complete!"
        echo ""
        echo "Next steps:"
        echo "1. Run REACT CRM application"
        echo "2. Navigate to Plugin Management"
        echo "3. The plugin should appear automatically"
        echo "4. Enable and execute the plugin"
    else
        echo "⚠️  Warning: Could not copy plugin to /plugins/ directory"
        echo "   Please copy manually from: $PLUGIN_DLL"
    fi
else
    echo "❌ Error: Plugin DLL not found at $PLUGIN_DLL"
    exit 1
fi

echo ""
echo "========================================="
