import React, { createContext, useState, useEffect } from 'react';

export const TelemetryContext = createContext({});

export function TelemetryProvider({ children }) {
  const [telemetry, setTelemetry] = useState({});

  useEffect(() => {
    function overlayHost() {
      const host = window.location.hostname;
      return host && host.length > 0 ? host : 'localhost';
    }

    const url = window.OVERLAY_WS_URL || `ws://${overlayHost()}:5221/ws`;
    let socket;
    let reconnect;

    const handle = (event) => {
      try {
        const data = JSON.parse(event.data);
        setTelemetry(data);
      } catch (err) {
        console.error('WebSocket parse error:', err);
      }
    };

    const connect = () => {
      socket = new WebSocket(url);
      socket.addEventListener('message', handle);
      socket.onclose = () => {
        reconnect = setTimeout(connect, 3000);
      };
    };

    connect();

    return () => {
      clearTimeout(reconnect);
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
