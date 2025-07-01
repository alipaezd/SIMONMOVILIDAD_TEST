using System;
using System.Collections.Generic;

namespace Simon.Movilidad.Api.Data.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
