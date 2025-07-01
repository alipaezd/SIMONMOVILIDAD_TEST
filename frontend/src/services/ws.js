import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { getToken }                       from "../contexts/AuthContext";

let connection = null;

export function startSensorHub(onReading, onAlert) {
  console.log("Iniciando conexión SignalR...");
  console.log("onReading:", connection);

  // if (connection) return connection;

  connection = new HubConnectionBuilder()
    .withUrl(`http://localhost:5000/sensorHub`, {
      accessTokenFactory: () => getToken(),
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

  connection.on("ReceiveSensorReading", onReading);
  connection.on("ReceiveAlert", onAlert);

  connection
    .start()
    .then(() => console.log("SignalR conectado"))
    .catch((err) => console.error("SignalR error:", err));

  return connection;
}
