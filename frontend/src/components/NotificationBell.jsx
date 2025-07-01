import React, { useState, useRef, useEffect } from 'react';
import { FaBell } from 'react-icons/fa';
import AlertsPanel from './AlertsPanel';
import './NotificationBell.css';

export default function NotificationBell() {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  // Cerrar al hacer click fuera
  useEffect(() => {
    const onClickOutside = e => {
      if (ref.current && !ref.current.contains(e.target)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', onClickOutside);
    return () => document.removeEventListener('mousedown', onClickOutside);
  }, []);

  return (
    <div className="notification-bell" ref={ref}>
      <button
        className="bell-button"
        onClick={() => setOpen(o => !o)}
        aria-label="Mostrar alertas"
      >
        <FaBell size={24} />
      </button>
      {open && (
        <div className="alerts-dropdown">
          <AlertsPanel />
        </div>
      )}
    </div>
  );
}
