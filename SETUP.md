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


Arquitectura de componentes  
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
