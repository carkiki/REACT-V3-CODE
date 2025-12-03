# Build Instructions - Advanced Analytics Plugin (Standalone)

## 🎯 Quick Build (Windows)

```batch
cd AdvancedAnalyticsPlugin-Standalone
build.bat
```

**Result**: `output/AdvancedAnalyticsPlugin.dll` ready to use!

---

## 📋 Detailed Build Process

### Prerequisites

1. **.NET 10.0 SDK** installed
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation:
     ```batch
     dotnet --version
     ```
     Should output: `10.0.100` or higher

2. **Windows 7 or higher** (WinForms requirement)

3. **Internet connection** (for first build to download NuGet packages)

### Step-by-Step Build

#### Method 1: Using Build Script (Recommended)

**Windows:**
```batch
cd AdvancedAnalyticsPlugin-Standalone
build.bat
```

**Linux/Mac:**
```bash
cd AdvancedAnalyticsPlugin-Standalone
chmod +x build.sh
./build.sh
```

**Expected Output:**
```
=========================================
Advanced Analytics Plugin - Standalone Build
=========================================

[OK] .NET SDK found
10.0.100

[1/4] Cleaning previous build...
[OK] Clean complete

[2/4] Restoring NuGet packages...
[OK] NuGet packages restored

[3/4] Building Advanced Analytics Plugin...
[OK] Plugin built successfully

[4/4] Verifying output...
[OK] Plugin DLL created successfully:
     bin\Release\net10.0-windows7.0\AdvancedAnalyticsPlugin.dll

[INFO] File size: 15 MB

[OK] Plugin copied to: output\AdvancedAnalyticsPlugin.dll

=========================================
BUILD SUCCESSFUL!
=========================================

Plugin ready for deployment:
  - Location: ...\AdvancedAnalyticsPlugin-Standalone\output\AdvancedAnalyticsPlugin.dll
  - Size: 15 MB

Next steps:
  1. Copy output\AdvancedAnalyticsPlugin.dll to REACT CRM's /plugins/ folder
  2. Run REACT CRM application
  3. Navigate to Plugin Management
  4. Enable and execute the plugin
```

#### Method 2: Using .NET CLI Manually

**Step 1: Clean**
```batch
dotnet clean AdvancedAnalyticsPlugin.csproj
```

**Step 2: Restore**
```batch
dotnet restore AdvancedAnalyticsPlugin.csproj
```

**Step 3: Build**
```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Release
```

**Step 4: Copy DLL**
```batch
mkdir output
copy bin\Release\net10.0-windows7.0\AdvancedAnalyticsPlugin.dll output\
```

#### Method 3: Using Visual Studio 2022

1. Open `AdvancedAnalyticsPlugin.csproj` in Visual Studio
2. Select **Release** configuration
3. Right-click project → **Build**
4. DLL will be in `bin\Release\net10.0-windows7.0\`

---

## 📦 Build Output

### Files Generated

```
AdvancedAnalyticsPlugin-Standalone/
├── bin/
│   └── Release/
│       └── net10.0-windows7.0/
│           ├── AdvancedAnalyticsPlugin.dll  ← MAIN OUTPUT
│           ├── AdvancedAnalyticsPlugin.pdb
│           └── ... (other build artifacts)
│
├── obj/  (build intermediates)
│
└── output/
    └── AdvancedAnalyticsPlugin.dll  ← COPIED HERE BY SCRIPT
```

### DLL Details

- **Name**: `AdvancedAnalyticsPlugin.dll`
- **Size**: ~15-20 MB
- **Target Framework**: .NET 10.0 (net10.0-windows7.0)
- **Dependencies Embedded**: Yes (Costura.Fody)

**Embedded Dependencies:**
- QuestPDF 2024.12.3
- ScottPlot 5.0.54
- ScottPlot.WinForms 5.0.54
- SkiaSharp + SkiaSharp.HarfBuzz
- HarfBuzzSharp
- SixLabors.Fonts

**External Dependencies** (must be in REACT CRM):
- Microsoft.Data.Sqlite 10.0.0
- Newtonsoft.Json 13.0.4

---

## 🚀 Deployment

### Copy to REACT CRM

**Option 1: Automatic (via script)**
The build script automatically copies to `output/`. Then copy to REACT CRM:
```batch
copy output\AdvancedAnalyticsPlugin.dll "C:\Path\To\REACT-CRM\plugins\"
```

**Option 2: Manual**
1. Navigate to `AdvancedAnalyticsPlugin-Standalone\output\`
2. Copy `AdvancedAnalyticsPlugin.dll`
3. Paste into REACT CRM's `/plugins/` folder

### Verify Installation

1. Run REACT CRM
2. Check debug output for:
   ```
   [PluginManager] ✓ Loaded: Advanced Analytics & Reporting v1.0.0
   ```
3. Go to Plugin Management
4. "Advanced Analytics & Reporting" should appear in the list

---

## 🔧 Troubleshooting

### Common Build Errors

#### Error: "SDK 'Microsoft.NET.Sdk' not found"

**Cause**: .NET SDK not installed

**Solution**:
1. Download .NET 10.0 SDK from https://dotnet.microsoft.com/download
2. Install and restart terminal
3. Verify with `dotnet --version`

#### Error: "Unable to resolve package 'QuestPDF'"

**Cause**: No internet connection or NuGet cache issue

**Solution**:
1. Check internet connection
2. Clear NuGet cache:
   ```batch
   dotnet nuget locals all --clear
   ```
3. Try restore again:
   ```batch
   dotnet restore AdvancedAnalyticsPlugin.csproj
   ```

#### Error: "Fody: An unhandled exception occurred"

**Cause**: Fody weaving failed

**Solution**:
1. Clean build:
   ```batch
   dotnet clean AdvancedAnalyticsPlugin.csproj
   rd /s /q bin obj
   ```
2. Restore packages:
   ```batch
   dotnet restore AdvancedAnalyticsPlugin.csproj
   ```
3. Build again:
   ```batch
   dotnet build AdvancedAnalyticsPlugin.csproj -c Release
   ```

#### Error: "Could not load file or assembly 'SkiaSharp'"

**Cause**: Fody didn't embed dependencies correctly

**Solution**:
1. Verify `FodyWeavers.xml` exists
2. Check that `Costura.Fody` package is installed
3. Clean and rebuild

#### Warning: "Assembly binding redirect"

**Cause**: Version mismatch in dependencies

**Solution**: Ignore if build succeeds. The DLL will work correctly.

### Runtime Errors

#### Error: "Could not load type 'IReactCrmPlugin'"

**Cause**: Interface mismatch between plugin and REACT CRM

**Solution**:
1. Copy latest `IReactCrmPlugin.cs` from REACT CRM:
   ```batch
   copy "..\Plugins\IReactCrmPlugin.cs" "Lib\IReactCrmPlugin.cs"
   ```
2. Rebuild plugin

#### Error: "FileNotFoundException: Microsoft.Data.Sqlite"

**Cause**: Missing dependency in REACT CRM

**Solution**: Ensure REACT CRM has `Microsoft.Data.Sqlite` version 10.0.0 installed

#### Error: "Could not load file or assembly 'Newtonsoft.Json'"

**Cause**: Missing dependency in REACT CRM

**Solution**: Ensure REACT CRM has `Newtonsoft.Json` version 13.0.4 or compatible

---

## 🧪 Testing the Build

### Quick Test

1. Build the DLL
2. Check file size:
   ```batch
   dir output\AdvancedAnalyticsPlugin.dll
   ```
   Should be ~15-20 MB

3. Verify it's a valid .NET assembly:
   ```batch
   dotnet --info output\AdvancedAnalyticsPlugin.dll
   ```

### Full Test

1. Copy DLL to REACT CRM `/plugins/` folder
2. Run REACT CRM
3. Check Plugin Management
4. Enable plugin
5. Execute plugin
6. Verify UI appears
7. Try basic analysis with test data

---

## 📊 Build Variants

### Release Build (Default)

**Command:**
```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Release
```

**Characteristics:**
- Optimized code
- No debug symbols
- Smaller DLL size
- Better performance
- For production use

### Debug Build

**Command:**
```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Debug
```

**Characteristics:**
- Unoptimized code
- Includes debug symbols (.pdb)
- Larger DLL size
- Easier debugging
- For development only

### Verbose Build

**Command:**
```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Release -v detailed
```

Shows all build steps and Fody weaving process

---

## 🔄 Rebuilding

### When to Rebuild

- After modifying any `.cs` source file
- After updating NuGet packages
- After changing `FodyWeavers.xml`
- After updating `.csproj` configuration

### Clean Rebuild

```batch
dotnet clean AdvancedAnalyticsPlugin.csproj
rd /s /q bin obj
dotnet build AdvancedAnalyticsPlugin.csproj -c Release
```

---

## 📝 Customization

### Modify Source Code

1. Edit any `.cs` file in the project
2. Save changes
3. Run `build.bat`
4. New DLL will be in `output/`

### Update Dependencies

Edit `AdvancedAnalyticsPlugin.csproj`:

```xml
<PackageReference Include="ScottPlot.WinForms" Version="5.0.55" />
```

Then rebuild:
```batch
build.bat
```

### Add New Files

1. Create new `.cs` file in appropriate folder (Engine/, Models/, UI/)
2. File will be automatically included in build
3. Run `build.bat`

---

## 🚀 Advanced Build Options

### Build for Different Framework

**Edit `.csproj`:**
```xml
<TargetFramework>net9.0-windows7.0</TargetFramework>
```

**Then rebuild:**
```batch
build.bat
```

### Multi-Target Build

**Edit `.csproj`:**
```xml
<TargetFrameworks>net10.0-windows7.0;net9.0-windows7.0</TargetFrameworks>
```

**Build both:**
```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Release
```

### Disable Fody Weaving (for debugging)

**Comment out in `.csproj`:**
```xml
<!--
<PackageReference Include="Costura.Fody" Version="5.8.0">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
-->
```

---

## 📦 Distribution

### Create Release Package

**Option 1: ZIP file**
```batch
7z a AdvancedAnalyticsPlugin-v1.0.0.zip output\AdvancedAnalyticsPlugin.dll README.md QUICKSTART.md
```

**Option 2: Self-extracting**
```batch
7z a -sfx AdvancedAnalyticsPlugin-v1.0.0.exe output\AdvancedAnalyticsPlugin.dll
```

### Installation Package for Users

**Include in distribution:**
- `output/AdvancedAnalyticsPlugin.dll` (required)
- `README.md` (recommended)
- `QUICKSTART.md` (recommended)
- Installation instructions

**User instructions:**
1. Download `AdvancedAnalyticsPlugin.dll`
2. Copy to `C:\Program Files\REACT CRM\plugins\`
3. Restart REACT CRM
4. Enable plugin in Plugin Management

---

## 📞 Support

### Build Issues

If you encounter build errors:

1. **Check prerequisites**:
   - .NET 10.0 SDK installed
   - Internet connection available
   - Sufficient disk space (500 MB)

2. **Try clean rebuild**:
   ```batch
   rd /s /q bin obj
   build.bat
   ```

3. **Check build logs**: Look for specific error messages in output

4. **Verify files**: Ensure all source files are present

### Getting Help

- **README.md**: General plugin documentation
- **DEVELOPER.md**: Developer guide
- **PERFORMANCE.md**: Performance optimization guide
- **README-STANDALONE.md**: Standalone-specific information

---

## ✅ Build Checklist

Before distributing the plugin:

- [ ] Clean build completed without errors
- [ ] DLL size is ~15-20 MB
- [ ] File exists at `output/AdvancedAnalyticsPlugin.dll`
- [ ] Tested in REACT CRM
- [ ] Plugin loads successfully
- [ ] Plugin executes without errors
- [ ] All features work correctly
- [ ] Documentation updated
- [ ] Version number updated (if applicable)

---

**Version**: 1.0.0
**Last Updated**: 3 de Diciembre, 2024
**Build System**: .NET 10.0 + Costura.Fody
