(function (global) {
  const { createContext, useState, useEffect } = React;
  const TelemetryContext = createContext({});

  function TelemetryProvider({ children }) {
    const [telemetry, setTelemetry] = useState({});

    useEffect(() => {
      function overlayHost() {
        const host = window.location.hostname;
        return host && host.length > 0 ? host : 'localhost';
      }

      const url = window.OVERLAY_WS_URL || `ws://${overlayHost()}:5221/ws`;
      let socket;
      let reconnect;

      const handler = (ev) => {
        try {
          setTelemetry(JSON.parse(ev.data));
        } catch (err) {
          console.error('WebSocket parse error:', err);
        }
      };

      const connect = () => {
        socket = new WebSocket(url);
        socket.addEventListener('message', handler);
        socket.onclose = () => {
          reconnect = setTimeout(connect, 3000);
        };
      };

      connect();

      return () => {
        clearTimeout(reconnect);
        socket.removeEventListener('message', handler);
        socket.close();
      };
    }, []);

    return React.createElement(TelemetryContext.Provider, { value: telemetry }, children);
  }

  global.TelemetryContext = TelemetryContext;
  global.TelemetryProvider = TelemetryProvider;
})(window);
