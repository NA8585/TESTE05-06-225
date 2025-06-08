// Extensões de cálculo e preenchimento para overlays
using System;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Calculations
{
    public static class TelemetryCalculationsOverlay
    {
        public static void PreencherOverlayTanque(ref TelemetryModel model)
        {
            TelemetryCalculations.UpdateFuelData(ref model);

            float faltante = model.NecessarioFim - model.FuelLevel;
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
            model.Tyres.LfWear ??= new float[3];
            model.Tyres.RfWear ??= new float[3];
            model.Tyres.LrWear ??= new float[3];
            model.Tyres.RrWear ??= new float[3];

            model.Tyres.LfPress = model.Tyres.LfPress;
            model.Tyres.RfPress = model.Tyres.RfPress;
            model.Tyres.LrPress = model.Tyres.LrPress;
            model.Tyres.RrPress = model.Tyres.RrPress;
        }

        public static void PreencherOverlaySetores(ref TelemetryModel model)
        {
            TelemetryCalculations.UpdateSectorData(ref model);
        }

        public static void PreencherOverlayDelta(ref TelemetryModel model)
        {
            // Delta de tempo para o carro imediatamente à frente e atrás
            model.TimeDeltaToCarAhead = 0f;
            model.TimeDeltaToCarBehind = 0f;

            if (model.CarIdxPosition.Length == model.CarIdxF2Time.Length &&
                model.PlayerCarIdx >= 0 && model.PlayerCarIdx < model.CarIdxPosition.Length)
            {
                int myPos = model.CarIdxPosition[model.PlayerCarIdx];

                for (int i = 0; i < model.CarIdxPosition.Length; i++)
                {
                    if (model.CarIdxPosition[i] == myPos - 1 && i < model.CarIdxF2Time.Length)
                    {
                        // CarIdxF2Time[i] representa a diferença do carro i para o jogador
                        model.TimeDeltaToCarAhead = -model.CarIdxF2Time[i];
                    }
                    else if (model.CarIdxPosition[i] == myPos + 1 && i < model.CarIdxF2Time.Length)
                    {
                        model.TimeDeltaToCarBehind = model.CarIdxF2Time[i];
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
