import React, { useState, useCallback, useRef, useContext } from 'react';
import './App.css';
import paiImg from './assets/images/pai.jpg';
import filhoImg from './assets/images/filho.jpg';
import alcoraoImg from './assets/images/alcorao.jpg';
import overlayList from './overlayList';
import { TelemetryContext } from './TelemetryContext';

export default function App() {
  const [started, setStarted] = useState(false);
  const [activeTab, setActiveTab] = useState('overlays');

  const clickSoundRef = useRef(new Audio('/sounds/mouse-click.mp3'));
  clickSoundRef.current.volume = 0.3;

  const engineSoundRef = useRef(new Audio('/sounds/engine-start.mp3'));
  engineSoundRef.current.volume = 0.5;

  const overlays = overlayList;
  const telemetry = useContext(TelemetryContext);

  const openOverlay = useCallback((name, file) => {
    const audio = clickSoundRef.current;
    audio.currentTime = 0;
    audio.play();

    if (window.electron?.createOverlay) {
      window.electron.createOverlay(name, {
        file,
        width: 400,
        height: 280,
        x: 100 + Math.random() * 200,
        y: 100 + Math.random() * 200,
        opacity: 0.92,
        movable: true,
        resizable: true,
        clickThrough: false,
      });
    } else {
      alert('Função Electron não disponível.');
    }
  }, []);

  const playStartSound = () => {
    const audio = engineSoundRef.current;
    audio.currentTime = 0;
    audio.play().catch(e => console.warn('Erro ao tocar som:', e));
  };

  if (!started) {
    return (
      <div className="app-container">
        <div className="highlight-verse">﷽ بسم الله الرحمن الرحيم</div>
        <div className="welcome-container">
          <div className="logo">
            <i className="fas fa-headset" /> Coaching <span className="gradient-nr85">NR85</span>{' '}
            <span className="gradient-ia">IA</span>
          </div>
          <div className="slogan">"Guiado pela fé, conduzido pela performance"</div>
          <div className="main-message">
            <p>Bem vindo ao Coaching NR85 IA.</p>
            <p>Tenha um engenheiro pessoal ao seu lado, sempre que acelerar.</p>
            <p>Nada na vida acontece por acaso. Tudo no tempo de Deus.</p>
          </div>
          <div className="triangle-inspiration">
            <div className="inspiration-card">
              <img src={paiImg} alt="Pai" />
              <label>Inspiração</label>
            </div>
            <div className="inspiration-card">
              <img src={filhoImg} alt="Filho" />
              <label>Motivação</label>
            </div>
            <div className="inspiration-card">
              <img src={alcoraoImg} alt="Alcorão" />
              <label className="faith">Fé</label>
            </div>
          </div>
          <button className="btn-start" onClick={() => { playStartSound(); setStarted(true); }}>
            Começar
          </button>
          <div className="quote-box">
            <p className="quote-text">"A paciência é luz." — Profeta Muhammad ﷺ</p>
          </div>
          <div className="footer-message">
            "Em nome de Deus, seja feita a vontade Dele."
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="menu-container">
      <div className="tab-bar">
        <button className={activeTab === 'overlays' ? 'tab active' : 'tab'} onClick={() => setActiveTab('overlays')}>Overlays</button>
        <button className={activeTab === 'coaching' ? 'tab active' : 'tab'} onClick={() => setActiveTab('coaching')}>Coaching</button>
      </div>
      <div className="overlay-list">
        {overlays.map(({ name, file }) => (
          <button key={name} onClick={() => openOverlay(name, file)}>{name}</button>
        ))}
      </div>
      <div className="telemetry-status">
        Velocidade: {telemetry?.player?.speed ? telemetry.player.speed.toFixed(0) : '--'} km/h
      </div>
    </div>
  );
}
