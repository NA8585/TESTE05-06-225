import { schedule, getFPS } from './overlay-scheduler.js';

export function startProfiler(renderTarget) {
  if (!renderTarget) return;
  let last = 0;
  const el = typeof renderTarget === 'string' ? document.getElementById(renderTarget) : renderTarget;
  if (!el) return;
  schedule((_, fps) => {
    if (fps !== last) {
      last = fps;
      el.textContent = `FPS: ${fps}`;
    }
  });
}
