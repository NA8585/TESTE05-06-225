import React, { createContext, useState, useEffect } from 'react';

export const TelemetryContext = createContext({});

export function TelemetryProvider({ children }) {
  const [telemetry, setTelemetry] = useState({});

  useEffect(() => {
    const url = window.OVERLAY_WS_URL || 'ws://localhost:5221/ws';
    let socket;
    let reconnectTimer;

    const handleMessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        setTelemetry(data);
      } catch (err) {
        console.error('WebSocket parse error:', err);
      }
    };

    const connect = () => {
      socket = new WebSocket(url);
      socket.addEventListener('message', handleMessage);
      socket.addEventListener('close', () => {
        reconnectTimer = setTimeout(connect, 3000);
      });
      socket.addEventListener('error', () => socket.close());
    };

    connect();

    return () => {
      clearTimeout(reconnectTimer);
      socket.removeEventListener('message', handleMessage);
      socket.close();
    };
  }, []);

  return (
    <TelemetryContext.Provider value={telemetry}>
      {children}
    </TelemetryContext.Provider>
  );
}
