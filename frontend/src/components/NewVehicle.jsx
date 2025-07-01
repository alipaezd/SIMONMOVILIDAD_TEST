// frontend/src/components/NewVehicle.jsx
import React, { useState } from 'react';
import { useNavigate }    from 'react-router-dom';
import api                from '../services/api';

export default function NewVehicle() {
  const [code, setCode]             = useState('');
  const [description, setDescription] = useState('');
  const [error, setError]           = useState(null);
  const navigate                    = useNavigate();

  const handleSubmit = async e => {
    e.preventDefault();
    setError(null);
    try {
      await api.post('/api/vehicles', { code, description });
      navigate('/');  // Volver al dashboard tras crear
    } catch (err) {
      console.error('Error creando vehículo:', err);
      const msg = err.response?.data?.error 
                  || err.message 
                  || 'Error desconocido';
      setError(msg);
    }
  };

  return (
 <div className="container" style={{ maxWidth: 400, margin: '2rem auto' }}>
      <h2>Nuevo Vehículo</h2>
      <form
        onSubmit={handleSubmit}
        style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem'
        }}
      >
        {/* Código */}
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <label htmlFor="code" style={{ marginBottom: '0.25rem' }}>
            Código:
          </label>
          <input
            id="code"
            type="text"
            value={code}
            onChange={e => setCode(e.target.value)}
            placeholder="Ej. VEH-004"
            required
            style={{
              padding: '0.5rem',
              borderRadius: 4,
              border: '1px solid var(--color-border)',
              backgroundColor: 'var(--color-surface)',
              color: 'var(--color-text)'
            }}
          />
        </div>

        {/* Descripción */}
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <label htmlFor="description" style={{ marginBottom: '0.25rem' }}>
            Descripción:
          </label>
          <input
            id="description"
            type="text"
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="Descripción del vehículo"
            style={{
              padding: '0.5rem',
              borderRadius: 4,
              border: '1px solid var(--color-border)',
              backgroundColor: 'var(--color-surface)',
              color: 'var(--color-text)'
            }}
          />
        </div>

        {/* Error */}
        {error && (
          <p style={{ color: 'var(--color-error)', margin: 0 }}>
            {error}
          </p>
        )}

        {/* Botones */}
        <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
          <button
            type="submit"
            style={{
              flex: 1,
              padding: '0.5rem',
              backgroundColor: 'var(--color-primary)',
              color: 'var(--color-bg)',
              border: 'none',
              borderRadius: 4,
              cursor: 'pointer'
            }}
          >
            Crear
          </button>
          <button
            type="button"
            onClick={() => navigate(-1)}
            style={{
              flex: 1,
              padding: '0.5rem',
              backgroundColor: 'var(--color-secondary)',
              color: 'var(--color-bg)',
              border: 'none',
              borderRadius: 4,
              cursor: 'pointer'
            }}
          >
            Cancelar
          </button>
        </div>
      </form>
    </div>
  );
}
