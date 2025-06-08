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
        private (DriverInfo? Drv, WeekendInfo? Wkd, SessionInfo? Ses,SectorInfo Sec) _cachedYamlData;
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
            UpdateLapInfo(d, t);
            ReadSectorTimes(d, t);
            ComputeForceFeedback(d, t);
            ComputeRelativeDistances(d, t);
            PopulateSessionInfo(d, t);
            PopulateTyres(d, t);
            await ApplyYamlData(d, t);
            RunCustomCalculations(d, t);
            await PersistCarTrackData(t);

            return t;
        }
    }
}
