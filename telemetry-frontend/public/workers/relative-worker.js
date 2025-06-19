self.onmessage = ({ data }) => {
  try {
    const { drivers, playerIdx, sessionType, numCarClasses } = data;
    const player = drivers.find(d => d.idx === playerIdx);
    if (!player) {
      self.postMessage({ type: 'error', error: 'player not found' });
      return;
    }

    const others = drivers.filter(d => d.idx !== playerIdx);
    const playerEstLap = player.estLap || 120;
    others.forEach(d => {
      if (d.carClassShortName === player.carClassShortName) {
        const estLapTime = d.estLap || playerEstLap;
        if (estLapTime === 0) {
          d.effectiveGap = undefined; d.timeGap = undefined; d.lapDiff = undefined;
          return;
        }
        let deltaPct = d.distPct - player.distPct;
        if (deltaPct > 0.5) deltaPct -= 1.0;
        if (deltaPct < -0.5) deltaPct += 1.0;
        const timeGap = deltaPct * estLapTime;
        d.timeGap = timeGap;
        d.effectiveGap = timeGap;
        if (!sessionType.toLowerCase().includes('practice')) {
          d.lapDiff = (d.lap || 0) - (player.lap || 0);
          if (deltaPct > 0.5) d.lapDiff--;
          if (deltaPct < -0.5) d.lapDiff++;
        } else {
          d.lapDiff = undefined;
        }
      } else {
        d.timeGap = undefined;
        d.lapDiff = undefined;
        d.effectiveGap = undefined;
      }
    });
    const N_AROUND = 4;
    const sameClass = others.filter(d => d.effectiveGap !== undefined);
    const ahead = sameClass.filter(d => d.effectiveGap > 0)
                           .sort((a,b)=>a.effectiveGap - b.effectiveGap)
                           .slice(0,N_AROUND).reverse();
    const behind = sameClass.filter(d => d.effectiveGap <= 0)
                            .sort((a,b)=>b.effectiveGap - a.effectiveGap)
                            .slice(0,N_AROUND);
    const carsToDisplay = [...ahead, player, ...behind];
    self.postMessage({ type: 'cars', cars: carsToDisplay, sessionType, numCarClasses });
  } catch (err) {
    self.postMessage({ type: 'error', error: err.message });
  }
};
