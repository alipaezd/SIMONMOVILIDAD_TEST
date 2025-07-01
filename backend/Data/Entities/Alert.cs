using System;

namespace Simon.Movilidad.Api.Data.Entities
{
    public class Alert
    {
        public int       Id          { get; set; }
        public int       VehicleId   { get; set; }
        public Vehicle   Vehicle     { get; set; } = null!;
        public AlertType Type        { get; set; }
        public string    Message     { get; set; } = null!;
        public DateTime  TriggeredAt { get; set; }
        public bool      Acknowledged{ get; set; }
        public bool      SeenByAdmin { get; set; }
    }
}
