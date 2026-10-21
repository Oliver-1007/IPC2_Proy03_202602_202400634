# IPC2_Proy03_202602_202400634

Proyecto 3 de **Introducción a la Programación y Computación 2 (IPC2)**, Universidad de San Carlos de Guatemala.

Servicio web que simula la autorización de **Documentos Tributarios Electrónicos (DTE)** de la Superintendencia de Administración Tributaria (SAT). Recibe mensajes JSON, valida las facturas contra el Registro Tributario Unificado (RTU), emite un número único de autorización y genera reportes y consultas.

## Arquitectura

El sistema tiene dos programas que se comunican por HTTP:

| Programa | Proyecto | Tecnología | URL por defecto |
|---|---|---|---|
| Servicio 2 (Backend) | `SatApi` | C# · ASP.NET Core Web API (.NET 8) | http://localhost:5029 |
| Programa 1 (Frontend) | `SatWeb` | C# · Razor Pages (.NET 8) | http://localhost:5200 |

El frontend solo simula la aplicación principal: envía mensajes y muestra lo que la API almacenó. La API también se puede consumir desde otro cliente, como Postman.

## Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior
- Conexión a internet la primera vez (paquete NuGet de QuestPDF y, en la página de gráficas, Chart.js desde CDN)

## Ejecución

Primero inicia la API y luego el frontend, cada uno en su propia terminal:

```bash
cd SatApi
dotnet run
```

```bash
cd SatWeb
dotnet run
```

Abre http://localhost:5200 en el navegador.

Si cambias el puerto de la API, actualiza `ApiUrl` en `SatWeb/appsettings.json`.

## Estructura del repositorio

```
IPC2_Proy03_202602_202400634/
├── SatApi/
│   ├── Controllers/      Endpoints HTTP
│   ├── Models/           Entidades y mensajes de respuesta
│   ├── Services/         Validación, procesamiento, consultas y persistencia
│   └── Program.cs
└── SatWeb/
    ├── Pages/            Páginas Razor (envío, peticiones, ayuda, inicializar)
    ├── Services/         Cliente HTTP de la API y generador de PDF
    ├── wwwroot/          Estilos y documentación
    └── Program.cs
```

## Servicios de la API

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/rtu` | Crea o actualiza contribuyentes del RTU |
| GET | `/api/rtu` | Lista los contribuyentes almacenados |
| POST | `/api/autorizaciones` | Valida facturas y devuelve los números de autorización |
| GET | `/api/consultas/estadisticas?desde=&hasta=` | Facturas recibidas y errores por fecha |
| GET | `/api/consultas/contribuyente?nit=&tipo=&desde=&hasta=` | Facturas `emitidas` o `recibidas` por NIT en un rango |
| GET | `/api/consultas/iva?fecha=` | IVA emitido y recibido por NIT en una fecha |
| GET | `/api/consultas/rango?desde=&hasta=&tipo=` | Valor autorizado por fecha (`total` o `sinIva`) |
| POST | `/api/sistema/inicializar` | Borra todos los datos |

Las fechas de las consultas usan el formato `yyyy-MM-dd`.

### Actualizar el RTU

`POST /api/rtu`

```json
[
  { "NIT": "7378106", "nombre": "Empresa ABC, S.A." },
  { "NIT": "8338817", "nombre": "Comercial XYZ, S.A." }
]
```

Respuesta:

```json
{
  "CONTRIBUYENTES_NUEVOS": 2,
  "CONTRIBUYENTES_ACTUALIZADOS": 0,
  "NITS_INVALIDOS": 0
}
```

Si el NIT es válido y no existe, se crea el contribuyente. Si ya existe, se actualiza. Si no es válido, se ignora.

### Solicitar autorizaciones

`POST /api/autorizaciones`

```json
[
  {
    "TIEMPO": "Guatemala, 15/01/2025 15:25 hrs.",
    "REFERENCIA": "A1990",
    "NIT_EMISOR": "7378106",
    "NIT_RECEPTOR": "8338817",
    "VALOR": 100.00,
    "IVA": 12.00,
    "TOTAL": 112.00
  }
]
```

Respuesta:

```json
{
  "resultadoAutorizaciones": {
    "cantidadAutorizacionesAprobadas": 1,
    "cantidadAutorizacionesRechazadasPorNitEmisorInexistente": 0,
    "cantidadAutorizacionesRechazadasPorNitReceptorInexistente": 0,
    "detalleAprobaciones": [
      { "referencia": "A1990", "codigoAprobacion": 2026102100000001 }
    ]
  }
}
```

## Reglas de validación

Una factura se aprueba solo si no tiene ningún error:

- **NIT emisor / receptor inválido:** de 1 a 20 dígitos más el dígito verificador (0-9 o `K`), calculado con módulo 11.
- **NIT emisor / receptor inexistente:** el NIT es válido pero no está en el RTU.
- **IVA mal calculado:** el IVA debe ser `REDONDEAR(VALOR * 0.12, 2)`.
- **TOTAL mal calculado:** el total debe ser `VALOR + IVA`.
- **REFERENCIA duplicada:** la referencia ya fue aprobada antes (máximo 40 caracteres).

El número de autorización tiene el formato `yyyymmdd########`: la fecha en que se solicita la autorización y un correlativo de 8 dígitos que inicia en 1 cada día.

De la fecha del mensaje solo se usa `dd/mm/yyyy hh:mm`, extraído con una expresión regular. El lugar y cualquier otro texto se descartan sin considerarse un error.

## Funcionalidades del frontend

- **Enviar mensaje:** carga un archivo `.json` de actualización del RTU o de solicitudes de autorización y muestra la respuesta de la API.
- **Consultar contribuyente:** facturas emitidas o recibidas por NIT, con cantidad y monto por fecha, y total del rango.
- **Resumen de IVA:** IVA emitido y recibido por NIT en una fecha.
- **Resumen por fechas:** valor autorizado por fecha, con IVA o sin IVA.
- **Gráficas:** gráficas de los dos resúmenes anteriores.
- **Estadísticas por fecha:** facturas recibidas, errores por tipo, facturas sin errores y cantidad de emisores y receptores.
- **Reportes en PDF:** cada petición se puede descargar en PDF.
- **Ayuda:** información del estudiante y documentación.
- **Inicializar sistema:** devuelve la API al estado inicial, sin datos.

## Persistencia

La API guarda el RTU, las facturas recibidas (con sus errores) y los correlativos diarios en `SatApi/Data/estado.json`. Esa carpeta está en `.gitignore`.

## Autor

- **Nombre:** Oliver Jorge Raxtún Morales
- **Carné:** 202400634
- **Curso:** Introducción a la Programación y Computación 
- **Universidad:** Universidad de San Carlos de Guatemala
