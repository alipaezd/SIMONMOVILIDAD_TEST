import React from 'react';
import {
  ResponsiveContainer,
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend
} from 'recharts';

export default function HistoryChart({ readings }) {
  if (!readings.length) return <p>No hay datos históricos.</p>;

  // Prepara datos para el chart
  const data = readings.map(r => ({
    time: new Date(r.recordedAt).toLocaleTimeString(),
    fuel: r.fuelLevel,
    speed: r.speed
  }));

  return (
    <ResponsiveContainer width="80%" height={350}>
      <LineChart data={data} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
        <CartesianGrid stroke="#ccc" />
        <XAxis dataKey="time" />
        <YAxis yAxisId="left" domain={['auto', 'auto']} />
        <YAxis yAxisId="right" orientation="right" domain={['auto', 'auto']} />
        <Tooltip />
        <Legend verticalAlign="top" />
        <Line yAxisId="left" type="monotone" dataKey="fuel" stroke="#8884d8" name="Combustible (L)" />
        <Line yAxisId="right" type="monotone" dataKey="speed" stroke="#82ca9d" name="Velocidad (km/h)" />
      </LineChart>
    </ResponsiveContainer>
  );
}
