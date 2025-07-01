using Microsoft.AspNetCore.SignalR;

namespace Simon.Movilidad.Api.Services
{
    // Hereda de Hub<ISensorClient> para que el hub conozca la interfaz
    public class SensorHub : Hub<ISensorClient> { }
}
