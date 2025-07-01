// frontend/src/components/VehicleDetail.jsx
import React, { useEffect, useState } from 'react';
import { FaSignOutAlt, FaCarSide,FaPlus  }    from 'react-icons/fa';
import NotificationBell               from './NotificationBell';

import { useParams, useNavigate }     from 'react-router-dom';
import api                            from '../services/api';
import MapView                        from './MapView';
import HistoryChart                   from './HistoryChart';
import AlertsPanel                    from './AlertsPanel';
import { startSensorHub, stopSensorHub } from '../services/ws';

export default function VehicleDetail() {
  const { id } = useParams();
  const nav    = useNavigate();

  // lectura histórica
  const [readings, setReadings] = useState([]);
  const [loading,  setLoading]  = useState(true);

  // filtros de fecha/hora en formato "YYYY-MM-DDThh:mm"
  const [fromDate, setFromDate] = useState('');
  const [toDate,   setToDate]   = useState('');
  const handleLogout = () => {
    logout();
    navigate('/login');
  };
  // función para cargar lecturas con filtros opcionales
  const fetchReadings = () => {
    setLoading(true);

    // Construir params sólo si hay valor
    const params = {};
    if (fromDate) params.from = new Date(fromDate).toISOString();
    if (toDate)   params.to   = new Date(toDate).toISOString();

    api.get(`/api/vehicles/${id}/readings`, { params })
      .then(res => setReadings(res.data))
      .catch(err => {
        console.error(err);
        if (err.response?.status === 401) nav('/login');
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    // Fetch inicial
    api.get(`/api/vehicles/${id}/readings`)
      .then(res => setReadings(res.data))
      .catch(err => {
        if (err.response?.status === 401) nav("/login");
      })
      .finally(() => setLoading(false));

    // Conectar SignalR
    const hub = startSensorHub(
      newReading => {
        console.log("📡 Nuevo reading recibido:", newReading,newReading.vehicleId,id);
        if (Number(newReading.vehicleId) === Number(id) ) {
          // 1) Si el reading es del vehículo actual, lo añadimos al array
          setReadings(prev => [...prev, newReading]);
        }

      },

    );

    // Cleanup al desmontar
    return () => {
      hub.stop();
      console.log("🚫 Desconectando SensorHub");
    };
  }, [id]);

  if (loading) return <p>Cargando lecturas…</p>;


  return (
    <div className="container" style={{ padding: '1rem' }}>

      <header className="dashboard-header">
              <button onClick={() => nav(-1)}>← Volver</button>
              <div className="header-actions">
                <NotificationBell />
                <button
                  className="icon-button"
                  onClick={handleLogout}
                  title="Cerrar sesión"
                >
                  <FaSignOutAlt size={24} />
                </button>
              </div>
      </header>
      <h2>Vehículo #{id}</h2>

      {/* Filtros de fecha */}
      <div style={{
        display: 'flex',
        gap: '1rem',
        alignItems: 'flex-end',
        margin: '1rem 0'
      }}>
        <div>
          <label>
            Desde:<br/>
            <input
              type="datetime-local"
              value={fromDate}
              onChange={e => setFromDate(e.target.value)}
            />
          </label>
        </div>
        <div>
          <label>
            Hasta:<br/>
            <input
              type="datetime-local"
              value={toDate}
              onChange={e => setToDate(e.target.value)}
            />
          </label>
        </div>
        <button onClick={fetchReadings}>Filtrar</button>
        <button onClick={() => {
          setFromDate('');
          setToDate('');
          fetchReadings();
        }}>
          Mostrar todo
        </button>
      </div>

      {/* Mapa con ruta y posición */}
      <MapView readings={readings} />

      {/* Gráfico histórico */}
      <HistoryChart readings={readings} />
    </div>
  );
}
