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

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService : BackgroundService
    {
        private const int TICK_MS = 16; // ~60 Hz
        private const float MIN_VALID_LAP_FUEL = 0.05f; // ignora voltas sem consumo
        private readonly ILogger<IRacingTelemetryService> _log;
        private readonly TelemetryBroadcaster _broadcaster;
        private readonly IRacingSdk _sdk = new();
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
        private readonly CarTrackDataStore _store;
        private string _carPath = string.Empty;
        private string _trackName = string.Empty;
        private bool _awaitingStoredData = false;
        private bool _wasOnPitRoad = false;
        private bool _initialized = false;
        private int _lastPitCount = -1;
        private string _lastCompoundLogged = string.Empty;
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

        public IRacingTelemetryService(
            ILogger<IRacingTelemetryService> log,
            TelemetryBroadcaster broadcaster,
            CarTrackDataStore store,
            SessionYamlParser yamlParser)
        {
            _log = log;
            TelemetryCalculations.SetLogger(log);
            _broadcaster = broadcaster;
            _store = store;
            _yamlParser = yamlParser;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _log.LogInformation("IRacingTelemetryService está iniciando.");

            _sdk.OnConnected += () => _log.LogInformation("SDK Conectado ao iRacing.");
            _sdk.OnDisconnected += () => _log.LogInformation("SDK Desconectado do iRacing.");
            _sdk.OnException += (ex) => _log.LogError(ex, "Exceção no IRSDKSharper.");

            try
            {
                StartSdkWithFlags();
                _log.LogInformation("IRSDKSharper iniciado e aguardando conexão com o iRacing.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha ao iniciar IRSDKSharper.");
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (!_sdk.IsConnected || !_sdk.IsStarted)
                    {
                        await Task.Delay(1000, ct);
                        continue;
                    }

                    if (!_loggedAvailableVars && _sdk.Data != null)
                    {
                        var available = _sdk.Data.TelemetryDataProperties.Keys;
                        _log.LogInformation("Variáveis disponíveis no SDK: " + string.Join(", ", available));
                        _loggedAvailableVars = true;
                    }

                    if (_sdk.Data != null && _sdk.Data.TickCount != _lastTick)
                    {
                        var telemetryModel = await BuildTelemetryModelAsync();
                        if (telemetryModel != null)
                        {
                            TelemetryCalculationsOverlay.PreencherOverlayTanque(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlayPneus(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlaySetores(ref telemetryModel);
                            TelemetryCalculationsOverlay.PreencherOverlayDelta(ref telemetryModel);
                            TelemetryCalculations.SanitizeModel(telemetryModel);

                            var payload = BuildFrontendPayload(telemetryModel);
                            await _broadcaster.BroadcastTelemetry(payload);
                        }
                        _lastTick = _sdk.Data.TickCount;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Erro no loop principal de telemetria.");
                }
                await Task.Delay(TICK_MS, ct);
            }

            _log.LogInformation("IRacingTelemetryService está parando.");
            _sdk.Stop();
        }


        private async Task<TelemetryModel?> BuildTelemetryModelAsync()
        {
            if (_sdk.Data == null) return null;

            var d = _sdk.Data;
            var t = new TelemetryModel();

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
            if (!string.IsNullOrEmpty(t.Tyres.Compound))
            {
                t.CompoundConfirmed = true;
                if (_lastCompoundLogged != t.Tyres.Compound)
                {
                    _lastCompoundLogged = t.Tyres.Compound;
                    _log.LogInformation($"Tire compound detected: {t.Tyres.Compound}");
                }
            }
            else
            {
                t.CompoundConfirmed = false;
            }

            TelemetryCalculations.SanitizeModel(t);
            await PersistCarTrackData(t);

            return t;
        }

        private void UpdateLastHotPress(TelemetryModel t)
        {
            if (!_initialized)
            {
                // Ignore the first update to avoid recording pit entry
                // values when the service starts while the car is stopped
                // in the pit lane.
                _wasOnPitRoad = t.OnPitRoad;
                _initialized = true;
            }
            else if (t.OnPitRoad && !_wasOnPitRoad)
            {
                // Entrando nos boxes: registra pressões e temperaturas quentes
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
            else if (!t.OnPitRoad && _wasOnPitRoad)
            {
                // Saindo dos boxes: registra pressões e temperaturas frias
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

            // Valores iniciais caso o serviço seja iniciado no meio da pista
            bool initialUpdate = false;
            if (_lfColdPress == 0f && t.LfPress > 0f) { _lfColdPress = t.LfPress; initialUpdate = true; }
            if (_rfColdPress == 0f && t.RfPress > 0f) { _rfColdPress = t.RfPress; initialUpdate = true; }
            if (_lrColdPress == 0f && t.LrPress > 0f) { _lrColdPress = t.LrPress; initialUpdate = true; }
            if (_rrColdPress == 0f && t.RrPress > 0f) { _rrColdPress = t.RrPress; initialUpdate = true; }

            if (_lfColdTempCl == 0f && t.LfTempCl > 0f) { _lfColdTempCl = t.LfTempCl; initialUpdate = true; }
            if (_lfColdTempCm == 0f && t.LfTempCm > 0f) { _lfColdTempCm = t.LfTempCm; initialUpdate = true; }
            if (_lfColdTempCr == 0f && t.LfTempCr > 0f) { _lfColdTempCr = t.LfTempCr; initialUpdate = true; }
            if (_rfColdTempCl == 0f && t.RfTempCl > 0f) { _rfColdTempCl = t.RfTempCl; initialUpdate = true; }
            if (_rfColdTempCm == 0f && t.RfTempCm > 0f) { _rfColdTempCm = t.RfTempCm; initialUpdate = true; }
            if (_rfColdTempCr == 0f && t.RfTempCr > 0f) { _rfColdTempCr = t.RfTempCr; initialUpdate = true; }
            if (_lrColdTempCl == 0f && t.LrTempCl > 0f) { _lrColdTempCl = t.LrTempCl; initialUpdate = true; }
            if (_lrColdTempCm == 0f && t.LrTempCm > 0f) { _lrColdTempCm = t.LrTempCm; initialUpdate = true; }
            if (_lrColdTempCr == 0f && t.LrTempCr > 0f) { _lrColdTempCr = t.LrTempCr; initialUpdate = true; }
            if (_rrColdTempCl == 0f && t.RrTempCl > 0f) { _rrColdTempCl = t.RrTempCl; initialUpdate = true; }
            if (_rrColdTempCm == 0f && t.RrTempCm > 0f) { _rrColdTempCm = t.RrTempCm; initialUpdate = true; }
            if (_rrColdTempCr == 0f && t.RrTempCr > 0f) { _rrColdTempCr = t.RrTempCr; initialUpdate = true; }

            if (_lfLastWear[0] == 0f && t.LfWear.Length == 3 && t.LfWear.Sum() > 0f)
            { Array.Copy(t.LfWear, _lfLastWear, 3); initialUpdate = true; }
            if (_rfLastWear[0] == 0f && t.RfWear.Length == 3 && t.RfWear.Sum() > 0f)
            { Array.Copy(t.RfWear, _rfLastWear, 3); initialUpdate = true; }
            if (_lrLastWear[0] == 0f && t.LrWear.Length == 3 && t.LrWear.Sum() > 0f)
            { Array.Copy(t.LrWear, _lrLastWear, 3); initialUpdate = true; }
            if (_rrLastWear[0] == 0f && t.RrWear.Length == 3 && t.RrWear.Sum() > 0f)
            { Array.Copy(t.RrWear, _rrLastWear, 3); initialUpdate = true; }

            // Do not capture hot pressures during normal running. These values
            // should only reflect the last pressures recorded when entering the
            // pits. Avoid copying the current live (cold) pressures while the
            // car is on track.

            if (_lfStartTread == 0f && t.TreadRemainingFl > 0f) { _lfStartTread = t.TreadRemainingFl; initialUpdate = true; }
            if (_rfStartTread == 0f && t.TreadRemainingFr > 0f) { _rfStartTread = t.TreadRemainingFr; initialUpdate = true; }
            if (_lrStartTread == 0f && t.TreadRemainingRl > 0f) { _lrStartTread = t.TreadRemainingRl; initialUpdate = true; }
            if (_rrStartTread == 0f && t.TreadRemainingRr > 0f) { _rrStartTread = t.TreadRemainingRr; initialUpdate = true; }

            if (_lfLastTempCl == 0f && t.LfTempCl > 0f) { _lfLastTempCl = t.LfTempCl; initialUpdate = true; }
            if (_lfLastTempCm == 0f && t.LfTempCm > 0f) { _lfLastTempCm = t.LfTempCm; initialUpdate = true; }
            if (_lfLastTempCr == 0f && t.LfTempCr > 0f) { _lfLastTempCr = t.LfTempCr; initialUpdate = true; }
            if (_rfLastTempCl == 0f && t.RfTempCl > 0f) { _rfLastTempCl = t.RfTempCl; initialUpdate = true; }
            if (_rfLastTempCm == 0f && t.RfTempCm > 0f) { _rfLastTempCm = t.RfTempCm; initialUpdate = true; }
            if (_rfLastTempCr == 0f && t.RfTempCr > 0f) { _rfLastTempCr = t.RfTempCr; initialUpdate = true; }
            if (_lrLastTempCl == 0f && t.LrTempCl > 0f) { _lrLastTempCl = t.LrTempCl; initialUpdate = true; }
            if (_lrLastTempCm == 0f && t.LrTempCm > 0f) { _lrLastTempCm = t.LrTempCm; initialUpdate = true; }
            if (_lrLastTempCr == 0f && t.LrTempCr > 0f) { _lrLastTempCr = t.LrTempCr; initialUpdate = true; }
            if (_rrLastTempCl == 0f && t.RrTempCl > 0f) { _rrLastTempCl = t.RrTempCl; initialUpdate = true; }
            if (_rrLastTempCm == 0f && t.RrTempCm > 0f) { _rrLastTempCm = t.RrTempCm; initialUpdate = true; }
            if (_rrLastTempCr == 0f && t.RrTempCr > 0f) { _rrLastTempCr = t.RrTempCr; initialUpdate = true; }

            if (initialUpdate)
            {
                _log.LogInformation(
                    $"Startup tyre values - ColdPress LF:{_lfColdPress} RF:{_rfColdPress} LR:{_lrColdPress} RR:{_rrColdPress}, " +
                    $"HotPress LF:{_lfLastHotPress} RF:{_rfLastHotPress} LR:{_lrLastHotPress} RR:{_rrLastHotPress}");
            }
            _wasOnPitRoad = t.OnPitRoad;

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

            // Preserve real-time hot pressures read from the SDK. Only fall
            // back to the last recorded values if no current data is
            // available (e.g. immediately after leaving the pits).
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

            t.FrontStagger = (t.RfRideHeight - t.LfRideHeight) * 1000f;
            t.RearStagger  = (t.RrRideHeight - t.LrRideHeight) * 1000f;

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

        private FrontendDataPayload BuildFrontendPayload(TelemetryModel t)
        {
            var drivers = t.YamlDrivers?.Select(d => new DriverPayload
            {
                CarIdx = d.CarIdx,
                UserName = d.UserName,
                IRating = d.IRating,
                LicLevel = d.LicLevel,
                LicSubLevel = d.LicSubLevel,
                CarClassID = d.CarClassID,
                CarClassShortName = d.CarClassShortName,
                CarPath = d.CarPath,
                TeamIncidentCount = d.TeamIncidentCount
            }).ToList();

            var sessionInfo = t.YamlSessionInfo == null ? null : new SessionInfoPayload
            {
                SessionType = t.YamlSessionInfo.SessionType ?? string.Empty,
                IncidentLimit = t.YamlSessionInfo.IncidentLimit,
                CurrentSessionTotalLaps = t.YamlSessionInfo.CurrentSessionTotalLaps
            };

            var weekendInfo = t.YamlWeekendInfo == null ? null : new WeekendInfoPayload
            {
                TrackDisplayName = t.YamlWeekendInfo.TrackDisplayName ?? string.Empty,
                TrackAirTemp = t.YamlWeekendInfo.TrackAirTemp
            };

            var results = t.Results?.Select(r => new ResultPayload
            {
                CarIdx = r.CarIdx,
                Position = r.Position,
                Time = r.Time,
                Interval = r.Interval,
                FastestTime = r.FastestTime,
                LastTime = r.LastTime,
                NewIRating = r.NewIRating
            }).ToList();

            RadarRelativePayload? radarRelative = null;
            if (t.CarIdxPosition.Length > 0)
            {
                int count = t.CarIdxPosition.Length;
                radarRelative = new RadarRelativePayload
                {
                    PlayerCarIdx = t.PlayerCarIdx,
                    TrackLength = t.TrackLength,
                    RadarCars = Enumerable.Range(0, count)
                        .Select(idx =>
                        {
                            float posX = 0f, posY = 0f;
                            if (idx < t.CarIdxLapDistPct.Length)
                            {
                                double ang = t.CarIdxLapDistPct[idx] * 2.0 * Math.PI;
                                posX = (float)Math.Cos(ang);
                                posY = (float)Math.Sin(ang);
                            }

                            bool onPit = idx < t.CarIdxOnPitRoad.Length && t.CarIdxOnPitRoad[idx];

                            return new RadarCarPayload
                            {
                                CarIdx = idx,
                                PosX = posX,
                                PosY = posY,
                                Gap = idx < t.CarIdxF2Time.Length ? t.CarIdxF2Time[idx] : 0f,
                                DeltaLap = (idx < t.CarIdxLap.Length ? t.CarIdxLap[idx] : 0) - t.Lap,
                                OnPitRoad = onPit,
                                TrackSurface = idx < t.CarIdxTrackSurface.Length ? t.CarIdxTrackSurface[idx] : 0,
                                CarClassId = idx < t.CarIdxCarClassIds.Length ? t.CarIdxCarClassIds[idx] : 0,
                                CarClassShortName = idx < t.CarIdxCarClassShortNames.Length ? t.CarIdxCarClassShortNames[idx] : string.Empty,
                                UserName = idx < t.CarIdxUserNames.Length ? t.CarIdxUserNames[idx] : string.Empty,
                                CarNumber = idx < t.CarIdxCarNumbers.Length ? t.CarIdxCarNumbers[idx] : string.Empty,
                                License = idx < t.CarIdxLicStrings.Length ? t.CarIdxLicStrings[idx] : string.Empty,
                                IRating = idx < t.CarIdxIRatings.Length ? t.CarIdxIRatings[idx] : 0,
                                TireCompound = idx < t.CarIdxTireCompounds.Length ? t.CarIdxTireCompounds[idx] : string.Empty,
                                IsFastestLap = idx < t.CarIdxBestLapTime.Length && Math.Abs(t.CarIdxBestLapTime[idx] - t.LapBestLapTime) < 1e-4f,
                                IsSameClass = idx < t.CarIdxCarClassIds.Length && t.CarIdxCarClassIds[idx] == t.PlayerCarClassID,
                                IsPlayer = idx == t.PlayerCarIdx
                            };
                        })
                        .ToArray()
                };
            }

            return new FrontendDataPayload
            {
                Telemetry = t,
                Drivers = drivers ?? new List<DriverPayload>(),
                SessionInfo = sessionInfo,
                WeekendInfo = weekendInfo,
                Results = results ?? new List<ResultPayload>(),
                ProximityCars = null,
                RadarRelative = radarRelative,
                Tyres = new TyrePayload
                {
                    LfPress = t.Tyres.LfPress,
                    RfPress = t.Tyres.RfPress,
                    LrPress = t.Tyres.LrPress,
                    RrPress = t.Tyres.RrPress,
                    LfColdPress = t.Tyres.LfColdPress,
                    RfColdPress = t.Tyres.RfColdPress,
                    LrColdPress = t.Tyres.LrColdPress,
                    RrColdPress = t.Tyres.RrColdPress,
                    LfSetupPressure = t.Tyres.LfSetupPressure,
                    RfSetupPressure = t.Tyres.RfSetupPressure,
                    LrSetupPressure = t.Tyres.LrSetupPressure,
                    RrSetupPressure = t.Tyres.RrSetupPressure,
                    LfHotPressure = t.Tyres.LfHotPressure,
                    RfHotPressure = t.Tyres.RfHotPressure,
                    LrHotPressure = t.Tyres.LrHotPressure,
                    RrHotPressure = t.Tyres.RrHotPressure,
                    LfTempCl = t.Tyres.LfTempCl,
                    LfTempCm = t.Tyres.LfTempCm,
                    LfTempCr = t.Tyres.LfTempCr,
                    RfTempCl = t.Tyres.RfTempCl,
                    RfTempCm = t.Tyres.RfTempCm,
                    RfTempCr = t.Tyres.RfTempCr,
                    LrTempCl = t.Tyres.LrTempCl,
                    LrTempCm = t.Tyres.LrTempCm,
                    LrTempCr = t.Tyres.LrTempCr,
                    RrTempCl = t.Tyres.RrTempCl,
                    RrTempCm = t.Tyres.RrTempCm,
                    RrTempCr = t.Tyres.RrTempCr,
                    LfWearAvg = t.Tyres.LfWearAvg,
                    RfWearAvg = t.Tyres.RfWearAvg,
                    LrWearAvg = t.Tyres.LrWearAvg,
                    RrWearAvg = t.Tyres.RrWearAvg,
                    LfWear = t.Tyres.LfWear,
                    RfWear = t.Tyres.RfWear,
                    LrWear = t.Tyres.LrWear,
                    RrWear = t.Tyres.RrWear,
                    TreadLF = t.Tyres.TreadLF,
                    TreadRF = t.Tyres.TreadRF,
                    TreadLR = t.Tyres.TreadLR,
                    TreadRR = t.Tyres.TreadRR,
                    Compound = t.Tyres.Compound
                },
                SectorsDelta = new SectorsDeltaPayload
                {
                    SectorCount = t.SectorCount,
                    LapAllSectorTimes = t.LapAllSectorTimes,
                    SessionBestSectorTimes = t.SessionBestSectorTimes,
                    LapDeltaToSessionBestSectorTimes = t.LapDeltaToSessionBestSectorTimes,
                    SectorIsBest = t.SectorIsBest,
                    LapDeltaToSessionBestLap = t.LapDeltaToSessionBestLap,
                    LapDeltaToSessionOptimalLap = t.LapDeltaToSessionOptimalLap,
                    LapDeltaToDriverBestLap = t.LapDeltaToDriverBestLap,
                    EstLapTime = t.EstLapTime,
                    LapBestLapTime = t.LapBestLapTime,
                    LapLastLapTime = t.LapLastLapTime,
                    LapCurrentLapTime = t.LapCurrentLapTime
                }
                ,Weather = new WeatherPayload
                {
                    AirTemp = t.AirTemp,
                    TrackTemp = t.TrackSurfaceTemp,
                    TrackTempCrew = t.TrackTempCrew,
                    Humidity = t.RelativeHumidity,
                    PressureMb = t.AirPressure,
                    Precipitation = t.Precipitation,
                    FogLevel = t.FogLevel,
                    SolarAltitude = t.SolarAltitude,
                    SolarAzimuth = t.SolarAzimuth,
                    WindVel = t.TrackWindVel,
                    WindDir = t.WindDir,
                    WeatherDeclaredWet = t.WeatherDeclaredWet,
                    TrackGripStatus = t.TrackGripStatus,
                    Forecast = t.ForecastType
                }
            };
        }

        private void StartSdkWithFlags()
        {
            try
            {
                var flagsType = Type.GetType("IRSDKSharper.DefinitionFlags, IRSDKSharper");
                if (flagsType != null)
                {
                    var allValue = Enum.Parse(flagsType, "All");
                    var startMethod = _sdk.GetType().GetMethod("Start", new[] { flagsType });
                    if (startMethod != null)
                    {
                        startMethod.Invoke(_sdk, new[] { allValue });
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao usar DefinitionFlags. Iniciando com Start() padrão.");
            }
            _sdk.Start();
        }
    }
}
