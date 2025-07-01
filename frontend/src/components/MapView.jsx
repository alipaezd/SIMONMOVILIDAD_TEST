import React from 'react';
import { MapContainer, TileLayer, Marker, Popup, Polyline } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';

import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png';
import markerIcon   from 'leaflet/dist/images/marker-icon.png';
import markerShadow from 'leaflet/dist/images/marker-shadow.png';

// Configura el icono por defecto
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl:       markerIcon,
  shadowUrl:     markerShadow,
});

export default function MapView({ readings }) {
  // 1) Validar array
  if (!Array.isArray(readings) || readings.length === 0) {
    return <p>No hay datos de ubicación.</p>;
  }

  // 2) Aquí declaras 'last' antes de usarla
  const last = readings[readings.length - 1];
  const center = [last.latitude, last.longitude];
  const path = readings.map(r => [r.latitude, r.longitude]);

  // 3) Renderizas el mapa
  return (
    <div style={{ height: 300, marginBottom: '1rem' }}>
      <MapContainer center={center} zoom={13} style={{ height: '100%' }}>
        <TileLayer
          attribution='&copy; OpenStreetMap'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Polyline positions={path} color="blue" />
        <Marker position={center}>
          <Popup>
            Última posición:<br/>
            {center[0].toFixed(4)}, {center[1].toFixed(4)}
          </Popup>
        </Marker>
      </MapContainer>
    </div>
  );
}
