// frontend/src/App.jsx
import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage      from './components/LoginPage';
import RegisterPage   from './components/RegisterPage';
import Dashboard      from './components/Dashboard';
import NewVehicle     from './components/NewVehicle';
import VehicleDetail  from './components/VehicleDetail';
import { useAuth }    from './contexts/AuthContext';

function PrivateRoute({ children }) {
  const { token } = useAuth();
  return token ? children : <Navigate to="/login" />;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Rutas públicas */}
        <Route path="/login"    element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Rutas privadas */}
        <Route
          path="/"
          element={
            <PrivateRoute>
              <Dashboard />
            </PrivateRoute>
          }
        />
        <Route
          path="/vehicles/new"
          element={
            <PrivateRoute>
              <NewVehicle />
            </PrivateRoute>
          }
        />
        <Route
          path="/vehicles/:id"
          element={
            <PrivateRoute>
              <VehicleDetail />
            </PrivateRoute>
          }
        />

        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </BrowserRouter>
  );
}
