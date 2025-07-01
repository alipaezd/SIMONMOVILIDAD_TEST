using System;
using Simon.Movilidad.Api.Data.Entities;

namespace Simon.Movilidad.Api.Services
{
    /// <summary>
    /// Lectura de sensores entrante.
    /// </summary>
    public record SensorReadingDto(
        int      VehicleId,
        DateTime RecordedAt,
        decimal  Latitude,
        decimal  Longitude,
        decimal  FuelLevel,
        decimal  Temperature,
        decimal? Speed
    );

    /// <summary>
    /// DTO para alertas generadas.
    /// </summary>
    public record AlertDto(
        int        Id,
        int        VehicleId,
        AlertType  Type,
        string     Message,
        DateTime   TriggeredAt
    );
}
