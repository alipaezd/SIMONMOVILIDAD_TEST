import React from 'react';
import { useOnlineStatus } from '../contexts/OnlineStatusContext';

export default function OfflineBanner() {
  const online = useOnlineStatus();
  if (online) return null;
  return (
    <div style={{
      background: '#ffcc00',
      color: '#333',
      padding: '0.5rem',
      textAlign: 'center',
      fontWeight: 'bold',
      position: 'fixed',
      top: 0,
      width: '100%',
      zIndex: 1000
    }}>
      ⚠️ Estás sin conexión. Mostrando datos en caché.
    </div>
  );
}
