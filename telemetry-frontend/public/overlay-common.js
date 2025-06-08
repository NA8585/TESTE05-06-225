let socket;

function overlayHost() {
  const host = window.location.hostname;
  return host && host.length > 0 ? host : 'localhost';
}

function initOverlayWebSocket(onData) {
  const url = window.OVERLAY_WS_URL || `ws://${overlayHost()}:5221/ws`;
  function connect() {
    socket = new WebSocket(url);
    socket.onmessage = (e) => {
      try { onData(JSON.parse(e.data)); } catch (err) { console.error('WS parse', err); }
    };
    socket.onclose = () => setTimeout(connect, 3000);
    socket.onerror = (err) => { console.error('WebSocket error', err); socket.close(); };
  }
  connect();
}

export { initOverlayWebSocket };
