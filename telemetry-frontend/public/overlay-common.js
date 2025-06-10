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

function enableBrowserEditMode(wrapperId, headerId) {
  const isElectron = !!window.electronAPI;
  if (!isElectron) {
    const wrapper = typeof wrapperId === 'string' ? document.getElementById(wrapperId) : wrapperId;
    const header = typeof headerId === 'string' ? document.getElementById(headerId) : headerId;
    if (wrapper) {
      wrapper.classList.add('global-edit-mode-active');
      wrapper.style.pointerEvents = 'auto';
      wrapper.querySelectorAll?.('.resize-handle').forEach(h => h.style.display = 'block');
    }
    if (header) header.style.cursor = 'move';
  }
  return isElectron;
}

export { initOverlayWebSocket, enableBrowserEditMode };
