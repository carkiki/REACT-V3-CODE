# Advanced Analytics Plugin - Standalone Build

Este es el plugin Advanced Analytics en versión **standalone** (independiente), que puede compilarse desde cualquier ubicación sin necesidad del proyecto principal REACT CRM.

## 🔄 Diferencias con la Versión Original

### Versión Original (en `/Plugins/AdvancedAnalyticsPlugin/`)
- ✅ Usa `ProjectReference` al proyecto principal
- ✅ Se compila junto con REACT CRM
- ✅ Integrado en la solución Visual Studio
- ❌ No puede moverse a otra ubicación

### Versión Standalone (esta carpeta)
- ✅ **Totalmente portable** - puede moverse a cualquier ubicación
- ✅ **Compilación independiente** - no requiere proyecto principal
- ✅ Incluye interface `IReactCrmPlugin` directamente
- ✅ Scripts de build simplificados
- ✅ Genera DLL listo para usar en carpeta `output/`

## 📁 Estructura del Proyecto

```
AdvancedAnalyticsPlugin-Standalone/
├── AdvancedAnalyticsPlugin.cs         # Plugin principal
├── AdvancedAnalyticsPlugin.csproj     # Proyecto standalone
├── FodyWeavers.xml                    # Configuración empaquetado
├── build.bat                          # Build script Windows
├── build.sh                           # Build script Linux/Mac
│
├── Engine/                            # Motor de análisis
│   ├── DataQueryEngine.cs
│   ├── ChartingEngine.cs
│   ├── AnalyticsEngine.cs
│   └── PdfReportGenerator.cs
│
├── Models/                            # Modelos de datos
│   ├── DataSeries.cs
│   ├── ChartConfiguration.cs
│   └── AnalyticsResult.cs
│
├── UI/                                # Interfaz de usuario
│   └── MainAnalyticsForm.cs
│
├── Lib/                               # Dependencias locales
│   └── IReactCrmPlugin.cs             # Interface del plugin
│
├── output/                            # DLL compilado (generado)
│   └── AdvancedAnalyticsPlugin.dll
│
└── Docs/                              # Documentación
    ├── README.md
    ├── QUICKSTART.md
    ├── DEVELOPER.md
    ├── PERFORMANCE.md
    └── CHANGELOG.md
```

## 🚀 Compilación Rápida

### Windows

```batch
cd AdvancedAnalyticsPlugin-Standalone
build.bat
```

### Linux/Mac

```bash
cd AdvancedAnalyticsPlugin-Standalone
chmod +x build.sh
./build.sh
```

## 📋 Proceso de Build

El script de compilación realiza los siguientes pasos:

1. **[1/4] Limpieza**: Elimina carpetas `bin/` y `obj/` anteriores
2. **[2/4] Restauración**: Descarga paquetes NuGet (ScottPlot, QuestPDF, etc.)
3. **[3/4] Compilación**: Compila el plugin en modo Release
4. **[4/4] Verificación**: Valida el DLL y lo copia a `output/`

### Salida del Build

```
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
```

## 📦 DLL Resultante

El build genera un **único archivo DLL** con todas las dependencias empaquetadas:

- **Ubicación**: `output/AdvancedAnalyticsPlugin.dll`
- **Tamaño**: ~15-20 MB
- **Dependencias incluidas**:
  - QuestPDF (generación de PDFs)
  - ScottPlot + ScottPlot.WinForms (gráficas)
  - SkiaSharp (renderizado gráfico)
  - HarfBuzzSharp (fuentes)
  - SixLabors.Fonts (tipografía)

**Dependencias excluidas** (deben estar en REACT CRM):
- Microsoft.Data.Sqlite
- Newtonsoft.Json

## 🔧 Instalación del Plugin

### Paso 1: Compilar el Plugin

```batch
build.bat
```

### Paso 2: Copiar DLL a REACT CRM

Copiar `output/AdvancedAnalyticsPlugin.dll` a la carpeta `/plugins/` de REACT CRM:

```batch
copy output\AdvancedAnalyticsPlugin.dll "C:\REACT CRM\plugins\"
```

O manualmente:
1. Abrir carpeta `output/`
2. Copiar `AdvancedAnalyticsPlugin.dll`
3. Pegar en carpeta `/plugins/` de REACT CRM

### Paso 3: Ejecutar REACT CRM

1. Abrir REACT CRM
2. El plugin se cargará automáticamente
3. Ir a **Gestión de Plugins**
4. Activar "Advanced Analytics & Reporting"
5. Hacer clic en **Ejecutar Plugin**

## 🔍 Diferencias Técnicas

### Archivo .csproj

**Versión original** (ProjectReference):
```xml
<ProjectReference Include="..\..\REACT CRM.csproj">
  <Private>false</Private>
</ProjectReference>
```

**Versión standalone** (sin dependencias):
```xml
<ItemGroup>
  <Compile Include="Lib\IReactCrmPlugin.cs" />
</ItemGroup>
```

### Interface IReactCrmPlugin

La versión standalone incluye el archivo `Lib/IReactCrmPlugin.cs` directamente en el proyecto, copiado del proyecto principal. Esto permite compilar sin necesidad de referenciar el proyecto REACT CRM.

## ⚙️ Configuración de Fody

El archivo `FodyWeavers.xml` configura qué dependencias se empaquetan:

**Incluidas** (se empaquetan en el DLL):
```xml
<IncludeAssemblies>
  QuestPDF
  ScottPlot
  ScottPlot.WinForms
  SkiaSharp
  SkiaSharp.HarfBuzz
  HarfBuzzSharp
  SixLabors.Fonts
</IncludeAssemblies>
```

**Excluidas** (deben estar en REACT CRM):
```xml
<ExcludeAssemblies>
  Microsoft.Data.Sqlite
  Newtonsoft.Json
  System
  mscorlib
</ExcludeAssemblies>
```

## 🛠️ Requisitos del Sistema

### Software
- .NET 10.0 SDK o superior
- Windows 7+ (WinForms requirement)

### Hardware
- Mínimo 4 GB RAM
- 100 MB espacio en disco
- Procesador moderno (Intel i5 o equivalente)

## 📝 Modificar y Recompilar

### Editar Código

1. Abrir archivos `.cs` en cualquier editor (Visual Studio, VS Code, etc.)
2. Hacer cambios necesarios
3. Guardar archivos

### Recompilar

```batch
build.bat
```

El DLL actualizado se generará en `output/AdvancedAnalyticsPlugin.dll`

## 🧪 Testing

### Build de Prueba

Para compilar en modo Debug (con símbolos de depuración):

```batch
dotnet build AdvancedAnalyticsPlugin.csproj -c Debug
```

### Verificar Dependencias

Para ver qué DLLs están empaquetados:

```batch
# Listar archivos en el DLL (requiere 7-Zip o similar)
7z l output\AdvancedAnalyticsPlugin.dll
```

## 🔄 Actualizar Interface

Si la interface `IReactCrmPlugin` cambia en el proyecto principal:

1. Copiar el nuevo archivo desde REACT CRM:
```batch
copy "..\Plugins\IReactCrmPlugin.cs" "Lib\IReactCrmPlugin.cs"
```

2. Recompilar:
```batch
build.bat
```

## 🐛 Troubleshooting

### Error: ".NET SDK is not installed"

**Solución**: Instalar .NET 10.0 SDK desde https://dotnet.microsoft.com/download

### Error: "Failed to restore NuGet packages"

**Solución**:
1. Verificar conexión a Internet
2. Limpiar caché de NuGet:
```batch
dotnet nuget locals all --clear
```
3. Intentar de nuevo:
```batch
dotnet restore AdvancedAnalyticsPlugin.csproj
```

### Error: "Plugin DLL not found"

**Solución**:
1. Verificar que no hay errores de compilación
2. Revisar carpeta `bin/Release/net10.0-windows7.0/`
3. Verificar que Fody se ejecutó correctamente (buscar mensajes en output)

### Warning: "Could not copy plugin to output folder"

**Solución**:
1. Cerrar cualquier programa que esté usando el DLL
2. Verificar permisos de escritura en carpeta
3. Copiar manualmente desde `bin/Release/net10.0-windows7.0/`

## 📦 Distribución

### Empaquetar para Distribución

El archivo `output/AdvancedAnalyticsPlugin.dll` es todo lo necesario:

```batch
# Crear ZIP para distribución
7z a AdvancedAnalyticsPlugin-v1.0.0.zip output\AdvancedAnalyticsPlugin.dll README.md QUICKSTART.md
```

### Instrucciones para Usuarios Finales

1. Descargar `AdvancedAnalyticsPlugin.dll`
2. Copiar a carpeta `/plugins/` de REACT CRM
3. Reiniciar REACT CRM
4. Activar plugin desde Gestión de Plugins

## 🚀 Ventajas de la Versión Standalone

1. ✅ **Portabilidad**: Puede moverse a cualquier ubicación
2. ✅ **Independencia**: No requiere proyecto principal para compilar
3. ✅ **Simplicidad**: Scripts de build más simples
4. ✅ **Distribución**: Fácil de compartir y distribuir
5. ✅ **Desarrollo**: Desarrollo independiente sin afectar proyecto principal
6. ✅ **CI/CD**: Fácil integración en pipelines de compilación

## 📚 Documentación Adicional

- **README.md**: Guía completa de características
- **QUICKSTART.md**: Guía rápida de 5 minutos
- **DEVELOPER.md**: Guía para desarrolladores
- **PERFORMANCE.md**: Optimizaciones y benchmarks
- **CHANGELOG.md**: Historial de cambios

## 🔗 Referencias

- **Costura.Fody**: https://github.com/Fody/Costura
- **.NET SDK**: https://dotnet.microsoft.com/download
- **ScottPlot**: https://scottplot.net/
- **QuestPDF**: https://www.questpdf.com/

---

**Versión**: 1.0.0 (Standalone)
**Última actualización**: 3 de Diciembre, 2024
**Compatibilidad**: .NET 10.0, Windows 7+
