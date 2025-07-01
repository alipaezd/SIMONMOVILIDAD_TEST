import React, { useState } from 'react';
import { useAuth }        from '../contexts/AuthContext';
import { useNavigate }    from 'react-router-dom';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login }               = useAuth();
  const navigate                = useNavigate();

  const handleSubmit = async e => {
    e.preventDefault();
    try {
      console.log('Intentando login', { username, password });
      const jwt = await login(username, password);
      console.log('Login exitoso, token:', jwt);
      navigate('/');  
    } catch (err) {
      console.error('Error en login:', err);
      alert('Usuario o clave incorrectos');
    }
  };

  return (
    <form 
      onSubmit={handleSubmit} 
      style={{
        maxWidth: 300,
        margin: '2rem auto',
        display: 'flex',
        flexDirection: 'column',
        gap: '1rem'
      }}
    >
      <h2 style={{ textAlign: 'center' }}>Iniciar sesión</h2>

      <input
        type="text"
        placeholder="Usuario"
        value={username}
        onChange={e => setUsername(e.target.value)}
        required
        style={{
          padding: '0.5rem',
          borderRadius: 4,
          border: '1px solid var(--color-border)',
          backgroundColor: 'var(--color-surface)',
          color: 'var(--color-text)'
        }}
      />

      <input
        type="password"
        placeholder="Contraseña"
        value={password}
        onChange={e => setPassword(e.target.value)}
        required
        style={{
          padding: '0.5rem',
          borderRadius: 4,
          border: '1px solid var(--color-border)',
          backgroundColor: 'var(--color-surface)',
          color: 'var(--color-text)'
        }}
      />

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
          Entrar
        </button>
        <button
          type="button"
          onClick={() => navigate('/register')}
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
          Registrarse
        </button>
      </div>
    </form>
  );
}
