using System.Threading;
using System.Threading.Tasks;
using SuperBackendNR85IA.Models;
using SuperBackendNR85IA.Calculations;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private async Task<TelemetryModel?> BuildTelemetryModelAsync(CancellationToken ct)
        {
            if (_reader.Data == null) return null;

            var d = _reader.Data;
            var t = new TelemetryModel();
            using var perf = new Utilities.PerformanceMonitor(_log, "BuildTelemetry");

            PopulateVehicleData(d, t);
            PopulateAllExtraData(d, t);
            UpdateLapInfo(d, t);
            ReadSectorTimes(d, t);
            ComputeForceFeedback(d, t);
            ComputeRelativeDistances(d, t);
            PopulateSessionInfo(d, t);
            PopulateTyres(d, t);
            if (_log.IsEnabled(LogLevel.Debug))
            {
                _log.LogDebug(
                    $"Tyre snapshot - Pressures LF:{t.LfPress} RF:{t.RfPress} LR:{t.LrPress} RR:{t.RrPress}, " +
                    $"HotPress LF:{_lfLastHotPress} RF:{_rfLastHotPress} LR:{_lrLastHotPress} RR:{_rrLastHotPress}, " +
                    $"ColdPress LF:{_lfColdPress} RF:{_rfColdPress} LR:{_lrColdPress} RR:{_rrColdPress}, " +
                    $"Temps LF:{t.LfTempCl}/{t.LfTempCm}/{t.LfTempCr} RF:{t.RfTempCl}/{t.RfTempCm}/{t.RfTempCr} " +
                    $"LR:{t.LrTempCl}/{t.LrTempCm}/{t.LrTempCr} RR:{t.RrTempCl}/{t.RrTempCm}/{t.RrTempCr}, " +
                    $"Tread FL:{t.TreadRemainingFl} FR:{t.TreadRemainingFr} RL:{t.TreadRemainingRl} RR:{t.TreadRemainingRr}");
            }
            UpdateLastHotPress(t);
            await ApplyYamlData(d, t);
            RunCustomCalculations(d, t);
            TelemetryCalculations.SanitizeModel(t);
            await PersistCarTrackData(t);

            return t;
        }
    }
}
