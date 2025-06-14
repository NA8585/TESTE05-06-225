// IRacingTelemetryService.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly SessionYamlParser _yamlParser = new();

        private string _lastYaml = string.Empty;
        private (DriverInfo? Drv, WeekendInfo? Wkd, SessionInfo? Ses, SectorInfo Sec, List<DriverInfo> Drivers) _cachedYamlData;
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

        public IRacingTelemetryService(
            ILogger<IRacingTelemetryService> log,
            TelemetryBroadcaster broadcaster,
            CarTrackDataStore store)
        {
            _log = log;
            _broadcaster = broadcaster;
            _store = store;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _log.LogInformation("IRacingTelemetryService está iniciando.");

            _sdk.OnConnected += () => _log.LogInformation("SDK Conectado ao iRacing.");
            _sdk.OnDisconnected += () => _log.LogInformation("SDK Desconectado do iRacing.");
            _sdk.OnException += (ex) => _log.LogError(ex, "Exceção no IRSDKSharper.");

            try
            {
                _sdk.Start(); // Inicia o monitoramento do iRacing
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

                            await _broadcaster.BroadcastTelemetry(telemetryModel);
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
            UpdateLastHotPress(t);
            await ApplyYamlData(d, t);
            RunCustomCalculations(d, t);
            await PersistCarTrackData(t);

            return t;
        }

        private void UpdateLastHotPress(TelemetryModel t)
        {
            if (t.OnPitRoad && !_wasOnPitRoad)
            {
                // Entrando nos boxes: registra pressões e temperaturas quentes
                _lfLastHotPress = t.LfPress;
                _rfLastHotPress = t.RfPress;
                _lrLastHotPress = t.LrPress;
                _rrLastHotPress = t.RrPress;

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
            }
            else
            {
                // Valores iniciais caso o serviço seja iniciado no meio da pista
                if (_lfColdPress == 0f) _lfColdPress = t.LfPress;
                if (_rfColdPress == 0f) _rfColdPress = t.RfPress;
                if (_lrColdPress == 0f) _lrColdPress = t.LrPress;
                if (_rrColdPress == 0f) _rrColdPress = t.RrPress;

                if (_lfLastHotPress == 0f && t.OnPitRoad) _lfLastHotPress = t.LfPress;
                if (_rfLastHotPress == 0f && t.OnPitRoad) _rfLastHotPress = t.RfPress;
                if (_lrLastHotPress == 0f && t.OnPitRoad) _lrLastHotPress = t.LrPress;
                if (_rrLastHotPress == 0f && t.OnPitRoad) _rrLastHotPress = t.RrPress;
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
        }
    }
}
