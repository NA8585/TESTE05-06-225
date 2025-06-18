const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const fs = require('fs');
const StoreP = require('electron-store');
const Store = StoreP.default || StoreP;
const store = new Store({ name: 'overlay-bounds' });

console.log('[MAIN] Caminho do preload:', path.resolve(__dirname, 'preload.cjs'));

let mainWindow;
const overlays = new Map();

function createMainWindow() {
  mainWindow = new BrowserWindow({
    width: 1024,
    height: 768,
    minWidth: 600,
    minHeight: 400,
    backgroundColor: '#0f172a',
    webPreferences: {
      preload: path.resolve(__dirname, 'preload.cjs'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true
    }
  });

  const splash = path.join(__dirname, 'public', 'start.html');
  const startFile = fs.existsSync(splash) ? splash : path.join(__dirname, 'telainicialPrograma.html');
  mainWindow.loadFile(startFile);
}

function createOverlay(name, file, bounds = {}) {
  console.log('[MAIN] Criando overlay:', name, file);

  if (file === '__close__') {
    const win = overlays.get(name);
    if (win && !win.isDestroyed()) win.close();
    return;
  }

  if (overlays.has(name)) {
    const win = overlays.get(name);
    if (!win.isDestroyed()) {
      win.show();
      return;
    }
    overlays.delete(name);
  }

  const fileToLoad = path.join(__dirname, 'public', 'overlays', file);
  console.log('[MAIN] Caminho final do overlay:', fileToLoad);

  if (!fs.existsSync(fileToLoad)) {
    console.error('[Overlay] Arquivo não encontrado:', fileToLoad);
    return;
  }

  const savedBounds = store.get(`overlay-bounds.${name}`);
  if (savedBounds && !bounds.width) bounds = { ...bounds, ...savedBounds };

  const win = new BrowserWindow({
    width: 640,
    height: 360,
    ...bounds,
    show: false,
    frame: false,
    transparent: true,
    alwaysOnTop: true,
    webPreferences: {
      preload: path.resolve(__dirname, 'preload.cjs'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true
    }
  });

  win.loadFile(fileToLoad);
  win.once('ready-to-show', () => win.show());

  win.on('close', () => {
    const bounds = win.getBounds();
    store.set(`overlay-bounds.${name}`, bounds);
  });

  win.on('closed', () => overlays.delete(name));
  overlays.set(name, win);
}

ipcMain.on('open-overlay', (_e, opts = {}) => createOverlay(opts.name, opts.file));
ipcMain.on('focus-overlay', (_e, name) => {
  const win = overlays.get(name);
  if (win && !win.isDestroyed()) win.show();
});
ipcMain.handle('overlay-close', e => BrowserWindow.fromWebContents(e.sender)?.close());
ipcMain.handle('overlay-lock', (e, lock) =>
  BrowserWindow.fromWebContents(e.sender)?.setIgnoreMouseEvents(lock, { forward: true })
);
ipcMain.handle('overlay-pin', (e, pin) =>
  BrowserWindow.fromWebContents(e.sender)?.setAlwaysOnTop(pin)
);
ipcMain.handle('overlay-is-destroyed', (e, id) => {
  for (const [_, win] of overlays) {
    if (win.id === id) return win.isDestroyed();
  }
  return true;
});

function openOverlayFromCli() {
  const arg = process.argv.find(a => a.startsWith('--open='));
  if (!arg) return;
  const file = arg.split('=')[1];
  if (!file) return;
  const name = file.replace(/\.html$/i, '');
  createOverlay(name, file);
}

app.whenReady().then(() => {
  createMainWindow();
  openOverlayFromCli();
});
app.on('activate', () => BrowserWindow.getAllWindows().length === 0 && createMainWindow());
app.on('window-all-closed', () => {
  for (const [, win] of overlays) {
    if (!win.isDestroyed()) win.destroy();
  }
  if (process.platform !== 'darwin') app.quit();
});
