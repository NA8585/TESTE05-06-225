// Overlay.jsx – Menu component snippet
import React from 'react';

export default function MainMenu() {
  const open = (file) => {
    if (window.createOverlay) {
      window.createOverlay(file, { file });
    } else if (window.electronAPI) {
      window.electronAPI.createOverlay(file, { file });
    } else alert('Electron API ausente');
  };

  return (
    <div className="menu p-4">
      <button onClick={() => open('overlays/overlay-diagnostico-full-definitiva.html')}>Diagnóstico Full</button>
      {/* demais botões */}
    </div>
  );
}
