// IRacingTelemetryService.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using IRSDKSharper; // Biblioteca IRSDKSharper para conexão com iRacing
using SuperBackendNR85IA.Models; // TelemetryModel e classes auxiliares
using SuperBackendNR85IA.Calculations; // Seus cálculos de telemetria personalizados
using SuperBackendNR85IA.Utilities;
using SuperBackendNR85IA.Repositories;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService : BackgroundService
    {
        private const int TICK_MS = 16; // ~60 Hz
        private const float MIN_VALID_LAP_FUEL = 0.05f; // ignora voltas sem consumo
        private readonly ILogger<IRacingTelemetryService> _log;
        private readonly TelemetryBroadcaster _broadcaster;
        private readonly TelemetryReader _reader;
        private readonly SessionYamlParser _yamlParser;

        private string _lastYaml = string.Empty;
        private (DriverInfo? Drv, WeekendInfo? Wkd, SessionInfo? Ses, SectorInfo? Sec, List<DriverInfo> Drivers) _cachedYamlData;
        private int _lastTick = -1;
        private int _lastLap = -1;
        private float _fuelAtLapStart = 0f;
        private float _consumoVoltaAtual = 0f;
        private float _consumoUltimaVolta = 0f;
        private readonly Queue<float> _ultimoConsumoVoltas = new();
        private int _lastSessionNum = -1;
        private readonly ICarTrackRepository _store;
        private string _carPath = string.Empty;
        private string _trackName = string.Empty;
        private bool _awaitingStoredData = false;
        private bool _wasOnPitRoad = false;
        private bool _initialized = false;
        private int _lastPitCount = -1;
        private float _lfLastHotPress;
        private float _rfLastHotPress;
        private float _lrLastHotPress;
        private float _rrLastHotPress;
        private float _lfColdPress;
        private float _rfColdPress;
        private float _lrColdPress;
        private float _rrColdPress;
        private float _lfLastTempCl;
        private float _lfLastTempCm;
        private float _lfLastTempCr;
        private float _rfLastTempCl;
        private float _rfLastTempCm;
        private float _rfLastTempCr;
        private float _lrLastTempCl;
        private float _lrLastTempCm;
        private float _lrLastTempCr;
        private float _rrLastTempCl;
        private float _rrLastTempCm;
        private float _rrLastTempCr;
        private float _lfColdTempCl;
        private float _lfColdTempCm;
        private float _lfColdTempCr;
        private float _rfColdTempCl;
        private float _rfColdTempCm;
        private float _rfColdTempCr;
        private float _lrColdTempCl;
        private float _lrColdTempCm;
        private float _lrColdTempCr;
        private float _rrColdTempCl;
        private float _rrColdTempCm;
        private float _rrColdTempCr;
        private float _lfStartTread;
        private float _rfStartTread;
        private float _lrStartTread;
        private float _rrStartTread;
        private readonly float[] _lfLastWear = new float[3];
        private readonly float[] _rfLastWear = new float[3];
        private readonly float[] _lrLastWear = new float[3];
        private readonly float[] _rrLastWear = new float[3];
        private float _lfLastTread;
        private float _rfLastTread;
        private float _lrLastTread;
        private float _rrLastTread;
        private bool _loggedAvailableVars = false;
        private readonly HashSet<string> _missingVarWarned = new();

        private static readonly PropertyInfo[] _telemetryProps =
            typeof(TelemetryModel).GetProperties()
                .Where(p => p.Name != nameof(TelemetryModel.SdkRaw))
                .ToArray();
        private static readonly string[] _telemetryPropNames = _telemetryProps
            .Select(p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1))
            .ToArray();

        public IRacingTelemetryService(
            ILogger<IRacingTelemetryService> log,
            TelemetryBroadcaster broadcaster,
            ICarTrackRepository store,
            SessionYamlParser yamlParser,
            TelemetryReader reader)
        {
            _log = log;
            TelemetryCalculations.SetLogger(log);
            _broadcaster = broadcaster;
            _store = store;
            _yamlParser = yamlParser;
            _reader = reader;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _log.LogInformation("IRacingTelemetryService está iniciando.");

                                    
            try
            {
                _reader.Start();
                _log.LogInformation("IRSDKSharper iniciado e aguardando conexão com o iRacing.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha ao iniciar IRSDKSharper.");
                return;
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TICK_MS));

            while (await timer.WaitForNextTickAsync(ct))
            {
                try
                {
                    if (!_reader.IsConnected || !_reader.IsStarted)
                        continue;

                    if (!_loggedAvailableVars && _reader.Data != null)
                    {
                        var available = _reader.Data.TelemetryDataProperties.Keys;
                        _log.LogInformation("Variáveis disponíveis no SDK: " + string.Join(", ", available));
                        _loggedAvailableVars = true;
                    }

                    if (_reader.Data != null && _reader.Data.TickCount != _lastTick)
                    {
                        var telemetryModel = await BuildTelemetryModelAsync(ct);
                        if (telemetryModel != null)
                        {
                            TelemetryCalculationsOverlay.PreencherOverlayTanque(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlayPneus(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlaySetores(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlayDelta(ref telemetryModel);
                            TelemetryCalculations.SanitizeModel(telemetryModel);

                            var payload = BuildFrontendPayload(telemetryModel);
                            var inputsPayload = BuildInputsPayload(telemetryModel);
                            await _broadcaster.BroadcastTelemetry(payload, inputsPayload);
                        }
                        _lastTick = _reader.Data.TickCount;
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Erro no loop principal de telemetria.");
                }
            }

            _log.LogInformation("IRacingTelemetryService está parando.");
            _reader.Stop();
        }



        private void UpdateLastHotPress(TelemetryModel t)
        {
            if (!_initialized)
            {
                _wasOnPitRoad = t.OnPitRoad;
                _initialized = true;
            }
            else if (t.OnPitRoad && !_wasOnPitRoad)
            {
                RecordPitEntry(t);
            }
            else if (!t.OnPitRoad && _wasOnPitRoad)
            {
                RecordPitExit(t);
            }

            bool initialUpdate = EnsureInitialValues(t);
            CopyTireStateToModel(t);
            _wasOnPitRoad = t.OnPitRoad;

            t.FrontStagger = (t.RfRideHeight - t.LfRideHeight) * 1000f;
            t.RearStagger  = (t.RrRideHeight - t.LrRideHeight) * 1000f;

            if (initialUpdate)
            {
                _log.LogInformation(
                    $"Startup tyre values - ColdPress LF:{_lfColdPress} RF:{_rfColdPress} LR:{_lrColdPress} RR:{_rrColdPress}, " +
                    $"HotPress LF:{_lfLastHotPress} RF:{_rfLastHotPress} LR:{_lrLastHotPress} RR:{_rrLastHotPress}");
            }

            if (_log.IsEnabled(LogLevel.Debug))
            {
                _log.LogDebug(
                    $"UpdateLastHotPress - Pressures LF:{t.LfPress} RF:{t.RfPress} LR:{t.LrPress} RR:{t.RrPress}, " +
                    $"HotPress LF:{t.LfLastHotPress} RF:{t.RfLastHotPress} LR:{t.LrLastHotPress} RR:{t.RrLastHotPress}, " +
                    $"ColdPress LF:{t.LfColdPress} RF:{t.RfColdPress} LR:{t.LrColdPress} RR:{t.RrColdPress}, " +
                    $"Temps LF:{t.LfTempCl}/{t.LfTempCm}/{t.LfTempCr} RF:{t.RfTempCl}/{t.RfTempCm}/{t.RfTempCr} " +
                    $"LR:{t.LrTempCl}/{t.LrTempCm}/{t.LrTempCr} RR:{t.RrTempCl}/{t.RrTempCm}/{t.RrTempCr}, " +
                    $"Tread FL:{t.TreadRemainingFl} FR:{t.TreadRemainingFr} RL:{t.TreadRemainingRl} RR:{t.TreadRemainingRr}");
            }
        }

        private void RecordPitEntry(TelemetryModel t)
        {
            _lfLastHotPress = t.LfPress;
            _rfLastHotPress = t.RfPress;
            _lrLastHotPress = t.LrPress;
            _rrLastHotPress = t.RrPress;

            Array.Copy(t.LfWear, _lfLastWear, _lfLastWear.Length);
            Array.Copy(t.RfWear, _rfLastWear, _rfLastWear.Length);
            Array.Copy(t.LrWear, _lrLastWear, _lrLastWear.Length);
            Array.Copy(t.RrWear, _rrLastWear, _rrLastWear.Length);
            _lfLastTread = t.TreadRemainingFl;
            _rfLastTread = t.TreadRemainingFr;
            _lrLastTread = t.TreadRemainingRl;
            _rrLastTread = t.TreadRemainingRr;

            _lfLastTempCl = t.LfTempCl;
            _lfLastTempCm = t.LfTempCm;
            _lfLastTempCr = t.LfTempCr;
            _rfLastTempCl = t.RfTempCl;
            _rfLastTempCm = t.RfTempCm;
            _rfLastTempCr = t.RfTempCr;
            _lrLastTempCl = t.LrTempCl;
            _lrLastTempCm = t.LrTempCm;
            _lrLastTempCr = t.LrTempCr;
            _rrLastTempCl = t.RrTempCl;
            _rrLastTempCm = t.RrTempCm;
            _rrLastTempCr = t.RrTempCr;

            _log.LogInformation(
                $"Pit entry - hot pressures LF:{_lfLastHotPress} RF:{_rfLastHotPress} LR:{_lrLastHotPress} RR:{_rrLastHotPress}, " +
                $"temps LF:{_lfLastTempCl}/{_lfLastTempCm}/{_lfLastTempCr} RF:{_rfLastTempCl}/{_rfLastTempCm}/{_rfLastTempCr} " +
                $"LR:{_lrLastTempCl}/{_lrLastTempCm}/{_lrLastTempCr} RR:{_rrLastTempCl}/{_rrLastTempCm}/{_rrLastTempCr}");
        }

        private void RecordPitExit(TelemetryModel t)
        {
            _lfColdPress = t.LfPress;
            _rfColdPress = t.RfPress;
            _lrColdPress = t.LrPress;
            _rrColdPress = t.RrPress;

            _lfColdTempCl = t.LfTempCl;
            _lfColdTempCm = t.LfTempCm;
            _lfColdTempCr = t.LfTempCr;
            _rfColdTempCl = t.RfTempCl;
            _rfColdTempCm = t.RfTempCm;
            _rfColdTempCr = t.RfTempCr;
            _lrColdTempCl = t.LrTempCl;
            _lrColdTempCm = t.LrTempCm;
            _lrColdTempCr = t.LrTempCr;
            _rrColdTempCl = t.RrTempCl;
            _rrColdTempCm = t.RrTempCm;
            _rrColdTempCr = t.RrTempCr;

            _lfStartTread = t.TreadRemainingFl;
            _rfStartTread = t.TreadRemainingFr;
            _lrStartTread = t.TreadRemainingRl;
            _rrStartTread = t.TreadRemainingRr;

            _log.LogInformation(
                $"Pit exit - cold pressures LF:{_lfColdPress} RF:{_rfColdPress} LR:{_lrColdPress} RR:{_rrColdPress}, " +
                $"temps LF:{_lfColdTempCl}/{_lfColdTempCm}/{_lfColdTempCr} RF:{_rfColdTempCl}/{_rfColdTempCm}/{_rfColdTempCr} " +
                $"LR:{_lrColdTempCl}/{_lrColdTempCm}/{_lrColdTempCr} RR:{_rrColdTempCl}/{_rrColdTempCm}/{_rrColdTempCr}, " +
                $"tread FL:{_lfStartTread} FR:{_rfStartTread} RL:{_lrStartTread} RR:{_rrStartTread}");
        }

        private bool EnsureInitialValues(TelemetryModel t)
        {
            bool updated = false;
            if (_lfColdPress == 0f && t.LfPress > 0f) { _lfColdPress = t.LfPress; updated = true; }
            if (_rfColdPress == 0f && t.RfPress > 0f) { _rfColdPress = t.RfPress; updated = true; }
            if (_lrColdPress == 0f && t.LrPress > 0f) { _lrColdPress = t.LrPress; updated = true; }
            if (_rrColdPress == 0f && t.RrPress > 0f) { _rrColdPress = t.RrPress; updated = true; }

            if (_lfColdTempCl == 0f && t.LfTempCl > 0f) { _lfColdTempCl = t.LfTempCl; updated = true; }
            if (_lfColdTempCm == 0f && t.LfTempCm > 0f) { _lfColdTempCm = t.LfTempCm; updated = true; }
            if (_lfColdTempCr == 0f && t.LfTempCr > 0f) { _lfColdTempCr = t.LfTempCr; updated = true; }
            if (_rfColdTempCl == 0f && t.RfTempCl > 0f) { _rfColdTempCl = t.RfTempCl; updated = true; }
            if (_rfColdTempCm == 0f && t.RfTempCm > 0f) { _rfColdTempCm = t.RfTempCm; updated = true; }
            if (_rfColdTempCr == 0f && t.RfTempCr > 0f) { _rfColdTempCr = t.RfTempCr; updated = true; }
            if (_lrColdTempCl == 0f && t.LrTempCl > 0f) { _lrColdTempCl = t.LrTempCl; updated = true; }
            if (_lrColdTempCm == 0f && t.LrTempCm > 0f) { _lrColdTempCm = t.LrTempCm; updated = true; }
            if (_lrColdTempCr == 0f && t.LrTempCr > 0f) { _lrColdTempCr = t.LrTempCr; updated = true; }
            if (_rrColdTempCl == 0f && t.RrTempCl > 0f) { _rrColdTempCl = t.RrTempCl; updated = true; }
            if (_rrColdTempCm == 0f && t.RrTempCm > 0f) { _rrColdTempCm = t.RrTempCm; updated = true; }
            if (_rrColdTempCr == 0f && t.RrTempCr > 0f) { _rrColdTempCr = t.RrTempCr; updated = true; }

            if (_lfLastWear[0] == 0f && t.LfWear.Length == 3 && t.LfWear.Sum() > 0f)
            { Array.Copy(t.LfWear, _lfLastWear, 3); updated = true; }
            if (_rfLastWear[0] == 0f && t.RfWear.Length == 3 && t.RfWear.Sum() > 0f)
            { Array.Copy(t.RfWear, _rfLastWear, 3); updated = true; }
            if (_lrLastWear[0] == 0f && t.LrWear.Length == 3 && t.LrWear.Sum() > 0f)
            { Array.Copy(t.LrWear, _lrLastWear, 3); updated = true; }
            if (_rrLastWear[0] == 0f && t.RrWear.Length == 3 && t.RrWear.Sum() > 0f)
            { Array.Copy(t.RrWear, _rrLastWear, 3); updated = true; }

            if (_lfStartTread == 0f && t.TreadRemainingFl > 0f) { _lfStartTread = t.TreadRemainingFl; updated = true; }
            if (_rfStartTread == 0f && t.TreadRemainingFr > 0f) { _rfStartTread = t.TreadRemainingFr; updated = true; }
            if (_lrStartTread == 0f && t.TreadRemainingRl > 0f) { _lrStartTread = t.TreadRemainingRl; updated = true; }
            if (_rrStartTread == 0f && t.TreadRemainingRr > 0f) { _rrStartTread = t.TreadRemainingRr; updated = true; }

            if (_lfLastTempCl == 0f && t.LfTempCl > 0f) { _lfLastTempCl = t.LfTempCl; updated = true; }
            if (_lfLastTempCm == 0f && t.LfTempCm > 0f) { _lfLastTempCm = t.LfTempCm; updated = true; }
            if (_lfLastTempCr == 0f && t.LfTempCr > 0f) { _lfLastTempCr = t.LfTempCr; updated = true; }
            if (_rfLastTempCl == 0f && t.RfTempCl > 0f) { _rfLastTempCl = t.RfTempCl; updated = true; }
            if (_rfLastTempCm == 0f && t.RfTempCm > 0f) { _rfLastTempCm = t.RfTempCm; updated = true; }
            if (_rfLastTempCr == 0f && t.RfTempCr > 0f) { _rfLastTempCr = t.RfTempCr; updated = true; }
            if (_lrLastTempCl == 0f && t.LrTempCl > 0f) { _lrLastTempCl = t.LrTempCl; updated = true; }
            if (_lrLastTempCm == 0f && t.LrTempCm > 0f) { _lrLastTempCm = t.LrTempCm; updated = true; }
            if (_lrLastTempCr == 0f && t.LrTempCr > 0f) { _lrLastTempCr = t.LrTempCr; updated = true; }
            if (_rrLastTempCl == 0f && t.RrTempCl > 0f) { _rrLastTempCl = t.RrTempCl; updated = true; }
            if (_rrLastTempCm == 0f && t.RrTempCm > 0f) { _rrLastTempCm = t.RrTempCm; updated = true; }
            if (_rrLastTempCr == 0f && t.RrTempCr > 0f) { _rrLastTempCr = t.RrTempCr; updated = true; }
            return updated;
        }

        private void CopyTireStateToModel(TelemetryModel t)
        {
            t.LfColdPress = _lfColdPress;
            t.RfColdPress = _rfColdPress;
            t.LrColdPress = _lrColdPress;
            t.RrColdPress = _rrColdPress;

            t.LfColdTempCl = _lfColdTempCl;
            t.LfColdTempCm = _lfColdTempCm;
            t.LfColdTempCr = _lfColdTempCr;
            t.RfColdTempCl = _rfColdTempCl;
            t.RfColdTempCm = _rfColdTempCm;
            t.RfColdTempCr = _rfColdTempCr;
            t.LrColdTempCl = _lrColdTempCl;
            t.LrColdTempCm = _lrColdTempCm;
            t.LrColdTempCr = _lrColdTempCr;
            t.RrColdTempCl = _rrColdTempCl;
            t.RrColdTempCm = _rrColdTempCm;
            t.RrColdTempCr = _rrColdTempCr;

            t.LfLastHotPress = _lfLastHotPress;
            t.RfLastHotPress = _rfLastHotPress;
            t.LrLastHotPress = _lrLastHotPress;
            t.RrLastHotPress = _rrLastHotPress;
            t.StartTreadFl = _lfStartTread;
            t.StartTreadFr = _rfStartTread;
            t.StartTreadRl = _lrStartTread;
            t.StartTreadRr = _rrStartTread;
            t.LfWear = _lfLastWear.ToArray();
            t.RfWear = _rfLastWear.ToArray();
            t.LrWear = _lrLastWear.ToArray();
            t.RrWear = _rrLastWear.ToArray();
            t.TreadRemainingFl = _lfLastTread;
            t.TreadRemainingFr = _rfLastTread;
            t.TreadRemainingRl = _lrLastTread;
            t.TreadRemainingRr = _rrLastTread;

            if (t.LfHotPressure <= 0f && _lfLastHotPress > 0f)
                t.LfHotPressure = _lfLastHotPress;
            if (t.RfHotPressure <= 0f && _rfLastHotPress > 0f)
                t.RfHotPressure = _rfLastHotPress;
            if (t.LrHotPressure <= 0f && _lrLastHotPress > 0f)
                t.LrHotPressure = _lrLastHotPress;
            if (t.RrHotPressure <= 0f && _rrLastHotPress > 0f)
                t.RrHotPressure = _rrLastHotPress;

            t.LfLastTempCl = _lfLastTempCl;
            t.LfLastTempCm = _lfLastTempCm;
            t.LfLastTempCr = _lfLastTempCr;
            t.RfLastTempCl = _rfLastTempCl;
            t.RfLastTempCm = _rfLastTempCm;
            t.RfLastTempCr = _rfLastTempCr;
            t.LrLastTempCl = _lrLastTempCl;
            t.LrLastTempCm = _lrLastTempCm;
            t.LrLastTempCr = _lrLastTempCr;
            t.RrLastTempCl = _rrLastTempCl;
            t.RrLastTempCm = _rrLastTempCm;
            t.RrLastTempCr = _rrLastTempCr;
        }

        private Dictionary<string, object?> BuildFrontendPayload(TelemetryModel t)
        {
            var payload = new Dictionary<string, object?>(_telemetryProps.Length + 3);
            for (int i = 0; i < _telemetryProps.Length; i++)
            {
                payload[_telemetryPropNames[i]] = _telemetryProps[i].GetValue(t);
            }

            // Snapshot simplificado de pneus e dados principais
            payload["telemetrySnapshot"] = BuildTelemetrySnapshot(t);

            // Inform clients which SDK variables are missing
            payload["missingVars"] = _missingVarWarned.ToArray();

            return payload;
        }

        private object BuildInputsPayload(TelemetryModel t)
        {
            return new
            {
                throttle = t.Throttle,
                brake = t.Brake,
                steeringWheelAngle = t.SteeringWheelAngle,
                gear = t.Gear
            };
        }

    }
}
