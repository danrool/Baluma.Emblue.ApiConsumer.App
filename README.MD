# Baluma Emblue Api Consumer

Aplicación WinForms construida con arquitectura limpia para ejecutar procesos automáticos relacionados con Emblue.

## Estructura

La solución está organizada en cuatro proyectos:

- **Baluma.Emblue.ApiConsumer.Domain**: Entidades y objetos de dominio.
- **Baluma.Emblue.ApiConsumer.Application**: Casos de uso, contratos e implementaciones de parsing.
- **Baluma.Emblue.ApiConsumer.Infrastructure**: Implementaciones de acceso a datos, servicios HTTP y utilitarios.
- **Baluma.Emblue.ApiConsumer.Presentation.WinForms**: Aplicación de escritorio encargada de exponer la UI y el manejo por línea de comandos.

## Configuración

Edite el archivo `src/Baluma.Emblue.ApiConsumer.Presentation.WinForms/appsettings.json` y complete las credenciales de Emblue.

- `AutomaticReports:BaseUrl`: URL base de la API (se provee un valor por defecto).
- `AutomaticReports:ReportsEndpoint`: Endpoint para solicitar los reportes automáticos.
- `AutomaticReports:Username` y `Password`: Credenciales de acceso.
- `Database:ConnectionString`: cadena de conexión SQLite donde se almacenarán los datos procesados.

## Ejecución por línea de comandos

```
Baluma.Emblue.ApiConsumer.Presentation.WinForms.exe ProcesarReporteDiario 2025-08-12
```

El segundo parámetro (fecha) es opcional. Si no se indica, se utilizará la fecha actual.

## Ejecución con interfaz gráfica

Ejecute la aplicación sin parámetros. Se mostrará un formulario simple que permite seleccionar una fecha (opcional) y lanzar el proceso.

Cada ejecución genera un registro en la tabla de bitácora junto con los datos de los reportes descargados.
