# DESIGN.md

## 1. Vision general  
Simon Movilidad es un sistema de monitoreo IoT para flotas vehiculares que combina:  
- **Backend** .NET 8 (C#) con Minimal APIs  
- **Base de datos** PostgreSQL en produccion (SQLite para dev rapido)  
- **Frontend** React + Vite  
- **WebSockets** con SignalR para actualizaciones en tiempo real  
- **Offline-first** con IndexedDB (usando [idb](https://github.com/jakearchibald/idb))  
- **Autenticacion** JWT manual (sin librerias externas para validacion)  
- **Contenerizacion** completa con Docker Compose  
- **Pruebas de API** automatizadas via Postman + Newman  

---

## 2. Arquitectura de componentes  
                                +----------------------+
                                |   Base de Datos      |
                                |      (SQLite)        |
                                +----------^-----------+
                                           |
                                        EF Core
                                           |
+---------------------+    HTTP/JSON    +-----------------------------+
|  Frontend Web       | ──────────────> |  Backend API (.NET 8)       |
|  (React + Vite)     |                 |  - Minimal APIs             |
+---------^-----------+                 |  - JwtService (JWT auth)    |
          |                             |  - SignalR Hub              |
          | WebSockets                  +-------------+---------------+
          |                                            |
          |                                            |
          |                                            |
          v                       WebSockets            v
+---------------------+  <──────────────────────────> +----------------------+
| Mobile App          |                                |  Clientes en Tiempo  |
| (React Native /Expo)|                               |      Real (JS)       |
+---------------------+                                +----------------------+

### 2.1 Backend  

- **JWT manual**: generacion/verificacion de tokens mediante firma HMAC en `JwtService` propio.  
- **Capa de datos**: EF Core con `MyDbContext` y modelos:
  - `Role`, `User`, `Vehicle`, `SensorReading`, `Alert`, `JwtBlacklist`.  
- **Persistencia**: SQLite .  
- **Real‐time**: `SensorHub : Hub` expone:
  - `ReceiveSensorReading(SensorReadingDto)`  
  - `ReceiveAlert(AlertDto)`  
- **Logica de alertas**: al ingest de un nuevo `SensorReading`, se calcula tasa de consumo y, si la autonomia <1 h, se genera un `Alert` y se notifica al hub.  
- **Sensores**: se simulan cada x  minutos con valores al azar extraidos de un api externa , simulando peticiones al carro..  
  
### 2.2 Frontend  
- **React + Vite** como bundler ligero.  
- **Rutas** protegidas con `react-router-dom` y contexto `AuthContext`.  
- **Offline‐first**:
  - Interceptores Axios guardan GET `/vehicles` y GET `/vehicles/:id/readings` en IndexedDB.  
  - En mutaciones (`POST`/`PUT`/`DELETE`) offline las peticiones se encolan y reintentan al reconectar (`flushQueue`).  
  - Banner `OfflineBanner` indica estado de conexion (`OnlineStatusContext`).  
- **WebSockets**: conexion SignalR en `ws.js`, des/ensambla hub, escucha nuevos readings y alerts.  
- **Mapas y graficos**:
  - `MapView` usa MapLibre o Google Maps para trazar posicion historica y actual.  
  - `HistoryChart` con Recharts o similar para velocidad/combustible.  
- **Enmascaramiento**:  
  - Para roles no admin, `Masking.MaskDeviceCode(code)` oculta el medio del codigo de vehiculo (e.g. `DEV-****-XC54`).  


---

## 3. Decisiones tecnicas y trade-offs  

| Área                 | Eleccion                               | Razonamiento / Trade-off                                    |
|----------------------|----------------------------------------|-------------------------------------------------------------|
| Lenguaje backend     | .NET 8 Minimal APIs (C#)               | Baja curva de aprendizaje, integracion con SignalR y EF Core|
| Validacion JWT       | Implementacion manual                  | Control total, elimina dependencias; mayor responsabilidad  |
| BD desarrollo        | SQLite                                 | Arranque rapido, cero configuracion local                     |
| Frontend framework   | React + Vite                           | Startup ultrarrapido,  comunidad amplia                      |
| Offline              | IndexedDB (idb) + axios interceptors   | Persistencia local, colas de peticiones; complejidad extra  |
| Real-time            | SignalR                                | Integracion nativa en .NET|
| Contenerizacion      | Docker Compose                        | Entorno replicable Facilmente                                 |
| Tests de API         | Postman + Newman                      | Rapido de configurar; cubre endpoints clave                     |

---

# SETUP.md

## Prerrequisitos  
- Docker ≥ 20.10  
- Docker Compose ≥ 2.x  
- (Opcional para desarrollo local sin Docker)  
  - .NET 8 SDK  
  - Node.js ≥ 18 + npm  

## Usuario Administrador
    -user:admin 
    -password:a123456789

## Levantar todo con Docker Compose
    -docker-compose up --build

## DETENER

    -docker-compose down o CTRL+C