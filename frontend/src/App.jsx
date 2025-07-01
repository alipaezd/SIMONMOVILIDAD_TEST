import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage      from './components/LoginPage';
import RegisterPage   from './components/RegisterPage';
import Dashboard      from './components/Dashboard';
import NewVehicle     from './components/NewVehicle';
import VehicleDetail  from './components/VehicleDetail';
import { useAuth }    from './contexts/AuthContext';
import { OnlineStatusProvider } from './contexts/OnlineStatusContext';
import OfflineBanner            from './components/OfflineBanner';
function PrivateRoute({ children }) {
  const { token } = useAuth();
  return token ? children : <Navigate to="/login" />;
}

export default function App() {
return (
  <OnlineStatusProvider>
    <OfflineBanner />
    <BrowserRouter>
      <Routes>
        <Route path="/login"    element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

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
  </OnlineStatusProvider>
  );
}