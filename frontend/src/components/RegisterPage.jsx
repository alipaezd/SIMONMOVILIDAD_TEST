// frontend/src/components/RegisterPage.jsx
import React, { useState } from 'react';
import { useNavigate }    from 'react-router-dom';
import api                from '../services/api';

export default function RegisterPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const navigate                = useNavigate();
  const [error, setError]       = useState(null);

  const handleRegister = async e => {
    e.preventDefault();
    setError(null);
    try {
      await api.post('/api/auth/register', { username, password });
      alert('Usuario creado con éxito. Por favor inicia sesión.');
      navigate('/login');
    } catch (err) {
      console.error('Error en registro:', err);
      setError(err.response?.data?.error || err.message);
    }
  };

  return (
    <form
      onSubmit={handleRegister}
      style={{
        maxWidth: 300,
        margin: '2rem auto',
        display: 'flex',
        flexDirection: 'column',
        gap: '1rem'
      }}
    >
      <h2 style={{ textAlign: 'center' }}>Crear cuenta</h2>
      <input
        type="text"
        placeholder="Usuario"
        value={username}
        onChange={e => setUsername(e.target.value)}
        required
      />
      <input
        type="password"
        placeholder="Contraseña"
        value={password}
        onChange={e => setPassword(e.target.value)}
        required
      />
      {error && <p style={{ color: 'var(--color-error)' }}>{error}</p>}
      <div style={{ display: 'flex', gap: '0.5rem' }}>
        <button type="submit" style={{ flex: 1 }}>Registrar</button>
        <button
          type="button"
          onClick={() => navigate('/login')}
          style={{
            flex: 1,
            backgroundColor: 'var(--color-secondary)'
          }}
        >
          Cancelar
        </button>
      </div>
    </form>
  );
}
