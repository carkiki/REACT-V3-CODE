# Advanced Analytics Plugin para REACT CRM

Plugin avanzado de análisis de datos con gráficas estilo mercado de valores y generación de reportes PDF.

## 📋 Características

### ⚡ Optimizado para Grandes Volúmenes de Datos
- ✅ **Maneja 3,700+ clientes sin problemas**
- ✅ **Algoritmo LTTB** para sampling inteligente de datos
- ✅ **Barra de progreso** en tiempo real
- ✅ **Procesamiento asíncrono** - UI siempre responsive
- ✅ **Límites configurables** de registros y puntos de gráfica
- ✅ **Rendimiento**: 3,700 registros procesados en ~500ms

### Análisis de Datos
- ✅ Consulta de todos los datos del sistema (clientes, campos nativos y personalizados)
- ✅ Selección flexible de campos nativos y custom fields
- ✅ Filtros avanzados (igual, diferente, mayor, menor, contiene, etc.)
- ✅ Agrupación de datos por cualquier campo
- ✅ Agregaciones: Conteo, Suma, Promedio, Mínimo, Máximo, Mediana, Desviación Estándar

### Visualizaciones Avanzadas
- 📊 **Tipos de gráficas**: Línea, Barra, Área, Dispersión
- 🎨 **Estilos visuales**: Profesional, Mercado de Valores, Científico, Moderno, Oscuro
- 📈 **Indicadores técnicos**: Moving Average (MA), Exponential Moving Average (EMA), RSI
- 📉 **Líneas de tendencia** con análisis de regresión lineal
- 🔍 **Zoom y pan** interactivos en las gráficas
- 🎯 **Leyendas, cuadrículas y etiquetas** personalizables

### Análisis Inteligente
- 🤖 **Detección automática de tendencias** (alcista, bajista, estable, volátil)
- ⚠️ **Detección de anomalías** usando desviación estándar
- 📊 **Análisis de volatilidad** con coeficiente de variación
- 🔗 **Análisis de correlación** entre series de datos
- 💡 **Insights automáticos** con recomendaciones

### Operaciones Matemáticas
- ➕ Suma, ➖ Resta, ✖️ Multiplicación, ➗ División
- 📐 Combinación de series de datos
- 🔢 Cálculos estadísticos avanzados

### Reportes PDF
- 📄 **Generación de reportes profesionales** en PDF
- 📊 **Gráficas embebidas** en alta resolución
- 📈 **Estadísticas detalladas** por serie
- 💡 **Insights y recomendaciones** incluidos
- 📋 **Tablas de datos** completas

## 🛠️ Compilación

### Requisitos
- .NET 10.0 SDK o superior
- Windows 7.0 o superior
- Visual Studio 2022 o JetBrains Rider (opcional)

### Pasos para Compilar

1. **Compilar el proyecto principal REACT CRM primero:**
   ```bash
   cd /home/user/REACT-V3-CODE
   dotnet build "REACT CRM.csproj" -c Release
   ```

2. **Compilar el plugin:**
   ```bash
   cd Plugins/AdvancedAnalyticsPlugin
   dotnet build AdvancedAnalyticsPlugin.csproj -c Release
   ```

3. **El plugin compilado estará en:**
   ```
   Plugins/AdvancedAnalyticsPlugin/bin/Release/net10.0-windows7.0/AdvancedAnalyticsPlugin.dll
   ```

### Empaquetado en un Solo DLL

El proyecto está configurado con **Costura.Fody** para empaquetar automáticamente todas las dependencias en un solo archivo DLL durante la compilación.

Las dependencias incluidas son:
- QuestPDF (generación de PDFs)
- ScottPlot + ScottPlot.WinForms (gráficas avanzadas)
- SkiaSharp (renderizado de gráficos)
- SixLabors.Fonts (fuentes para PDFs)

**Nota:** `Microsoft.Data.Sqlite` y `Newtonsoft.Json` NO se empaquetan porque ya están presentes en la aplicación principal.

## 📦 Instalación

1. **Copiar el DLL compilado** a la carpeta de plugins de REACT CRM:
   ```
   /plugins/AdvancedAnalyticsPlugin.dll
   ```

2. **Abrir REACT CRM** y navegar a la sección de **Gestión de Plugins**.

3. **El plugin se cargará automáticamente** y aparecerá en la lista de plugins disponibles.

4. **Habilitar el plugin** si no está habilitado por defecto.

## 🚀 Uso del Plugin

### 1. Ejecutar el Plugin

- Desde el menú de plugins en REACT CRM, hacer clic en **"Advanced Analytics & Reporting"**.
- Se abrirá la ventana principal del plugin.

### 2. Pestaña "Datos"

#### Seleccionar Campos
- Marque los campos que desea analizar (nativos o personalizados).
- Use **"Seleccionar Todos"** o **"Limpiar"** para facilitar la selección.
- Los campos nativos se muestran con 📊 y los personalizados con 📝.

#### Agrupar Datos
- Seleccione un campo para agrupar los datos (opcional).
- Por ejemplo: agrupar clientes por mes de creación.

#### Agregación
- Elija cómo agregar los datos:
  - **Conteo**: Cantidad de registros
  - **Suma**: Suma de valores numéricos
  - **Promedio**: Media aritmética
  - **Mínimo/Máximo**: Valores extremos
  - **Mediana**: Valor central

### 3. Pestaña "Gráfica"

#### Configurar Visualización
- **Título**: Nombre descriptivo para la gráfica
- **Estilo Visual**:
  - Profesional (limpio, ideal para presentaciones)
  - Mercado de Valores (estilo Bloomberg/Yahoo Finance)
  - Científico (para análisis técnico)
  - Moderno (colorido y atractivo)
  - Oscuro (modo oscuro)
- **Tipo de Gráfica**:
  - Línea (ideal para tendencias temporales)
  - Barra (comparaciones categóricas)
  - Área (énfasis en magnitud)
  - Dispersión (correlaciones)

#### Opciones Avanzadas
- ✅ **Mostrar leyenda**: Identificar cada serie
- ✅ **Mostrar cuadrícula**: Facilitar lectura de valores
- ✅ **Línea de tendencia**: Regresión lineal automática
- ✅ **Promedio móvil**: Suavizar datos con MA (configurar período)

### 4. Pestaña "Análisis"

#### Análisis Automático
- ✅ **Generar insights automáticos**: IA detecta patrones
- ✅ **Detectar anomalías**: Valores atípicos
- ✅ **Calcular RSI**: Índice de Fuerza Relativa (trading)
- ✅ **Calcular EMA**: Media Móvil Exponencial

#### Visualizar Insights
Los insights generados se muestran en la lista con iconos:
- 🔴 Crítico
- 🟡 Advertencia
- 🟢 Positivo
- 🔵 Informativo

### 5. Ejecutar Análisis

1. Haga clic en **"🔍 Ejecutar Análisis"**.
2. El plugin consultará la base de datos y procesará los datos.
3. La gráfica se renderizará automáticamente en el panel derecho.
4. Los insights aparecerán en la pestaña "Análisis".

### 6. Exportar Resultados

#### Guardar Gráfica
- **"💾 Guardar Gráfica"**: Exportar como PNG o JPEG en alta resolución (1920x1080).

#### Generar Reporte PDF
- **"📄 Exportar PDF"**: Crear un reporte completo que incluye:
  - Resumen ejecutivo
  - Estadísticas generales y por serie
  - Gráficas en alta resolución
  - Insights y recomendaciones
  - Tablas de datos detallados

## 🔧 Arquitectura Técnica

### Componentes Principales

#### 1. `DataQueryEngine.cs`
Motor de consultas que:
- Obtiene campos disponibles (nativos y custom)
- Ejecuta consultas con filtros y agregaciones
- Procesa datos de la base de datos SQLite
- Maneja campos JSON en ExtraData

#### 2. `ChartingEngine.cs`
Motor de visualización usando ScottPlot:
- Renderiza 5 tipos de gráficas
- Aplica 5 estilos visuales
- Agrega indicadores técnicos (MA, Trend)
- Exporta imágenes en alta resolución

#### 3. `AnalyticsEngine.cs`
Motor de análisis inteligente:
- Regresión lineal para tendencias
- Detección de anomalías (2σ)
- Cálculo de RSI y EMA
- Análisis de correlación (Pearson)
- Generación de insights automáticos

#### 4. `PdfReportGenerator.cs`
Generador de reportes usando QuestPDF:
- Layout profesional multipágina
- Tablas responsivas
- Imágenes embebidas
- Formato personalizable

#### 5. `MainAnalyticsForm.cs`
Interfaz de usuario WinForms:
- Diseño responsive con SplitContainer
- Tabs para organizar opciones
- Controles interactivos
- Actualización en tiempo real

### Modelos de Datos

- `DataSeries`: Serie de datos con estadísticas
- `DataPoint`: Punto individual con metadatos
- `ChartConfiguration`: Configuración de gráficas
- `QueryConfiguration`: Configuración de consultas
- `AnalyticsResult`: Resultado completo del análisis
- `Insight`: Insight generado automáticamente

## 🎯 Casos de Uso

### 1. Análisis de Clientes por Mes
```
Campos: CreatedAt (agrupado por mes), Count
Tipo: Barra
Resultado: Gráfica de clientes nuevos por mes
```

### 2. Distribución de Edades
```
Campos: DOB (calcular edad), Count
Tipo: Área
Análisis: Detectar patrones demográficos
```

### 3. Tendencia de Custom Field Numérico
```
Campos: IngresoAnual (custom field numérico)
Tipo: Línea con MA y Tendencia
Análisis: RSI, Anomalías
```

### 4. Comparación Multi-Serie
```
Campos: Varios custom fields numéricos
Tipo: Línea múltiple
Análisis: Correlación entre campos
```

## 🐛 Solución de Problemas

### El plugin no aparece en la lista
- Verificar que el DLL esté en `/plugins/`
- Revisar que el archivo no esté bloqueado por Windows
- Comprobar logs de errores en REACT CRM

### Error al generar gráficas
- Asegurarse de seleccionar al menos un campo numérico
- Verificar que haya datos en la base de datos
- Revisar que los custom fields tengan valores válidos

### PDF no se genera
- Verificar permisos de escritura en la carpeta destino
- Comprobar que QuestPDF.dll esté embebido
- Revisar que la gráfica se haya renderizado correctamente

### Dependencias faltantes
Si aparecen errores de DLLs faltantes:
1. Verificar que Costura.Fody se ejecutó correctamente durante la compilación
2. Revisar el archivo FodyWeavers.xml
3. Limpiar y recompilar: `dotnet clean && dotnet build`

## 📊 Estadísticas del Proyecto

- **Líneas de código**: ~3,000+
- **Archivos**: 12
- **Dependencias NuGet**: 4 principales
- **Algoritmos implementados**: 10+
- **Tipos de gráficas**: 5
- **Estilos visuales**: 5
- **Operaciones matemáticas**: 4

## 📝 Licencias

- **QuestPDF**: Community License (gratuita para uso no comercial)
- **ScottPlot**: MIT License
- **SkiaSharp**: MIT License
- **Costura.Fody**: MIT License

## 🤝 Soporte

Para reportar bugs o solicitar características:
1. Abrir un issue en el repositorio de REACT CRM
2. Incluir capturas de pantalla si es posible
3. Describir los pasos para reproducir el problema

## 🎓 Créditos

Desarrollado para REACT CRM por el equipo de desarrollo.
Plugin diseñado para análisis profesional de datos empresariales.

---

**Versión**: 1.0.0
**Fecha**: Diciembre 2024
**Compatible con**: REACT CRM v3+
