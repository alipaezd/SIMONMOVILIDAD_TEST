using System;

namespace Simon.Movilidad.Api.Data.Entities
{
    public class SensorReading
    {
        public long Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public DateTime RecordedAt { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal FuelLevel { get; set; }
        public decimal Temperature { get; set; }
        public decimal? Speed { get; set; }
        public string? RawPayload { get; set; }
    }
}
