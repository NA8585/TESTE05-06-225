const callbacks = new Set();
let last = performance.now();
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
  requestAnimationFrame(loop);
}
requestAnimationFrame(loop);

export function schedule(callback) {
  callbacks.add(callback);
}

export function unschedule(callback) {
  callbacks.delete(callback);
}

export function getFPS() {
  return fps;
}
