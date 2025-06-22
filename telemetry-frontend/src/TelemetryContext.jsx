import React, { createContext, useState, useEffect } from 'react';

export const TelemetryContext = createContext({});

export function TelemetryProvider({ children }) {
  const [telemetry, setTelemetry] = useState({});

  useEffect(() => {
    const url = window.OVERLAY_WS_URL || 'ws://localhost:5221/ws';
    const socket = new WebSocket(url);
    const handle = (event) => {
      try {
        const data = JSON.parse(event.data);
        setTelemetry(data);
      } catch (err) {
        console.error('WebSocket parse error:', err);
      }
    };
    socket.addEventListener('message', handle);

    return () => {
      socket.removeEventListener('message', handle);
      socket.close();
    };
  }, []);

  return (
    <TelemetryContext.Provider value={telemetry}>
      {children}
    </TelemetryContext.Provider>
  );
}
