using System.Threading.Tasks;

namespace Simon.Movilidad.Api.Services
{
    public interface ISensorClient
    {
        Task ReceiveSensorReading(SensorReadingDto dto);
        Task ReceiveAlert(AlertDto dto);
        Task AlertAcknowledged(int alertId);  // si añadiste este método
    }
}
