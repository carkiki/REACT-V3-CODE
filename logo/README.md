# Logo Folder

This folder contains the application logo/icon.

## How to Use

1. **Place your logo file here**: `logo.png`
   - Recommended size: 256x256 pixels or larger
   - Format: PNG with transparent background (preferred) or JPG
   - Aspect ratio: Square (1:1) works best

2. **The logo will be used for**:
   - Application icon/taskbar icon
   - Window title bar icon
   - About dialog
   - Splash screen (if implemented)

## Icon Conversion (Optional)

If you want to create a Windows `.ico` file from your PNG:

### Online Tools:
- https://convertio.co/png-ico/
- https://icoconvert.com/

### Using ImageMagick (Command Line):
```bash
convert logo.png -define icon:auto-resize=256,128,64,48,32,16 logo.ico
```

## Updating the Project

After placing `logo.png` in this folder:

1. **For immediate use** in forms:
   ```csharp
   this.Icon = new Icon("logo/logo.png");
   ```

2. **To embed in the project**:
   - Right-click project in Visual Studio
   - Add > Existing Item > Select `logo/logo.png`
   - Properties > Build Action > Embedded Resource
   - Then use:
     ```csharp
     this.Icon = new Icon(Assembly.GetExecutingAssembly()
         .GetManifestResourceStream("ReactCRM.logo.logo.png"));
     ```

3. **To set as application icon**:
   - Convert logo.png to logo.ico
   - Right-click project > Properties
   - Application > Resources > Icon > Browse > Select logo.ico

## Current Status

📁 **Folder created and ready**
📄 **Waiting for logo.png** - Please place your logo file here

---

**Note**: Once you add `logo.png`, you can reference it in your application code. The project is already configured to look for the logo in this location.
