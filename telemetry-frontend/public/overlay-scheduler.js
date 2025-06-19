const callbacks = new Set();
let handle = 0;
let last = 0;
let frames = 0;
let fps = 0;

function loop(now) {
  frames++;
  if (now - last >= 1000) {
    fps = frames;
    frames = 0;
    last = now;
  }
  callbacks.forEach(cb => cb(now, fps));
  handle = callbacks.size > 0 ? requestAnimationFrame(loop) : 0;
}

function ensureLoop() {
  if (!handle && callbacks.size > 0) {
    last = performance.now();
    frames = 0;
    handle = requestAnimationFrame(loop);
  }
}

export function schedule(callback) {
  callbacks.add(callback);
  ensureLoop();
}

export function unschedule(callback) {
  callbacks.delete(callback);
  if (callbacks.size === 0 && handle) {
    cancelAnimationFrame(handle);
    handle = 0;
  }
}

export function getFPS() {
  return fps;
}
