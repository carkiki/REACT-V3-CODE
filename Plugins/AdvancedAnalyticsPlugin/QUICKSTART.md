# Quick Start - Advanced Analytics Plugin

Guía rápida de 5 minutos para empezar a usar el plugin de análisis avanzado.

## 🚀 Instalación Rápida

### 1. Compilar el Plugin

**Windows:**
```batch
cd Plugins\AdvancedAnalyticsPlugin
build.bat
```

**Linux/Mac:**
```bash
cd Plugins/AdvancedAnalyticsPlugin
chmod +x build.sh
./build.sh
```

El script compilará automáticamente el plugin y lo copiará a la carpeta `/plugins/`.

### 2. Cargar en REACT CRM

1. Abrir REACT CRM
2. El plugin se cargará automáticamente desde `/plugins/`
3. Ir a **Gestión de Plugins** (o donde se administren los plugins)
4. Activar "Advanced Analytics & Reporting"
5. Hacer clic en **Ejecutar Plugin**

## 📊 Primer Análisis en 3 Pasos

### Paso 1: Seleccionar Campos
- En la pestaña **"Datos"**, marca los campos que quieres analizar
- Ejemplo: ✓ Name, ✓ CreatedAt, ✓ Email
- Usa **"Seleccionar Todos"** para incluir todos los campos

### Paso 2: Configurar Visualización
- En la pestaña **"Gráfica"**, selecciona:
  - **Tipo**: Línea (para tendencias) o Barra (para comparaciones)
  - **Estilo Visual**: Mercado de Valores (recomendado)
  - Activa: ✓ Mostrar leyenda, ✓ Cuadrícula

### Paso 3: Ejecutar
- Clic en botón verde **"Ejecutar Análisis"**
- Espera 1-2 segundos (dependiendo de datos)
- ¡Gráfica lista!

## 🎯 Casos de Uso Comunes

### 1. Análisis de Crecimiento de Clientes
```
✓ Campo: CreatedAt
✓ Agrupar por: Mes
✓ Agregación: Count
✓ Tipo de gráfica: Línea
✓ Indicador: Moving Average (periodo 3)
```

### 2. Distribución por Estado
```
✓ Campo: State (custom field)
✓ Agrupar por: State
✓ Agregación: Count
✓ Tipo de gráfica: Barra
✓ Ordenar por: Valor (descendente)
```

### 3. Análisis de Ventas
```
✓ Campo: TotalSales (custom field)
✓ Agrupar por: Mes
✓ Agregación: Sum
✓ Tipo de gráfica: Área
✓ Línea de tendencia: ✓ Activada
```

## 📈 Características Clave

### Optimizado para 3,700+ Registros
El plugin está **optimizado automáticamente**:
- ✅ Sampling inteligente (LTTB) activado por defecto
- ✅ Límite de 5,000 registros procesados
- ✅ Límite de 1,000 puntos en gráfica
- ✅ Procesamiento asíncrono (UI responsive)
- ✅ Barra de progreso en tiempo real

**Resultado**: ~500ms para procesar 3,700 clientes ⚡

### Exportar Resultados

**Guardar Gráfica (PNG/JPG):**
1. Clic en botón azul **"Guardar Gráfica"**
2. Elegir ubicación y formato
3. Gráfica guardada en alta resolución

**Generar Reporte PDF:**
1. Clic en botón rojo **"Exportar PDF"**
2. Elegir ubicación
3. PDF profesional con:
   - Resumen ejecutivo
   - Gráficas embebidas
   - Estadísticas detalladas
   - Insights y recomendaciones
   - Tablas de datos

## ⚡ Configuración de Rendimiento

### Para 3,700 Clientes (Tu Caso)
**Configuración recomendada** (ya configurada por defecto):
```
Máx. registros a procesar: 5000
Máx. puntos en gráfica: 1000
✓ Sampling inteligente: Activado
```
**Rendimiento esperado**: ~500-700ms

### Para Más de 10,000 Clientes
```
Máx. registros a procesar: 5000
Máx. puntos en gráfica: 500
✓ Sampling inteligente: Activado
```
**Rendimiento esperado**: ~600-900ms

### Para Análisis Completo (Sin Límites)
⚠️ Solo en equipos potentes
```
Máx. registros a procesar: 0 (sin límite)
Máx. puntos en gráfica: 2000
✓ Sampling inteligente: Activado
```

## 🔧 Solución de Problemas

### La consulta tarda mucho
1. Reducir "Máx. registros a procesar" a 3000
2. Reducir "Máx. puntos en gráfica" a 500
3. Usar agrupación (Group By) en vez de datos individuales

### La gráfica se ve vacía
1. Verificar que los campos seleccionados tengan datos
2. Cambiar el tipo de agregación
3. Revisar filtros aplicados

### Error al generar PDF
1. Verificar que tienes permisos de escritura
2. Cerrar el PDF si está abierto
3. Asegurar espacio en disco disponible

## 💡 Tips Profesionales

1. **Usa agrupación para datasets grandes**
   - ❌ Individual: 3,700 puntos
   - ✅ Agrupado por mes: ~36 puntos (más rápido y legible)

2. **Combina indicadores técnicos**
   - Moving Average + Línea de Tendencia = análisis completo

3. **Exporta en alta resolución**
   - Para presentaciones: 1920x1080
   - Para impresión: 3840x2160

4. **Revisa los insights automáticos**
   - El plugin detecta tendencias y anomalías automáticamente
   - Aparecen en la pestaña "Análisis"

## 📚 Más Información

- **README.md**: Guía completa de características y uso
- **PERFORMANCE.md**: Optimizaciones y benchmarks detallados
- **DEVELOPER.md**: Guía para desarrolladores y extensiones
- **CHANGELOG.md**: Historial de cambios y versiones

## 🎓 Ejemplos Prácticos

### Ejemplo 1: Tendencia de Nuevos Clientes
1. Seleccionar: CreatedAt
2. Agrupar por: Mes
3. Agregación: Count
4. Gráfica: Línea
5. Activar: Moving Average (3 meses)
6. **Resultado**: Visualiza el crecimiento mensual de tu base de clientes

### Ejemplo 2: Top 10 Estados con Más Clientes
1. Seleccionar: State (custom field)
2. Agrupar por: State
3. Agregación: Count
4. Gráfica: Barra
5. Ordenar: Descendente
6. Límite: 10 resultados
7. **Resultado**: Identifica tus mercados principales

### Ejemplo 3: Análisis de Volatilidad
1. Seleccionar: Revenue (custom field)
2. Agrupar por: Semana
3. Agregación: Sum
4. Gráfica: Área
5. Activar: Detección de anomalías
6. Activar: Análisis de volatilidad
7. **Resultado**: Identifica períodos inusuales en ingresos

## ✅ Checklist de Verificación

Antes de crear tu primer reporte profesional:

- [ ] Plugin compilado y activado
- [ ] Al menos 1 campo seleccionado
- [ ] Tipo de gráfica elegido
- [ ] Configuración de rendimiento apropiada
- [ ] Análisis ejecutado correctamente
- [ ] Gráfica se visualiza correctamente
- [ ] Reporte PDF generado sin errores

---

**¿Listo?** ¡Abre REACT CRM y empieza a analizar tus datos en minutos! 🚀

**Última actualización**: 3 de Diciembre, 2024
**Versión del plugin**: 1.0.0
