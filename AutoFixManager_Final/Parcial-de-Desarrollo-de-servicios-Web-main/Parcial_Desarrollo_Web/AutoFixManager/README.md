# AutoFix Manager

Sistema web para administrar un taller automotriz: clientes, vehículos, órdenes de servicio, mecánicos y cuentas de acceso. Incluye una API REST en ASP.NET Core, persistencia en MySQL mediante Entity Framework Core y una interfaz React.

## Requisitos

- .NET 10 SDK.
- MySQL 8.4 LTS o superior.
- Node.js 20 o superior y npm.
- Postman para ejecutar la colección incluida.
- Opcional: Docker Desktop con Docker Compose.

## Inicio rápido en Windows

Los comandos siguientes se ejecutan desde la carpeta `AutoFixManager` (la que contiene `backend`, `database` y `frontend`).

### 1. Crear la base de datos

Abre MySQL Workbench, conéctate a tu instancia y ejecuta `database\script-autofixmanager.sql` seguido de `database\002-expansion.sql`. El primer script crea la base principal y sus tablas originales; el segundo agrega las tablas de agenda, inventario, compras e historial.

Si ya tenías una base creada, ejecuta `database\002-expansion.sql` sobre ella antes de generar respaldos. Es idempotente y crea únicamente las tablas que falten; `EnsureCreated()` no actualiza automáticamente una base existente.

Crea un usuario de aplicación (no uses `root` desde la API) y dale permisos únicamente sobre esta base:

```sql
CREATE USER 'autofix_app'@'localhost' IDENTIFIED BY 'elige-una-clave-local';
GRANT ALL PRIVILEGES ON AutoFixManagerDb.* TO 'autofix_app'@'localhost';
```

Configura `ConnectionStrings:DefaultConnection` mediante User Secrets o la variable de entorno `ConnectionStrings__DefaultConnection`, con servidor `localhost`, puerto `3306`, base `AutoFixManagerDb`, usuario `autofix_app` y la clave elegida. No guardes esa clave en Git.

### 2. Ejecutar el backend

Abre una terminal PowerShell en `AutoFixManager`:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=AutoFixManagerDb;User Id=autofix_app;Password=TU_CLAVE;CharSet=utf8mb4"
dotnet restore .\backend\AutoFixManager.Api\AutoFixManager.Api.csproj
dotnet run --project .\backend\AutoFixManager.Api\AutoFixManager.Api.csproj --urls http://localhost:8080
```

Al iniciar, la API crea las tablas si la base está vacía y crea la cuenta `admin` únicamente si aún no está registrada. El script debe ejecutarse primero para dejar también preparado el procedimiento almacenado. La cuenta inicial es:

- Usuario: `admin`
- Contraseña: `Admin123*`

Cambia la contraseña antes de usar el sistema fuera del entorno local. No uses las claves de ejemplo en producción; configura la conexión y `Jwt__Key` mediante secretos/variables de entorno.

### 3. Ejecutar el frontend

Abre otra terminal PowerShell en `AutoFixManager`:

```powershell
Set-Location .\frontend
npm.cmd install
npm.cmd run dev
```

Frontend: `http://localhost:5173` · API: `http://localhost:8080` · Swagger: `http://localhost:8080/swagger` · Salud: `http://localhost:8080/health`.

### Alternativa: Docker Compose

Desde la carpeta `AutoFixManager`:

```powershell
$env:MYSQL_ROOT_PASSWORD = "CAMBIA_ESTA_CLAVE_ROOT"
$env:MYSQL_APP_PASSWORD = "CAMBIA_ESTA_CLAVE_APP"
docker compose up --build
```

Compose levanta MySQL 8.4, API y frontend. La base ejecuta el script SQL en el volumen nuevo. Define `MYSQL_ROOT_PASSWORD` y `MYSQL_APP_PASSWORD` como variables de entorno antes de iniciar; sus valores por defecto son solo para desarrollo. La interfaz queda en `http://localhost:3000` y la API en `http://localhost:8080`.

## Persistencia y Scaffold

`AutoFixDbContext` configura 15 tablas: `Clientes`, `Vehiculos`, `Servicios`, `Usuarios`, `ServiciosMecanicos`, `RefreshTokens`, `Citas`, `CategoriasRepuesto`, `Repuestos`, `Proveedores`, `OrdenesCompra`, `DetallesOrdenCompra`, `MovimientosInventario`, `ServiciosRepuestos` e `HistorialEstadosServicio`. El esquema incluye claves, índices, restricciones y relaciones para la agenda, compras e inventario. Los servicios existentes usan Entity Framework Core y el reporte de órdenes ejecuta `sp_ResumenServiciosPorEstado`.

### Respaldo manual en SQLite

MySQL continúa siendo la base principal. Un usuario con rol `ADMIN` puede solicitar una copia SQLite completa enviando `POST /api/respaldos/sqlite` con su token Bearer. La API guarda el archivo `backups/autofixmanager-backup.sqlite` (configurable con `Backup:SqlitePath`) y responde con el nombre, fecha y cantidad de registros respaldados por tabla. Cada ejecución reemplaza la copia anterior únicamente después de crear correctamente la nueva.

El respaldo conserva las **15 tablas** de la aplicación, incluidas las nuevas de agenda e inventario. No recomiendo tablas separadas de respaldo dentro del archivo: SQLite ya es un archivo de base de datos independiente que replica el esquema y los datos. En Docker Compose, el archivo queda persistido en el volumen `autofix-backup-data`.

Ejemplo desde PowerShell, una vez que tengas un JWT de administrador en `$token`:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/respaldos/sqlite" -Headers @{ Authorization = "Bearer $token" }
```

El respaldo puede contener datos personales y hashes de credenciales; protege el archivo y almacénalo fuera del equipo para contar con una copia ante pérdida del servidor.

Para generar una copia de lectura del esquema existente con la consola de NuGet de Visual Studio, instala una vez las herramientas de EF Core en el proyecto (`Microsoft.EntityFrameworkCore.Design` ya está configurado) y ejecuta desde el proyecto `backend\AutoFixManager.Api`:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 10.0.12
Scaffold-DbContext "Server=localhost;Port=3306;Database=AutoFixManagerDb;User Id=autofix_app;Password=TU_CLAVE;CharSet=utf8mb4" MySql.EntityFrameworkCore -OutputDir Models\Scaffold -ContextDir Data\Scaffold -Context AutoFixScaffoldContext -NoOnConfiguring -Force
```

Con .NET CLI, desde la raíz `AutoFixManager` (instala `dotnet-ef` solo si aún no está disponible):

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef dbcontext scaffold "Server=localhost;Port=3306;Database=AutoFixManagerDb;User Id=autofix_app;Password=TU_CLAVE;CharSet=utf8mb4" MySql.EntityFrameworkCore --project .\backend\AutoFixManager.Api\AutoFixManager.Api.csproj --output-dir Models\Scaffold --context-dir Data\Scaffold --context AutoFixScaffoldContext --no-onconfiguring --force
```

El scaffold se dirige a `Models\Scaffold` y `Data\Scaffold` para no sobrescribir `AutoFixDbContext`, que es el contexto utilizado por la aplicación.

## Autenticación y permisos

El inicio de sesión emite un JWT de acceso sin estado (15 minutos por defecto), firmado y validado con emisor, audiencia, firma y expiración. El token se envía en `Authorization: Bearer <token>`. También se emite un refresh token aleatorio de 7 días: solo su hash SHA-256 se persiste; el valor se entrega en una cookie `HttpOnly`, `SameSite=Lax`, con ruta restringida a `/api/auth`. Cada renovación revoca el token anterior y lo reemplaza en una transacción. En producción configura `Auth__SecureRefreshCookie=true`, HTTPS y claves robustas en variables de entorno. Los roles del taller corresponden al requisito académico así:

| Rol | Permisos |
|---|---|
| `ADMIN` | Gestiona clientes, vehículos, órdenes de servicio y cuentas de mecánicos/clientes. Asigna mecánicos a órdenes. |
| `MECANICO` | Consulta únicamente clientes, vehículos y órdenes con una orden asignada a su usuario; registra órdenes y actualiza las asignadas. El filtro `RequireMecanicoAsignacion` responde 404 para órdenes no asignadas. |
| `CLIENTE` | Consulta solo sus vehículos y su historial de servicios en el portal. |

Las rutas de administración rechazan otros roles; el mecánico no puede consultar listas globales ni reasignar una orden. El portal del cliente toma su `ClienteId` del token, nunca de un parámetro enviado por el usuario. `POST /api/auth/refresh` rota el refresh token; `POST /api/auth/logout` lo revoca y elimina la cookie.

## Endpoints principales

| Método | Ruta | Acceso |
|---|---|---|
| `POST` | `/api/auth/login` | Público |
| `POST` | `/api/auth/refresh`, `/api/auth/logout` | Público (cookie HttpOnly) |
| `GET`, `POST`, `DELETE` | `/api/auth/usuarios` | `ADMIN` (no elimina ADMIN ni mecánicos con órdenes asignadas) |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/clientes` | `ADMIN` |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/vehiculos` | `ADMIN` |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/servicios` | `ADMIN` |
| `GET` | `/api/mecanico/mis-clientes`, `/mis-vehiculos`, `/mis-servicios` | `MECANICO` |
| `GET`, `PUT` | `/api/mecanico/servicios/{id}` | `MECANICO` (solo asignaciones propias) |
| `POST` | `/api/mecanico/servicios` | `MECANICO` (se asigna automáticamente a sí mismo) |
| `GET` | `/api/portal/mis-vehiculos`, `/mis-servicios` | `CLIENTE` (solo sus registros) |
| `GET` | `/api/reportes/servicios/estado` | `ADMIN` (procedimiento almacenado) |
| `POST` | `/api/respaldos/sqlite` | `ADMIN` (genera o reemplaza el respaldo SQLite) |
| `GET` | `/health` | Público |

`POST` de recursos devuelve `201 Created`, `DELETE` devuelve `204 No Content`, los errores de validación devuelven `400`, los recursos inexistentes `404` y los conflictos de datos `409`. El middleware central convierte restricciones SQL comunes en respuestas seguras y registra los errores inesperados; los detalles internos de SQL no se exponen.

## Pruebas con Postman

Importa `postman\AutoFixManager.postman_collection.json`, configura `baseUrl` como `http://localhost:8080` y ejecuta en orden con el cookie jar habilitado. La colección utiliza `admin` para probar login y rotación de refresh, CRUD, validación, asignación de una orden a dos mecánicos, reporte por procedimiento almacenado y aislamiento por rol. Para una prueba manual detallada también está `AutoFixManager.http`; al usarla, copia los IDs de las respuestas a las variables del inicio del archivo y conserva la cookie del login para refresh/logout. La colección crea cuentas de prueba con nombres únicos por ejecución.

También se puede ejecutar la colección desde PowerShell con Newman, desde la carpeta `AutoFixManager` y con la API iniciada:

```powershell
npx.cmd --yes newman run .\postman\AutoFixManager.postman_collection.json --env-var baseUrl=http://localhost:8080 --reporters cli
```

Ejecución funcional verificada el 22-11-2026 contra una instancia MySQL temporal aislada: **56 solicitudes, 62 aserciones y 0 fallos**. Se validaron JWT y refresh, respuestas 401/403, CRUD, errores 400/404/409, asignación muchos-a-muchos, portales por rol y el procedimiento almacenado. Esta ejecución fue con Newman; no se generaron capturas de la aplicación Postman de escritorio. Para repetirla en otra instalación, configura primero la base y las credenciales locales según los pasos anteriores.

## Informe técnico y entrega

El informe está en `INFORME_TECNICO.md` e incluye descripción, arquitectura, diagrama de entidades, permisos, instrucciones y espacios para evidencias reales. Este directorio de trabajo no tiene un remoto Git configurado. Después de publicar el proyecto, pega en el informe el enlace de GitHub/plataforma y concede acceso de lectura al docente; no se inventa un enlace ni evidencia.

## Notas de seguridad

Las credenciales `admin` y MySQL del ejemplo son únicamente para desarrollo. Define claves robustas como secretos de entorno antes de publicar, rota la contraseña inicial y no subas credenciales reales al repositorio.
