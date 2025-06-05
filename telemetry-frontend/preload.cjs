const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  createOverlay: (name, opts) => {
    console.log('[PRELOAD] createOverlay chamado:', name, opts);
    ipcRenderer.send('open-overlay', { name, ...opts });
  },
  focusOverlay: (name) => ipcRenderer.send('focus-overlay', name),
  isDestroyed: (id) => ipcRenderer.invoke('overlay-is-destroyed', id),
  onOverlayClosed: (handler) => ipcRenderer.on('overlay-closed', handler)
});
