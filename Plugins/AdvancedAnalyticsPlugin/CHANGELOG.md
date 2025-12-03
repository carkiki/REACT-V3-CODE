# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [1.0.0] - 2024-12-03

### Agregado

#### Motor de Consultas
- Motor de consultas avanzado para base de datos SQLite
- Soporte para campos nativos del sistema (Id, SSN, Name, DOB, Phone, Email, etc.)
- Soporte para custom fields definidos por el usuario
- Filtros avanzados: Equals, NotEquals, GreaterThan, LessThan, Contains, etc.
- Agrupación de datos por cualquier campo
- Funciones de agregación: Count, Sum, Average, Min, Max, Median, StdDev
- Ordenamiento y límite de resultados

#### Visualizaciones
- Gráficas de línea para análisis de tendencias temporales
- Gráficas de barra para comparaciones categóricas
- Gráficas de área para énfasis en magnitud de cambio
- Gráficas de dispersión para análisis de correlación
- 5 estilos visuales profesionales:
  - Profesional (limpio para presentaciones)
  - Mercado de Valores (estilo Bloomberg/Yahoo Finance)
  - Científico (para análisis técnico)
  - Moderno (colorido y atractivo)
  - Oscuro (modo oscuro)
- Indicadores técnicos financieros:
  - Moving Average (MA) con período configurable
  - Exponential Moving Average (EMA)
  - Relative Strength Index (RSI)
- Líneas de tendencia con regresión lineal
- Zoom y pan interactivos en gráficas
- Leyendas, cuadrículas y etiquetas personalizables

#### Análisis Avanzado
- Detección automática de tendencias (alcista, bajista, estable, volátil)
- Análisis de regresión lineal con R²
- Detección de anomalías usando desviación estándar (2σ)
- Análisis de volatilidad con coeficiente de variación
- Análisis de correlación entre múltiples series (Pearson)
- Generación automática de insights con severidad (crítico, advertencia, positivo, info)
- Cálculo de estadísticas completas: promedio, mediana, desviación estándar, rango

#### Operaciones Matemáticas
- Operaciones aritméticas entre series: suma, resta, multiplicación, división
- Aplicación de operaciones a todos los puntos de una serie
- Combinación de múltiples series de datos

#### Reportes PDF
- Generación automática de reportes profesionales en PDF
- Resumen ejecutivo con métricas clave
- Estadísticas generales y por serie
- Gráficas embebidas en alta resolución
- Sección de insights y recomendaciones con iconos de severidad
- Tablas de datos detallados (hasta 100 registros por serie)
- Diseño multipágina responsive
- Numeración de páginas
- Formato profesional usando QuestPDF

#### Interfaz de Usuario
- Diseño moderno con panel dividido (configuración | visualización)
- Organización en 3 pestañas:
  - Pestaña "Datos": Selección de campos, agrupación, agregación
  - Pestaña "Gráfica": Configuración visual completa
  - Pestaña "Análisis": Opciones de análisis avanzado e insights
- Controles intuitivos:
  - CheckedListBox para selección múltiple de campos
  - ComboBoxes para opciones categóricas
  - CheckBoxes para características opcionales
  - NumericUpDown para valores numéricos
- Botones de acción:
  - Ejecutar Análisis (verde)
  - Guardar Gráfica (azul)
  - Exportar PDF (rojo)
- Barra de estado con mensajes informativos
- Indicadores visuales para campos nativos (📊) y custom (📝)
- Iconos de severidad para insights (🔴🟡🟢🔵)

#### Empaquetado
- Configuración de Costura.Fody para empaquetado automático
- Todas las dependencias embebidas en un solo DLL
- Scripts de compilación multiplataforma (build.sh y build.bat)
- Exclusión de dependencias ya presentes en aplicación principal

#### Documentación
- README.md completo con guía de usuario
- DEVELOPER.md con guía técnica para desarrolladores
- CHANGELOG.md con historial de cambios
- Comentarios XML en código público
- Ejemplos de uso y casos prácticos

### Detalles Técnicos

#### Dependencias
- .NET 10.0 Framework
- QuestPDF 2024.12.3 (generación de PDFs)
- ScottPlot 5.0.54 (visualización de gráficas)
- Microsoft.Data.Sqlite 10.0.0 (acceso a base de datos)
- Newtonsoft.Json 13.0.4 (serialización JSON)
- Costura.Fody 5.8.0 (empaquetado de DLLs)

#### Arquitectura
- Patrón Separation of Concerns (SoC)
- 3 capas: Engine, Models, UI
- Singleton pattern en AnalyticsEngine
- Repository pattern para acceso a datos
- Factory pattern para creación de insights

#### Estadísticas del Código
- Total de archivos: 15+
- Líneas de código: ~3,500+
- Clases: 20+
- Métodos públicos: 100+
- Algoritmos implementados: 15+

### Notas de Lanzamiento

Esta es la primera versión estable del plugin Advanced Analytics & Reporting para REACT CRM.

El plugin está completamente funcional y listo para producción. Permite análisis
completos de datos empresariales con visualizaciones profesionales y reportes PDF.

#### Limitaciones Conocidas

1. **Rendimiento**: Para consultas con más de 10,000 registros, puede haber lentitud
2. **Memoria**: Gráficas con más de 1,000 puntos pueden consumir mucha RAM
3. **PDF**: Tablas limitadas a 100 registros por serie para evitar PDFs muy grandes
4. **Exportación**: Solo soporta PNG y JPEG para gráficas (no SVG)
5. **Idioma**: Interfaz solo en español (no multiidioma todavía)

#### Requisitos del Sistema

- Windows 7 o superior
- .NET 10.0 Runtime instalado
- Mínimo 4 GB RAM
- 100 MB espacio en disco

#### Instalación

Ver README.md para instrucciones detalladas de instalación y uso.

---

**Versión**: 1.0.0
**Fecha de Lanzamiento**: 3 de Diciembre, 2024
**Tipo**: Plugin para REACT CRM
**Licencia**: Propietaria
