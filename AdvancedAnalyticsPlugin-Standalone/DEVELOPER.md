# Developer Guide - Advanced Analytics Plugin

Guía técnica para desarrolladores que deseen modificar o extender el plugin.

## 🏗️ Estructura del Proyecto

```
AdvancedAnalyticsPlugin/
├── AdvancedAnalyticsPlugin.cs          # Clase principal del plugin
├── Engine/
│   ├── DataQueryEngine.cs              # Motor de consultas SQL/SQLite
│   ├── ChartingEngine.cs               # Motor de visualización (ScottPlot)
│   ├── AnalyticsEngine.cs              # Algoritmos de análisis avanzado
│   └── PdfReportGenerator.cs           # Generador de PDFs (QuestPDF)
├── Models/
│   ├── DataSeries.cs                   # Serie de datos con estadísticas
│   ├── ChartConfiguration.cs           # Configuración de gráficas
│   └── AnalyticsResult.cs              # Resultado del análisis
├── UI/
│   └── MainAnalyticsForm.cs            # Formulario principal WinForms
├── AdvancedAnalyticsPlugin.csproj      # Archivo de proyecto
├── FodyWeavers.xml                     # Configuración de Costura.Fody
├── README.md                           # Documentación de usuario
├── DEVELOPER.md                        # Esta guía
├── build.sh / build.bat                # Scripts de compilación
└── .gitignore                          # Exclusiones de Git
```

## 🔧 Tecnologías Utilizadas

### Frameworks y Librerías

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 10.0 | Framework principal |
| Windows Forms | 10.0 | Interfaz de usuario |
| ScottPlot | 5.0.54 | Visualización de gráficas |
| QuestPDF | 2024.12.3 | Generación de PDFs |
| Microsoft.Data.Sqlite | 10.0.0 | Acceso a base de datos |
| Newtonsoft.Json | 13.0.4 | Serialización JSON |
| Costura.Fody | 5.8.0 | Empaquetado de DLLs |

### Dependencias Indirectas

- **SkiaSharp**: Renderizado de gráficos (usado por ScottPlot)
- **HarfBuzzSharp**: Procesamiento de texto (usado por SkiaSharp)
- **SixLabors.Fonts**: Fuentes tipográficas (usado por QuestPDF)

## 🎯 Arquitectura

### Patrón de Diseño

El plugin sigue el patrón **Separation of Concerns** (SoC):

1. **Engine Layer**: Lógica de negocio y procesamiento
2. **Models Layer**: Estructuras de datos
3. **UI Layer**: Presentación e interacción

### Flujo de Datos

```
Usuario → UI Form → DataQueryEngine → SQLite Database
                        ↓
                  DataSeries Models
                        ↓
                  AnalyticsEngine → Insights
                        ↓
                  ChartingEngine → Gráficas
                        ↓
                  PdfReportGenerator → PDF File
```

## 💻 Guía de Desarrollo

### Agregar un Nuevo Tipo de Gráfica

1. **Actualizar el enum en `Models/DataSeries.cs`:**
```csharp
public enum SeriesType
{
    Line,
    Bar,
    Area,
    Scatter,
    Candlestick,
    YourNewType  // ← Agregar aquí
}
```

2. **Implementar el renderizado en `Engine/ChartingEngine.cs`:**
```csharp
private void RenderSeries(DataSeries series)
{
    // ...
    case SeriesType.YourNewType:
        RenderYourNewTypeSeries(series, color);
        break;
}

private void RenderYourNewTypeSeries(DataSeries series, Color color)
{
    // Implementación usando ScottPlot
    // Ejemplo: _plot.Add.YourPlotType(...)
}
```

3. **Agregar opción en UI en `UI/MainAnalyticsForm.cs`:**
```csharp
chartTypeComboBox.Items.Add("Tu Nuevo Tipo");
```

### Agregar un Nuevo Algoritmo de Análisis

1. **Crear método en `Engine/AnalyticsEngine.cs`:**
```csharp
public DataSeries CalculateYourAlgorithm(DataSeries series, int parameter)
{
    var result = new DataSeries
    {
        Name = $"{series.Name} (YourAlgorithm{parameter})",
        Type = SeriesType.Line
    };

    // Tu algoritmo aquí
    foreach (var point in series.Points)
    {
        double value = YourCalculation(point.Value, parameter);
        result.Points.Add(new DataPoint
        {
            Label = point.Label,
            Value = value
        });
    }

    return result;
}
```

2. **Agregar checkbox en UI:**
```csharp
calculateYourAlgorithmCheckBox = new CheckBox
{
    Text = "Calcular [Tu Algoritmo]",
    Location = new Point(10, yPosition),
    Size = new Size(300, 20),
    Checked = false
};
panel.Controls.Add(calculateYourAlgorithmCheckBox);
```

3. **Ejecutar en `ExecuteButton_Click`:**
```csharp
if (calculateYourAlgorithmCheckBox.Checked && _currentResult.Series.Any())
{
    var result = _analyticsEngine.CalculateYourAlgorithm(_currentResult.Series[0], parameter);
    _currentResult.Series.Add(result);
}
```

### Agregar un Nuevo Tipo de Insight

1. **Actualizar enum en `Models/AnalyticsResult.cs`:**
```csharp
public enum InsightType
{
    Trend,
    Anomaly,
    Correlation,
    Seasonality,
    Threshold,
    Pattern,
    Recommendation,
    YourNewInsightType  // ← Agregar aquí
}
```

2. **Crear detector en `Engine/AnalyticsEngine.cs`:**
```csharp
private Insight? DetectYourInsight(DataSeries series)
{
    // Tu lógica de detección
    if (conditionMet)
    {
        return new Insight
        {
            Type = InsightType.YourNewInsightType,
            Title = "Tu Insight Detectado",
            Description = "Descripción detallada",
            Severity = InsightSeverity.Info
        };
    }
    return null;
}
```

3. **Llamar en `GenerateInsights`:**
```csharp
public List<Insight> GenerateInsights(AnalyticsResult result)
{
    var insights = new List<Insight>();

    foreach (var series in result.Series)
    {
        var yourInsight = DetectYourInsight(series);
        if (yourInsight != null)
            insights.Add(yourInsight);
    }

    return insights;
}
```

### Personalizar el Reporte PDF

El generador de PDF usa **QuestPDF** que tiene un DSL fluido para crear documentos.

Ejemplo de agregar una nueva sección:

```csharp
private void ComposeYourCustomSection(IContainer container)
{
    container.Column(column =>
    {
        column.Item().Text("Tu Sección Personalizada")
            .FontSize(16)
            .Bold()
            .FontColor(Colors.Blue.Darken2);

        column.Item().PaddingTop(8).PaddingBottom(4).LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);

        column.Item().PaddingTop(8).Text("Contenido de tu sección");

        // Agregar tabla
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Element(CellStyle).Text("Header 1");
            table.Cell().Element(CellStyle).Text("Header 2");

            table.Cell().Element(CellStyle).Text("Data 1");
            table.Cell().Element(CellStyle).Text("Data 2");
        });
    });
}
```

Luego llamar en `ComposeContent`:

```csharp
private void ComposeContent(IContainer container)
{
    container.PaddingVertical(10).Column(column =>
    {
        column.Spacing(15);

        column.Item().Element(ComposeExecutiveSummary);
        column.Item().Element(ComposeStatistics);
        column.Item().Element(ComposeYourCustomSection);  // ← Agregar aquí
        // ...
    });
}
```

## 🔍 Debugging

### Habilitar Logs

Agregar líneas de debug en métodos clave:

```csharp
System.Diagnostics.Debug.WriteLine($"[AnalyticsEngine] Calculating trend for {series.Name}");
System.Diagnostics.Debug.WriteLine($"[AnalyticsEngine] Slope: {slope}, R²: {rSquared}");
```

Ver logs en **Output Window** de Visual Studio (Debug → Windows → Output).

### Breakpoints Recomendados

- `ExecuteButton_Click` - Ver configuración de consulta
- `ExecuteQuery` - Ver SQL generado
- `RenderChart` - Ver configuración de gráfica
- `GenerateInsights` - Ver insights detectados

### Probar sin UI

Crear proyecto de pruebas unitarias:

```csharp
[TestClass]
public class AnalyticsEngineTests
{
    [TestMethod]
    public void TestTrendAnalysis()
    {
        var engine = new AnalyticsEngine();
        var series = new DataSeries
        {
            Points = new List<DataPoint>
            {
                new() { Value = 10 },
                new() { Value = 20 },
                new() { Value = 30 }
            }
        };

        var trend = engine.AnalyzeTrend(series);

        Assert.AreEqual(TrendDirection.Increasing, trend.Direction);
        Assert.IsTrue(trend.Slope > 0);
    }
}
```

## 🚀 Optimizaciones

### Performance

1. **Consultas SQL**: Usar índices en campos frecuentemente consultados
2. **Caching**: Cachear resultados de consultas pesadas
3. **Async/Await**: Ya implementado en `ExecuteButton_Click`
4. **Lazy Loading**: Cargar datos solo cuando se necesiten

### Memoria

1. **Dispose Pattern**: Liberar recursos en `Cleanup()`
2. **Weak References**: Para gráficas grandes
3. **Streaming**: Para archivos PDF grandes

### Código

```csharp
// ✅ Bueno: Usar LINQ eficientemente
var result = data.Where(x => x.Value > 0)
                 .Select(x => x.Value)
                 .ToList();

// ❌ Malo: Múltiples iteraciones
var filtered = data.Where(x => x.Value > 0).ToList();
var values = filtered.Select(x => x.Value).ToList();
```

## 🧪 Testing

### Manual Testing Checklist

- [ ] Plugin carga correctamente
- [ ] Todos los campos nativos aparecen
- [ ] Custom fields aparecen correctamente
- [ ] Gráficas se renderizan sin errores
- [ ] Cada tipo de gráfica funciona
- [ ] Cada estilo visual se aplica
- [ ] Moving average se calcula correctamente
- [ ] Trend line se dibuja correctamente
- [ ] Insights se generan
- [ ] PDF se genera sin errores
- [ ] PDF contiene todas las secciones
- [ ] Imágenes en PDF están en buena calidad

### Datos de Prueba

Crear clientes de prueba con:
- Valores numéricos variados
- Fechas distribuidas en el tiempo
- Custom fields con diferentes tipos
- Algunos valores nulos
- Valores extremos (outliers)

## 🐛 Common Issues

### Error: "QuestPDF License Required"

```csharp
// Agregar en constructor de PdfReportGenerator
QuestPDF.Settings.License = LicenseType.Community;
```

### Error: "Assembly not found"

Verificar que Costura.Fody embebió correctamente:
1. Revisar `FodyWeavers.xml`
2. Limpiar y recompilar
3. Verificar logs de compilación

### Gráfica no se renderiza

```csharp
// Asegurarse de llamar Refresh
_formsPlot?.Refresh();
```

### PDF sin imágenes

```csharp
// Convertir bitmap correctamente
using var ms = new MemoryStream();
bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
return ms.ToArray();
```

## 📚 Recursos

### ScottPlot
- Documentación: https://scottplot.net/
- Cookbook: https://scottplot.net/cookbook/5.0/
- GitHub: https://github.com/ScottPlot/ScottPlot

### QuestPDF
- Documentación: https://www.questpdf.com/
- API Reference: https://www.questpdf.com/api-reference/
- GitHub: https://github.com/QuestPDF/QuestPDF

### SQLite
- Documentación: https://www.sqlite.org/docs.html
- SQL Reference: https://www.sqlite.org/lang.html

## 🤝 Contributing

### Código Style

- **Naming**: PascalCase para públicos, camelCase para privados
- **Comments**: XML docs en métodos públicos
- **Formatting**: K&R style braces
- **Line Length**: Máximo 120 caracteres

### Pull Request Checklist

- [ ] Código compila sin warnings
- [ ] Tests agregados para nueva funcionalidad
- [ ] README actualizado si es necesario
- [ ] No hay dependencias innecesarias
- [ ] Performance no se ve afectado negativamente

## 📞 Support

Para preguntas o issues:
1. Revisar esta guía y README.md
2. Buscar en issues cerrados
3. Crear nuevo issue con:
   - Descripción del problema
   - Pasos para reproducir
   - Logs relevantes
   - Versión del plugin

---

**Happy Coding! 🚀**
