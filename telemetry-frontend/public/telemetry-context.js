(function (global) {
  const { createContext, useState, useEffect } = React;
  const TelemetryContext = createContext({});

  function TelemetryProvider({ children }) {
    const [telemetry, setTelemetry] = useState({});

    useEffect(() => {
      const url = window.OVERLAY_WS_URL || 'ws://localhost:5221/ws';
      const socket = new WebSocket(url);
      const handler = (ev) => {
        try {
          setTelemetry(JSON.parse(ev.data));
        } catch (err) {
          console.error('WebSocket parse error:', err);
        }
      };
      socket.addEventListener('message', handler);
      return () => {
        socket.removeEventListener('message', handler);
        socket.close();
      };
    }, []);

    return React.createElement(TelemetryContext.Provider, { value: telemetry }, children);
  }

  global.TelemetryContext = TelemetryContext;
  global.TelemetryProvider = TelemetryProvider;
})(window);
