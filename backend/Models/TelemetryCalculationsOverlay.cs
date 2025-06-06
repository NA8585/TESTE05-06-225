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

            model.ConsumoVoltaAtual = model.FuelLevelLapStart - model.FuelLevel;
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

            float lapsEfetivos = (model.Lap > 0) ? ((model.Lap - 1) + model.LapDistPct) : model.LapDistPct;
            model.ConsumoMedio = (lapsEfetivos > 0 && model.FuelUsedTotal > 0)
                ? model.FuelUsedTotal / lapsEfetivos
                : 0f;
            model.VoltasRestantesMedio = model.ConsumoMedio > 0 ? model.FuelLevel / model.ConsumoMedio : 0;
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

            // Estimativa de volta ideal com base em soma dos melhores setores
            model.EstLapTime = 0f;
            foreach (var setor in model.SessionBestSectorTimes)
                model.EstLapTime += setor;
        }
    }
}
