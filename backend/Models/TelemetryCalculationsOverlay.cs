// Extensões de cálculo e preenchimento para overlays
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

            model.LapsRemaining = (int)TelemetryCalculations.GetFuelLapsLeft(model.FuelLevel, model.ConsumoVoltaAtual);
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

            // Estimativa de volta ideal com base em soma dos melhores setores
            model.EstLapTime = 0f;
            foreach (var setor in model.SessionBestSectorTimes)
                model.EstLapTime += setor;
        }
    }
}
