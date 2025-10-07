# Baluma Emblue Api Consumer

Aplicación WinForms construida con arquitectura limpia para ejecutar procesos automáticos relacionados con Emblue.

## Estructura

Toda la solución vive en un único proyecto WinForms (`Baluma.Emblue.ApiConsumer.App`). El código se organiza en carpetas para mantener las capas:

- **Domain** (`src/Baluma.Emblue.ApiConsumer.App/Domain`): Entidades y objetos de dominio.
- **Application** (`src/Baluma.Emblue.ApiConsumer.App/Application`): Casos de uso, contratos e implementaciones de parsing.
- **Infrastructure** (`src/Baluma.Emblue.ApiConsumer.App/Infrastructure`): Implementaciones de acceso a datos, servicios HTTP y utilitarios.
- **Presentation** (`src/Baluma.Emblue.ApiConsumer.App/Presentation`): Formularios WinForms y punto de entrada.

El registro de logs se realiza mediante [NLog](https://nlog-project.org/); la configuración por defecto se encuentra en `src/Baluma.Emblue.ApiConsumer.App/nlog.config`.

## Configuración

Edite el archivo `src/Baluma.Emblue.ApiConsumer.App/appsettings.json` y complete las credenciales de Emblue.

- `AutomaticReports:BaseUrl`: URL base de la API (se provee un valor por defecto).
- `AutomaticReports:ReportsEndpoint`: Endpoint para solicitar los reportes automáticos.
- `AutomaticReports:Username` y `Password`: Credenciales de acceso.
- `Database:ConnectionString`: cadena de conexión SQLite donde se almacenarán los datos procesados.

## Ejecución por línea de comandos

```
Baluma.Emblue.ApiConsumer.App.exe ProcesarReporteDiario 2025-08-12
```

El segundo parámetro (fecha) es opcional. Si no se indica, se utilizará la fecha actual.

## Ejecución con interfaz gráfica

Ejecute la aplicación sin parámetros. Se mostrará un formulario simple que permite seleccionar una fecha (opcional) y lanzar el proceso.

Cada ejecución genera un registro en la tabla de bitácora junto con los datos de los reportes descargados.
