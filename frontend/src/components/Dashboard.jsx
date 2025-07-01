// frontend/src/components/Dashboard.jsx
import React, { useEffect, useState } from 'react';
import { FaSignOutAlt, FaCarSide,FaPlus  }    from 'react-icons/fa';
import NotificationBell               from './NotificationBell';
import { useAuth }                    from '../contexts/AuthContext';
import api                            from '../services/api';
import { useNavigate }                from 'react-router-dom';

export default function Dashboard() {
  const { logout } = useAuth();
  const navigate   = useNavigate();
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading]   = useState(true);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  useEffect(() => {
    api.get('/api/vehicles')
       .then(res => setVehicles(res.data))
       .catch(err => {
         if (err.response?.status === 401) handleLogout();
       })
       .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="container">
        <p>Cargando vehículos…</p>
      </div>
    );
  }

  return (
    <div className="container">
      {/* HEADER */}
      <header className="dashboard-header">
        <h1 className="dashboard-title">Mis Vehículos</h1>
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

      {/* BOTÓN AGREGAR */}
      <button
        className="primary-button"
        onClick={() => navigate('/vehicles/new')}
      >
         <FaPlus size={20} /> <FaCarSide size={20} style={{ marginLeft: '0.5rem' }} />
      </button>

      {/* LISTA DE VEHÍCULOS */}
      {vehicles.length === 0 ? (
        <p>No tienes vehículos registrados.</p>
      ) : (
        <ul>
          {vehicles.map(v => (
            <li key={v.id} className="vehicle-item">
              <FaCarSide className="vehicle-icon" />
              <strong>{v.code}</strong> — {v.description || 'Sin descripción'}
              <button
                className="secondary-button"
                onClick={() => navigate(`/vehicles/${v.id}`)}
              >
                Ver detalles
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
