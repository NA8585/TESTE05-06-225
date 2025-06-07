// Extensões de cálculo e preenchimento para overlays
using System;
using System.Linq;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Calculations
{
    public static class TelemetryCalculationsOverlay
    {
        public static void PreencherOverlayTanque(ref TelemetryModel model)
        {
            model.FuelUsePerLap = TelemetryCalculations.CalculateFuelPerLap(
                model.FuelUsedTotal,
                model.LapDistPct,
                model.LapLastLapTime,
                model.Lap,
                model.FuelUsePerLap
            );

            float diffLap = model.FuelLevelLapStart - model.FuelLevel;
            if (diffLap > 0 && !model.OnPitRoad)
                model.ConsumoVoltaAtual = diffLap;
            if (model.ConsumoVoltaAtual <= 0)
            {
                float[] opts = { model.FuelUsePerLap, model.FuelPerLap, model.FuelUsePerLapCalc };
                foreach (var opt in opts)
                {
                    if (opt > 0)
                    {
                        model.ConsumoVoltaAtual = opt;
                        break;
                    }
                }
            }

            model.LapsRemaining = (int)TelemetryCalculations.GetFuelLapsLeft(model.FuelLevel, model.ConsumoVoltaAtual);

            // --- Cálculo de Consumo Médio ---
            float lapsEfetivos = model.Lap + model.LapDistPct;
            float novoConsumoMedio = (lapsEfetivos > 0.5f && model.FuelUsedTotal > 0)
                ? model.FuelUsedTotal / lapsEfetivos
                : 0f;
            if (novoConsumoMedio > 0)
                model.ConsumoMedio = novoConsumoMedio;
            // Agora os cálculos que dependem do ConsumoMedio
            model.VoltasRestantesMedio = model.ConsumoMedio > 0
                ? model.FuelLevel / model.ConsumoMedio
                : 0;
            model.NecessarioFim = (float)TelemetryCalculations.GetFuelForTargetLaps(
                model.LapsRemainingRace, model.ConsumoMedio);

            float faltante = model.NecessarioFim - model.FuelLevel;
            model.RecomendacaoAbastecimento = MathF.Max(0, faltante);

            model.FuelStatus = new FuelStatus();
            if (faltante <= 0)
            {
                model.FuelStatus.Text = "OK";
                model.FuelStatus.Class = "status-ok";
            }
            else if (faltante <= 5)
            {
                model.FuelStatus.Text = "Atenção";
                model.FuelStatus.Class = "status-warning";
            }
            else
            {
                model.FuelStatus.Text = "Crítico";
                model.FuelStatus.Class = "status-danger";
            }
        }

        public static void PreencherOverlayPneus(ref TelemetryModel model)
        {
            model.LfWear ??= new float[3];
            model.RfWear ??= new float[3];
            model.LrWear ??= new float[3];
            model.RrWear ??= new float[3];

            model.LfPress = model.LfPress;
            model.RfPress = model.RfPress;
            model.LrPress = model.LrPress;
            model.RrPress = model.RrPress;
        }

        public static void PreencherOverlaySetores(ref TelemetryModel model)
        {
            if (model.LapAllSectorTimes == null || model.LapAllSectorTimes.Length == 0)
                model.LapAllSectorTimes = new float[model.SectorCount];

            if (model.LapDeltaToSessionBestSectorTimes == null || model.LapDeltaToSessionBestSectorTimes.Length == 0)
                model.LapDeltaToSessionBestSectorTimes = new float[model.SectorCount];

            if (model.SessionBestSectorTimes == null || model.SessionBestSectorTimes.Length == 0)
                model.SessionBestSectorTimes = new float[model.SectorCount];

            if (model.LapAllSectorTimes.All(v => v == 0f) && model.LapLastLapTime > 0 && model.SectorCount > 0)
                for (int i = 0; i < model.SectorCount; i++)
                    model.LapAllSectorTimes[i] = model.LapLastLapTime / model.SectorCount;

            if (model.SessionBestSectorTimes.All(v => v == 0f) && model.LapBestLapTime > 0 && model.SectorCount > 0)
                for (int i = 0; i < model.SectorCount; i++)
                    model.SessionBestSectorTimes[i] = model.LapBestLapTime / model.SectorCount;

            if (model.LapDeltaToSessionBestSectorTimes.All(v => v == 0f) && model.LapAllSectorTimes.Length == model.SessionBestSectorTimes.Length)
                for (int i = 0; i < model.SectorCount; i++)
                    model.LapDeltaToSessionBestSectorTimes[i] = model.LapAllSectorTimes[i] - model.SessionBestSectorTimes[i];

            // Estimativa de volta ideal com base em soma dos melhores setores
            model.EstLapTime = 0f;
            foreach (var setor in model.SessionBestSectorTimes)
                model.EstLapTime += setor;
        }

        public static void PreencherOverlayDelta(ref TelemetryModel model)
        {
            // Tempo até o carro à frente usando CarIdxF2Time
            if (model.CarIdxF2Time.Length > model.PlayerCarIdx)
                model.TimeDeltaToCarAhead = model.CarIdxF2Time[model.PlayerCarIdx];
            else
                model.TimeDeltaToCarAhead = 0f;

            model.TimeDeltaToCarBehind = 0f;
            if (model.CarIdxPosition.Length == model.CarIdxF2Time.Length &&
                model.PlayerCarIdx >= 0 && model.PlayerCarIdx < model.CarIdxPosition.Length)
            {
                int myPos = model.CarIdxPosition[model.PlayerCarIdx];
                for (int i = 0; i < model.CarIdxPosition.Length; i++)
                {
                    if (model.CarIdxPosition[i] == myPos + 1)
                    {
                        if (i < model.CarIdxF2Time.Length)
                            model.TimeDeltaToCarBehind = model.CarIdxF2Time[i];
                        break;
                    }
                }
            }

            model.SectorDeltas = model.LapDeltaToSessionBestSectorTimes ?? Array.Empty<float>();

            if (model.LapAllSectorTimes.Length == model.SessionBestSectorTimes.Length &&
                model.LapAllSectorTimes.Length > 0)
            {
                int len = model.LapAllSectorTimes.Length;
                var flags = new bool[len];
                for (int i = 0; i < len; i++)
                {
                    float lap = model.LapAllSectorTimes[i];
                    float best = model.SessionBestSectorTimes[i];
                    flags[i] = lap > 0 && Math.Abs(lap - best) < 1e-4f;
                }
                model.SectorIsBest = flags;
            }
            else
            {
                model.SectorIsBest = Array.Empty<bool>();
            }
        }
    }
}
