using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Simon.Movilidad.Api.Data;
using Simon.Movilidad.Api.Data.Entities;
using Simon.Movilidad.Api.Services;
using BCrypt.Net;  // para el HashPassword


var builder = WebApplication.CreateBuilder(args);

// 1) DbContext SQLite
builder.Services.AddDbContext<MyDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
           .WithOrigins("http://localhost:3000")  
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();                  
    });
});

// 3) JWT y SignalR
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSignalR();

// 4) URLs
builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});
builder.Services.AddHostedService<SensorSimulator>();

var app = builder.Build();

// 5) Crear/actualizar BD
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    // 1) Crear o actualizar esquema
    db.Database.EnsureCreated();

    // 2) Sembrar roles “admin” y “user”
    if (!db.Roles.Any(r => r.Name == "admin"))
        db.Roles.Add(new Role { Name = "admin" });
    if (!db.Roles.Any(r => r.Name == "user"))
        db.Roles.Add(new Role { Name = "user" });
    db.SaveChanges();

    // 3) Sembrar usuario admin si no existe
    var adminRole = db.Roles.Single(r => r.Name == "admin");
    if (!db.Users.Any(u => u.Username == "admin"))
    {
        //para test , valores quemados 
        // en producción usar un hash seguro
        // y un password más complejo
        var admin = new User
        {
            Username     = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("a123456789"),
            RoleId       = adminRole.Id
        };
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

app.UseCors("AllowReact");


app.MapPost("/api/auth/register", async (RegisterRequest req, MyDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Username == req.Username))
        return Results.BadRequest(new { error = "Username already exists" });

    // Aseguro rol “user” existe
    var userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "user")
                   ?? new Role { Name = "user" };
    if (userRole.Id == 0)
        db.Roles.Add(userRole);

    var user = new User
    {
        Username     = req.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        Role          = userRole
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/auth/{user.Id}", new { user.Id, user.Username });
});
app.MapPost("/api/auth/login", async (LoginRequest req, MyDbContext db, JwtService jwt) =>
{
    var user = await db.Users
                       .Include(u => u.Role)
                       .FirstOrDefaultAsync(u => u.Username == req.Username);
    if (user is null || 
        !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = jwt.GenerateToken(user.Id, user.Role.Name);
    return Results.Ok(new { token });
});
app.MapHub<SensorHub>("/sensorHub");

// —— Ingesta de datos de sensores —— //
app.MapPost("/api/sensors/data", async (
    HttpContext            http,
    SensorReadingDto       dto,
    MyDbContext            db,
    JwtService             jwt,
    IHubContext<SensorHub> hub
) =>
{
    // 1) Validar el JWT
    var auth = http.Request.Headers["Authorization"].FirstOrDefault();
    if (auth is null || !auth.StartsWith("Bearer ")) 
        return Results.Unauthorized();

    var token = auth["Bearer ".Length..].Trim();
    if (!jwt.ValidateToken(token, out var claims))
        return Results.Unauthorized();

    // 2) Crear y guardar la lectura
    var reading = new SensorReading
    {
        VehicleId   = dto.VehicleId,
        RecordedAt  = dto.RecordedAt,
        Latitude    = dto.Latitude,
        Longitude   = dto.Longitude,
        FuelLevel   = dto.FuelLevel,
        Temperature = dto.Temperature,
        Speed       = dto.Speed
    };
    db.SensorReadings.Add(reading);
    await db.SaveChangesAsync();  

    // 3) Lógica predictiva de combustible
    var prev = await db.SensorReadings
                       .Where(r => r.VehicleId == dto.VehicleId && r.Id != reading.Id)
                       .OrderByDescending(r => r.RecordedAt)
                       .FirstOrDefaultAsync();

    Alert? alert = null;
    if (prev is not null)
    {
        var hoursDiff = (dto.RecordedAt - prev.RecordedAt).TotalHours;
        if (hoursDiff > 0)
        {
            var consumptionRate = (prev.FuelLevel - dto.FuelLevel) / (decimal)hoursDiff;
            if (consumptionRate > 0)
            {
                var hoursLeft = dto.FuelLevel / consumptionRate;
                if (hoursLeft < 1m)
                {
                    alert = new Alert
                    {
                        VehicleId   = dto.VehicleId,
                        Type        = AlertType.LowFuel,
                        Message     = $"Autonomía crítica: {hoursLeft:F2} h restantes",
                        TriggeredAt = DateTime.UtcNow
                    };
                    db.Alerts.Add(alert);
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    // 4) Notificar vía SignalR
    if (alert is not null)
    {
        await hub.Clients.All.SendAsync("ReceiveAlert", new AlertDto(
            alert.Id,
            alert.VehicleId,
            alert.Type,
            alert.Message,
            alert.TriggeredAt
        ));
    }

    await hub.Clients.All.SendAsync("ReceiveSensorReading", new SensorReadingDto(
        reading.VehicleId,
        reading.RecordedAt,
        reading.Latitude,
        reading.Longitude,
        reading.FuelLevel,
        reading.Temperature,
        reading.Speed
    ));

    // 5) Devolver Created con la lectura
    return Results.Created($"/api/sensors/{reading.Id}", reading);
});
// —— Endpoints de salud —— //
// Endpoint de salud para verificar conexión a la base de datos
app.MapGet("/api/health", async (HttpContext http,
                                 MyDbContext db,
                                 JwtService jwt) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
    {
        return Results.Unauthorized();
    }

    var up = await db.Database.CanConnectAsync();
    return Results.Ok(new { status = up ? "up" : "down" });
});

static bool TryValidateToken(HttpContext http, JwtService jwt, out int userId, out string role)
{
    userId = 0; role = string.Empty;
    var auth = http.Request.Headers["Authorization"].FirstOrDefault();
    if (auth is null || !auth.StartsWith("Bearer ")) return false;

    var token = auth["Bearer ".Length..].Trim();
    if (!jwt.ValidateToken(token, out var claims) || claims is null) return false;

    if (!claims.TryGetValue("sub", out var sub) || !int.TryParse(sub, out userId))
        return false;
    if (!claims.TryGetValue("role", out role))
        return false;

    return true;
}

// —— Endpoints de vehículos —— //
// Crear vehículo (requiere JWT válido)
app.MapPost("/api/vehicles", async (
    HttpContext            http,
    VehicleCreateDto       dto,
    MyDbContext            db,
    JwtService             jwt
) =>
{
    // 1) Validar JWT
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    // 2) Comprobar unicidad de código
    if (await db.Vehicles.AnyAsync(v => v.Code == dto.Code))
        return Results.BadRequest(new { error = "Code already exists" });

    // 3) Crear y guardar
    var veh = new Vehicle {
        Code        = dto.Code,
        Description = dto.Description,
        OwnerId     = userId
    };
    db.Vehicles.Add(veh);
    await db.SaveChangesAsync();

    // 4) Devolver DTO
    var result = new VehicleDto(
        veh.Id, veh.Code, veh.Description, veh.OwnerId, veh.CreatedAt
    );
    return Results.Created($"/api/vehicles/{veh.Id}", result);
});
// Listar vehículos (filtro por usuario si no es admin)
// (Si es admin, ve todos; si es user, ve sólo los suyos)
app.MapGet("/api/vehicles", async (HttpContext http, MyDbContext db, JwtService jwt) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    var query = db.Vehicles.AsQueryable();
    if (role != "admin")
        query = query.Where(v => v.OwnerId == userId);

    var list = await query
        .Select(v => new VehicleDto(
            v.Id, v.Code, v.Description, v.OwnerId, v.CreatedAt))
        .ToListAsync();

    return Results.Ok(list);
});

// Obtener un vehículo por ID (requiere JWT válido)
// (Si es admin, ve todos; si es user, ve sólo los suyos)
app.MapGet("/api/vehicles/{id:int}", async (int id, HttpContext http, MyDbContext db, JwtService jwt) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    var v = await db.Vehicles.FindAsync(id);
    if (v == null) return Results.NotFound();
    if (role != "admin" && v.OwnerId != userId) return Results.Forbid();

    return Results.Ok(new VehicleDto(v.Id, v.Code, v.Description, v.OwnerId, v.CreatedAt));
});
// Listar lecturas de sensores de un vehículo
// (filtro opcional por rango de fechas)    

app.MapGet("/api/vehicles/{vehicleId:int}/readings",
    async (
        int vehicleId,
        DateTime? from,    
        DateTime? to,      
        HttpContext http,
        MyDbContext db,
        JwtService jwt
    ) =>
{
    // 1) Validar JWT
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    // 2) Chequear vehículo y permisos
    var veh = await db.Vehicles.FindAsync(vehicleId);
    if (veh == null)
        return Results.NotFound(new { error = "Vehículo no encontrado" });
    if (role != "admin" && veh.OwnerId != userId)
        return Results.Forbid();

    // 3) Construir consulta
    var q = db.SensorReadings
              .Where(r => r.VehicleId == vehicleId);

    if (from.HasValue)
    {
        var utcFrom = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
        q = q.Where(r => r.RecordedAt >= utcFrom);
    }
    if (to.HasValue)
    {
        var utcTo = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
        q = q.Where(r => r.RecordedAt <= utcTo);
    }

    // 4) Ejecutar y proyectar
    var readings = await q
        .OrderBy(r => r.RecordedAt)
        .Select(r => new SensorReadingDto(
            r.VehicleId,
            r.RecordedAt,
            r.Latitude,
            r.Longitude,
            r.FuelLevel,
            r.Temperature,
            r.Speed
        ))
        .ToListAsync();

    return Results.Ok(readings);
})
.Produces<List<SensorReadingDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetVehicleReadings");
// Listar alertas (filtro opcional por vehículo y estado)
app.MapGet("/api/alerts", async (
    int?    vehicleId,
    bool?   acknowledged,
    HttpContext http,
    MyDbContext db,
    JwtService jwt
) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    var q = db.Alerts.Include(a => a.Vehicle).AsQueryable();

    // Sólo admin ve todas; user ve sólo sus vehículos
    if (role != "admin")
        q = q.Where(a => a.Vehicle.OwnerId == userId);

    if (vehicleId.HasValue)
        q = q.Where(a => a.VehicleId == vehicleId.Value);

    if (acknowledged.HasValue)
        q = q.Where(a => a.Acknowledged == acknowledged.Value);

    var list = await q
      .OrderByDescending(a => a.TriggeredAt)
      .Select(a => new AlertDto(
          a.Id, a.VehicleId, a.Type, a.Message, a.TriggeredAt
      ))
      .ToListAsync();

    return Results.Ok(list);
});

// Obtener una alerta
app.MapGet("/api/alerts/{id:int}", async (
    int id,
    HttpContext http,
    MyDbContext db,
    JwtService jwt
) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    var a = await db.Alerts
                    .Include(a => a.Vehicle)
                    .FirstOrDefaultAsync(a => a.Id == id);
    if (a == null) return Results.NotFound();
    if (role != "admin" && a.Vehicle.OwnerId != userId)
        return Results.Forbid();

    return Results.Ok(new AlertDto(
        a.Id, a.VehicleId, a.Type, a.Message, a.TriggeredAt
    ));
});

// Marcar alerta como “reconocida”
app.MapPut("/api/alerts/{id:int}/ack", async (
    int id,
    HttpContext http,
    MyDbContext db,
    JwtService jwt,
    IHubContext<SensorHub> hub
) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();

    var a = await db.Alerts.Include(a => a.Vehicle)
                           .FirstOrDefaultAsync(a => a.Id == id);
    if (a == null) return Results.NotFound();
    if (role != "admin" && a.Vehicle.OwnerId != userId)
        return Results.Forbid();

    a.Acknowledged = true;
    await db.SaveChangesAsync();

    // Notificar en tiempo real que la alerta fue reconocida
    await hub.Clients.All.SendAsync("AlertAcknowledged", new AlertDto(
        a.Id, a.VehicleId, a.Type, a.Message, a.TriggeredAt
    ));

    return Results.Ok();
});

// (Opcional) Crear alerta custom
app.MapPost("/api/alerts/custom", async (
    CustomAlertRequest req,
    HttpContext http,
    MyDbContext db,
    JwtService jwt,
    IHubContext<SensorHub> hub
) =>
{
    if (!TryValidateToken(http, jwt, out var userId, out var role))
        return Results.Unauthorized();
    if (role != "admin")
        return Results.Forbid();

    var a = new Alert {
        VehicleId   = req.VehicleId,
        Type        = AlertType.Custom,
        Message     = req.Message,
        TriggeredAt = DateTime.UtcNow
    };
    db.Alerts.Add(a);
    await db.SaveChangesAsync();

    await hub.Clients.All.SendAsync("ReceiveAlert", new AlertDto(
        a.Id, a.VehicleId, a.Type, a.Message, a.TriggeredAt
    ));

    return Results.Created($"/api/alerts/{a.Id}", new AlertDto(
        a.Id, a.VehicleId, a.Type, a.Message, a.TriggeredAt
    ));
});



app.Run();

// DTOs usados
public record CustomAlertRequest(int VehicleId, string Message);
public record AlertDto(int Id, int VehicleId, AlertType Type, string Message, DateTime TriggeredAt);
public record RegisterRequest(string Username, string Password);
public record LoginRequest   (string Username, string Password);
public record VehicleCreateDto(string Code, string? Description);

// DTO de respuesta
public record VehicleDto(int Id, string Code, string? Description, int OwnerId, DateTime CreatedAt);


// (SensorReadingDto ya la definimos arriba)
