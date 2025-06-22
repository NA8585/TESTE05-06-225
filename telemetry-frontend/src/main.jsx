// src/main.jsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';   // Tailwind e estilos globais
import App from './App';
import { TelemetryProvider } from './TelemetryContext';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <TelemetryProvider>
      <App />
    </TelemetryProvider>
  </React.StrictMode>
);
