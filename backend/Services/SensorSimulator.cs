using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simon.Movilidad.Api.Data;            // ← Necesario para MyDbContext
using Simon.Movilidad.Api.Data.Entities;    // ← Para AlertType, Vehicle, etc.

namespace Simon.Movilidad.Api.Services
{
    public class SensorSimulator : BackgroundService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly JwtService        _jwt;
        private readonly IServiceProvider  _provider;
        private readonly ILogger<SensorSimulator> _logger;

        public SensorSimulator(
            IHttpClientFactory httpFactory,
            JwtService jwt,
            IServiceProvider provider,
            ILogger<SensorSimulator> logger)
        {
            _httpFactory = httpFactory;
            _jwt         = jwt;
            _provider    = provider;
            _logger      = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Cliente para randomnumberapi
            var rndClient = _httpFactory.CreateClient();
            rndClient.BaseAddress = new Uri("https://www.randomnumberapi.com");

            // Cliente para tu API interna
            var apiClient = _httpFactory.CreateClient("api");
            // Generamos un token para userId=1 (ajusta si tienes otro seed user)
            var token = _jwt.GenerateToken(1, "user");
            apiClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("SensorSimulator iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1) Levantamos un scope para acceder a la BD
                    using var scope = _provider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                    // 2) Obtenemos todos los vehicleIds
                    var vehicleIds = await db.Vehicles
                                             .Select(v => v.Id)
                                             .ToListAsync(stoppingToken);

                    // 3) Para cada vehículo simulamos una lectura
                    foreach (var vehicleId in vehicleIds)
                    {
                        // Llamada a la API de random (5 valores)
                        var numbers = await rndClient
                            .GetFromJsonAsync<int[]>("/api/v1.0/random?min=100&max=1000&count=5", stoppingToken)
                            ?? Array.Empty<int>();

                        if (numbers.Length != 5)
                        {
                            _logger.LogWarning("RandomAPI devolvió {Count} valores", numbers.Length);
                            continue;
                        }

                        // Desestructuramos
                        var baseLat     = 4.6000m;
                        var baseLon     = -74.0800m;
                        var latitude    = baseLat + numbers[0] * 0.0001m;
                        var longitude   = baseLon + numbers[1] * 0.0001m;
                        var fuelLevel   = numbers[2] * 0.1m;
                        var temperature = (numbers[3] - 100m) * 25m / 900m + 15m;
                        var speed       = (numbers[4] - 100m) / 900m * 105m + 25m; 

                        // Preparamos el DTO
                        var dto = new
                        {
                            vehicleId,
                            recordedAt = DateTime.UtcNow,
                            latitude,
                            longitude,
                            fuelLevel,
                            temperature,
                            speed
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(dto),
                            Encoding.UTF8,
                            "application/json"
                        );

                        // Enviamos al endpoint de ingestión
                        var res = await apiClient.PostAsync("/api/sensors/data", content, stoppingToken);
                        if (res.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(
                                "Simulado vehicle={Veh} → lat={Lat},lon={Lon},fuel={Fuel},temp={Temp},speed={Speed}",
                                vehicleId, latitude, longitude, fuelLevel, temperature, speed
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Error simulando vehicle={Veh}: {Status}",
                                vehicleId, res.StatusCode
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en SensorSimulator");
                }

                // 4) Espera 3 minutos
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
