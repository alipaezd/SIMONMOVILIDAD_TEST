import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../services/api';

const AuthContext = createContext({
  token: null,
  user: null,
  login: async () => {},
  logout: () => {},
});

export const AuthProvider = ({ children }) => {
  const [token, setTokenState] = useState(null);
  const [user, setUser]         = useState(null);

  // Al montar, cargamos token de localStorage (si existe)
  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    if (storedToken) {
      setTokenState(storedToken);
      // Opcional: podrías decodificar el token o llamar a /api/auth/me
      // para rellenar `user`. Por ahora lo dejamos null.
    }
  }, []);

  const login = async (username, password) => {
    // Llamada a tu endpoint de login
    const { data } = await api.post('/api/auth/login', { username, password });
    const jwt       = data.token;
    localStorage.setItem('token', jwt);
    setTokenState(jwt);
    return true;
  };

  const logout = () => {
    localStorage.removeItem('token');
    setTokenState(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ token, user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Hook para consumir el contexto
export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return ctx;
};

// Función helper para leer el token (usa localStorage para que funcione antes de montar React)
export const getToken = () => {
  return localStorage.getItem('token');
};
