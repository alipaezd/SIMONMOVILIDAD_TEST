import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { startSensorHub } from '../services/ws';

export default function AlertsPanel() {
  console.log('🔔 AlertsPanel montado');

  const [alerts, setAlerts] = useState([]);

  useEffect(() => {
    // 1) Carga inicial
    api.get('/api/alerts?acknowledged=false')
      .then(res => setAlerts(res.data));

    // 2) Suscripción SignalR
    const hub = startSensorHub(
      () => {},                  // no necesitamos nuevos readings aquí
      newAlert => {
        setAlerts(a => [newAlert, ...a]);
      }
    );
    hub.on('AlertAcknowledged', id => {
      setAlerts(a => a.filter(alert => alert.id !== id));
    });

    return () => {
      hub.stop();

    };
  }, []);

  const ack = id => {
    api.put(`/api/alerts/${id}/ack`)
      .then(() => setAlerts(a => a.filter(x => x.id !== id)));
  };

  if (!alerts.length) return <p>No hay alertas nuevas.</p>;

  return (
    <div style={{ padding: '1rem', background: '#2a2a2a', borderRadius: 4 }}>
      <h3>Alertas</h3>
      <ul>
        {alerts.map(a => (
          <li key={a.id} style={{ marginBottom: '0.5rem' }}>
            <strong>[{a.type}]</strong> Vehículo {a.vehicleId}: {a.message}
            <button 
              style={{ marginLeft: '0.5rem' }} 
              onClick={() => ack(a.id)}>
              ✓ 
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}
