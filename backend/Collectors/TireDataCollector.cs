// File: TireDataCollector.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using irsdksharper; // Usando irsdksharper
using Microsoft.Extensions.Hosting;
using SuperBackendNR85IA.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SuperBackendNR85IA.Collectors
{
    // Esta classe é responsável por coletar dados de telemetria e sessão do iRacing.
    public class TireDataCollector : BackgroundService
    {
        private readonly Services.TelemetryBroadcaster _broadcaster;
        private IrSdkClient irsdkClient;
        private CancellationTokenSource cancellationTokenSource;
        private List<TelemetrySnapshot> telemetryBatch;
        private readonly int batchSize = 30;
        private readonly TimeSpan batchInterval = TimeSpan.FromSeconds(1);
        private DateTime lastBatchSendTime;

        // Variáveis para armazenar a última temperatura fria inferida por pneu
        private double _lastInferredFLColdTemp = 0;
        private double _lastInferredFRColdTemp = 0;
        private double _lastInferredLRColdTemp = 0;
        private double _lastInferredRRColdTemp = 0;

        // Variáveis para armazenar a última pressão fria inferida por pneu
        private double _lastInferredFLColdPressure = 0;
        private double _lastInferredFRColdPressure = 0;
        private double _lastInferredLRColdPressure = 0;
        private double _lastInferredRRColdPressure = 0;

        // Variável para armazenar o composto de pneu atual (obtido da SessionInfo)
        private string _currentTireCompound = "Unknown";

        // Desserializador para o YAML da SessionInfo
        private IDeserializer _sessionInfoDeserializer;
        
        public TireDataCollector(Services.TelemetryBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
            irsdkClient = new IrSdkClient();
            irsdkClient.OnNewData += OnTelemetryUpdated;
            irsdkClient.OnSessionInfoUpdated += OnSessionInfoUpdated;
            irsdkClient.OnConnected += OnConnected;
            irsdkClient.OnDisconnected += OnDisconnected;

            telemetryBatch = new List<TelemetrySnapshot>();
            lastBatchSendTime = DateTime.UtcNow;

            _sessionInfoDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            irsdkClient.Start();
            Console.WriteLine("Coletor de dados de telemetria iniciado. Aguardando conexão com iRacing...");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource?.Cancel();
            irsdkClient.Stop();
            if (telemetryBatch.Count > 0)
            {
                await SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
                telemetryBatch.Clear();
            }
            Console.WriteLine("Coletor de dados de telemetria parado.");
            await base.StopAsync(cancellationToken);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Conectado ao iRacing.");
            lock (telemetryBatch)
            {
                telemetryBatch.Clear();
                lastBatchSendTime = DateTime.UtcNow;
            }
            _lastInferredFLColdTemp = 0; _lastInferredFRColdTemp = 0; _lastInferredLRColdTemp = 0; _lastInferredRRColdTemp = 0;
            _lastInferredFLColdPressure = 0; _lastInferredFRColdPressure = 0; _lastInferredLRColdPressure = 0; _lastInferredRRColdPressure = 0;
            _currentTireCompound = "Unknown";
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Desconectado do iRacing.");
            if (telemetryBatch.Count > 0)
            {
                _ = SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
                telemetryBatch.Clear();
            }
        }

        private void OnSessionInfoUpdated(object sender, EventArgs e)
        {
            Console.WriteLine("Informações da sessão atualizadas.");
            try
            {
                var sessionInfoYaml = irsdkClient.SessionInfo;
                if (string.IsNullOrEmpty(sessionInfoYaml))
                    return;

                var sessionInfoData = _sessionInfoDeserializer.Deserialize<SessionInfoData>(sessionInfoYaml);
                int playerCarIdx = sessionInfoData.DriverInfo.DriverCarIdx;
                var playerDriver = sessionInfoData.DriverInfo.Drivers.FirstOrDefault(d => d.CarIdx == playerCarIdx);

                if (playerDriver?.CarSetup?.Tires?.Compound != null)
                {
                    _currentTireCompound = playerDriver.CarSetup.Tires.Compound;
                }
                else
                {
                    _currentTireCompound = sessionInfoData.WeekendInfo.WeekendOptions.TireCompound ?? "Não especificado";
                }
                Console.WriteLine($"Composto de Pneu: {_currentTireCompound}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter composto de pneu da SessionInfo: {ex.Message}");
                _currentTireCompound = "Erro";
            }
        }

        private void OnTelemetryUpdated(object sender, EventArgs e)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                return;

            float speed = irsdkClient.GetTelemetryValue<float>("Speed");
            int playerCarIdx = irsdkClient.GetTelemetryValue<int>("PlayerCarIdx");
            int[] carIdxTrackSurface = irsdkClient.GetTelemetryValue<int[]>("CarIdxTrackSurface");
            bool isInPitStallAndStopped = (speed < 0.1f && carIdxTrackSurface != null &&
                playerCarIdx >= 0 && playerCarIdx < carIdxTrackSurface.Length && carIdxTrackSurface[playerCarIdx] == (int)TrackSurface.InPitStall);

            if (isInPitStallAndStopped)
            {
                _lastInferredFLColdTemp = irsdkClient.GetTelemetryValue<float>("LFtempCL");
                _lastInferredFRColdTemp = irsdkClient.GetTelemetryValue<float>("RFtempCL");
                _lastInferredLRColdTemp = irsdkClient.GetTelemetryValue<float>("LRtempCL");
                _lastInferredRRColdTemp = irsdkClient.GetTelemetryValue<float>("RRtempCL");

                _lastInferredFLColdPressure = irsdkClient.GetTelemetryValue<float>("LFpress");
                _lastInferredFRColdPressure = irsdkClient.GetTelemetryValue<float>("RFpress");
                _lastInferredLRColdPressure = irsdkClient.GetTelemetryValue<float>("LRpress");
                _lastInferredRRColdPressure = irsdkClient.GetTelemetryValue<float>("RRpress");
            }

            var snapshot = new TelemetrySnapshot
            {
                Timestamp = DateTime.UtcNow,
                LapNumber = irsdkClient.GetTelemetryValue<int>("Lap"),
                LapDistance = irsdkClient.GetTelemetryValue<float>("LapDistPct"),
                FrontLeftTire = BuildTireData(0, _lastInferredFLColdTemp, _lastInferredFLColdPressure,
                    "LFtempCL", "LFtempCM", "LFtempCR", "LFpress", "LFhotPressure"),
                FrontRightTire = BuildTireData(1, _lastInferredFRColdTemp, _lastInferredFRColdPressure,
                    "RFtempCL", "RFtempCM", "RFtempCR", "RFpress", "RFhotPressure"),
                RearLeftTire = BuildTireData(2, _lastInferredLRColdTemp, _lastInferredLRColdPressure,
                    "LRtempCL", "LRtempCM", "LRtempCR", "LRpress", "LRhotPressure"),
                RearRightTire = BuildTireData(3, _lastInferredRRColdTemp, _lastInferredRRColdPressure,
                    "RRtempCL", "RRtempCM", "RRtempCR", "RRpress", "RRhotPressure"),
                Speed = speed,
                Rpm = irsdkClient.GetTelemetryValue<float>("RPM"),
                VerticalAcceleration = irsdkClient.GetTelemetryValue<float>("VertAccel"),
                LateralAcceleration = irsdkClient.GetTelemetryValue<float>("LatAccel"),
                LongitudinalAcceleration = irsdkClient.GetTelemetryValue<float>("LongAccel"),
                TireCompound = _currentTireCompound
            };

            lock (telemetryBatch)
            {
                telemetryBatch.Add(snapshot);
                if (telemetryBatch.Count >= batchSize || (DateTime.UtcNow - lastBatchSendTime) >= batchInterval)
                {
                    var batchToSend = new List<TelemetrySnapshot>(telemetryBatch);
                    telemetryBatch.Clear();
                    lastBatchSendTime = DateTime.UtcNow;
                    _ = SendTelemetryBatchAsync(batchToSend);
                }
            }
        }

        private TireData BuildTireData(int index, double coldTemp, double coldPressure,
            string tempClVar, string tempCmVar, string tempCrVar,
            string pressVar, string hotPressVar)
        {
            return new TireData
            {
                CurrentPressure = irsdkClient.GetTelemetryValue<float>(pressVar),
                LastHotPressure = irsdkClient.GetTelemetryValue<float>(hotPressVar),
                ColdPressure = coldPressure,
                CurrentTempInternal = irsdkClient.GetTelemetryValue<float>(tempClVar),
                CurrentTempMiddle = irsdkClient.GetTelemetryValue<float>(tempCmVar),
                CurrentTempExternal = irsdkClient.GetTelemetryValue<float>(tempCrVar),
                CoreTemp = irsdkClient.GetTelemetryValue<float>(tempClVar),
                LastHotTemp = irsdkClient.GetTelemetryValue<float>(tempClVar),
                ColdTemp = coldTemp,
                // Campos adicionais podem ser coletados aqui conforme necessário
            };
        }

        private async Task SendTelemetryBatchAsync(List<TelemetrySnapshot> batch)
        {
            if (batch == null || batch.Count == 0)
                return;

            try
            {
                await _broadcaster.BroadcastTireSnapshots(batch);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar lote de telemetria para o backend: {ex.Message}");
            }
        }
    }
}
