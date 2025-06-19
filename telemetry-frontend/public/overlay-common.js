import { schedule } from './overlay-scheduler.js';

let socket;
let latestData = null;

function overlayHost() {
  const host = window.location.hostname;
  return host && host.length > 0 ? host : 'localhost';
}

function initOverlayWebSocket(onData, params = {}) {
  const query = Object.keys(params).length > 0 ? '?' + new URLSearchParams(params).toString() : '';
  const url = (window.OVERLAY_WS_URL || `ws://${overlayHost()}:5221/ws`) + query;
  console.log('[Overlay] connecting to', url);
  schedule(() => {
    if (latestData !== null) {
      onData(latestData);
      latestData = null;
    }
  });

  function connect() {
    socket = new WebSocket(url);
    socket.onopen = () => console.log('[Overlay] WebSocket connected');
    socket.onmessage = (e) => {
      try {
        latestData = JSON.parse(e.data);
      } catch (err) {
        console.error('WS parse', err);
      }
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
