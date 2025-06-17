// File: TireDataCollector.cs

using System;
using System.Collections.Generic;
using System.Linq; // Para usar FirstOrDefault
using System.Threading;
using System.Threading.Tasks;
using IRSDKSharper;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace SuperBackendNR85IA.Collectors
{
// Esta classe é responsável por coletar dados de telemetria e sessão do iRacing.
public class TireDataCollector : BackgroundService
{
    private SdkWrapper iracingSdk;
    private CancellationTokenSource cancellationTokenSource;
    private List<TelemetrySnapshot> telemetryBatch; // Lista para acumular snapshots antes de enviar
    private readonly int batchSize = 30;           // Número de snapshots por lote (aprox. 0.5 segundos a 60Hz)
    private readonly TimeSpan batchInterval = TimeSpan.FromSeconds(1); // Intervalo máximo para enviar um lote
    private DateTime lastBatchSendTime;            // Timestamp do último envio de lote

    // Variáveis para armazenar a última temperatura fria inferida por pneu
    // Estes valores são persistidos entre atualizações de telemetria
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

    public TireDataCollector()
    {
        iracingSdk = new SdkWrapper();
        iracingSdk.TelemetryUpdateInterval = 16; // Define a frequência de atualização da telemetria (aprox. 60Hz)
        iracingSdk.TelemetryUpdated += OnTelemetryUpdated; // Assina o evento de atualização de telemetria
        iracingSdk.Connected += OnConnected;       // Assina o evento de conexão
        iracingSdk.Disconnected += OnDisconnected; // Assina o evento de desconexão

        telemetryBatch = new List<TelemetrySnapshot>();
        lastBatchSendTime = DateTime.UtcNow;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        iracingSdk.Start();
        Console.WriteLine("Coletor de dados de telemetria iniciado. Aguardando conexão com iRacing...");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource?.Cancel();
        iracingSdk.Stop();
        if (telemetryBatch.Count > 0)
        {
            await SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
            telemetryBatch.Clear();
        }
        Console.WriteLine("Coletor de dados de telemetria parado.");
        await base.StopAsync(cancellationToken);
    }

    private T? GetSdkValue<T>(IRacingSdkData data, string name) where T : struct
    {
        if (!data.TelemetryDataProperties.TryGetValue(name, out var datum) || datum.Count == 0)
            return null;

        object? value = null;
        if (typeof(T) == typeof(float)) value = data.GetFloat(datum);
        else if (typeof(T) == typeof(int)) value = data.GetInt(datum);
        else if (typeof(T) == typeof(bool)) value = data.GetBool(datum);
        else if (typeof(T) == typeof(double)) value = data.GetDouble(datum);
        return (T?)value;
    }

    private T[] GetSdkArray<T>(IRacingSdkData data, string name) where T : struct
    {
        if (!data.TelemetryDataProperties.TryGetValue(name, out var datum) || datum.Count == 0)
            return Array.Empty<T>();

        if (typeof(T) == typeof(float))
        {
            float[] arr = new float[datum.Count];
            data.GetFloatArray(datum, arr, 0, datum.Count);
            return arr.Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(int))
        {
            int[] arr = new int[datum.Count];
            data.GetIntArray(datum, arr, 0, datum.Count);
            return arr.Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(bool))
        {
            bool[] arr = new bool[datum.Count];
            data.GetBoolArray(datum, arr, 0, datum.Count);
            return arr.Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(double))
        {
            double[] arr = new double[datum.Count];
            data.GetDoubleArray(datum, arr, 0, datum.Count);
            return arr.Cast<T>().ToArray();
        }
        return Array.Empty<T>();
    }

    // Evento disparado quando o SDK se conecta ao iRacing
    private void OnConnected(object sender, EventArgs e)
    {
        Console.WriteLine("Conectado ao iRacing.");
        // Limpa o lote e reseta o tempo ao iniciar uma nova conexão/sessão
        lock (telemetryBatch)
        {
            telemetryBatch.Clear();
            lastBatchSendTime = DateTime.UtcNow;
        }
        // Reseta as inferências de frio e o composto de pneu ao conectar
        _lastInferredFLColdTemp = 0; _lastInferredFRColdTemp = 0; _lastInferredLRColdTemp = 0; _lastInferredRRColdTemp = 0;
        _lastInferredFLColdPressure = 0; _lastInferredFRColdPressure = 0; _lastInferredLRColdPressure = 0; _lastInferredRRColdPressure = 0;
        _currentTireCompound = "Default";
    }

    // Evento disparado quando o SDK se desconecta do iRacing
    private void OnDisconnected(object sender, EventArgs e)
    {
        Console.WriteLine("Desconectado do iRacing.");
        // Envia qualquer lote restante ao desconectar
        if (telemetryBatch.Count > 0)
        {
            _ = SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
            telemetryBatch.Clear();
        }
    }

    // Evento disparado a cada atualização de telemetria (alta frequência)
    private void OnTelemetryUpdated(object sender, TelemetryUpdateEventArgs e)
    {
        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        var data = e.Telemetry;

        float speed = GetSdkValue<float>(data, "Speed") ?? 0f;
        int playerIdx = GetSdkValue<int>(data, "PlayerCarIdx") ?? 0;
        var surfaces = GetSdkArray<int>(data, "CarIdxTrackSurface");
        bool isInPitStallAndStopped = speed < 0.1f &&
            surfaces.Length > playerIdx && surfaces[playerIdx] == 1;

        if (isInPitStallAndStopped)
        {
            var coreTemps = GetSdkArray<float>(data, "TireTempCore");
            if (coreTemps.Length >= 4)
            {
                _lastInferredFLColdTemp = coreTemps[0];
                _lastInferredFRColdTemp = coreTemps[1];
                _lastInferredLRColdTemp = coreTemps[2];
                _lastInferredRRColdTemp = coreTemps[3];
            }

            _lastInferredFLColdPressure = GetSdkValue<float>(data, "LFpress") ?? 0f;
            _lastInferredFRColdPressure = GetSdkValue<float>(data, "RFpress") ?? 0f;
            _lastInferredLRColdPressure = GetSdkValue<float>(data, "LRpress") ?? 0f;
            _lastInferredRRColdPressure = GetSdkValue<float>(data, "RRpress") ?? 0f;
        }

        var tireTempCore = GetSdkArray<float>(data, "TireTempCore");

        var snapshot = new TelemetrySnapshot
        {
            Timestamp = DateTime.UtcNow,
            LapNumber = GetSdkValue<int>(data, "Lap") ?? 0,
            LapDistance = GetSdkValue<float>(data, "LapDistPct") ?? 0f,

            // Popula os dados de cada pneu
            FrontLeftTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "LFpress") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "LFhotPressure") ?? 0f,
                ColdPressure = _lastInferredFLColdPressure,
                CurrentTempInternal = GetSdkValue<float>(data, "LFtempCL") ?? 0f,
                CurrentTempMiddle = GetSdkValue<float>(data, "LFtempCM") ?? 0f,
                CurrentTempExternal = GetSdkValue<float>(data, "LFtempCR") ?? 0f,
                CoreTemp = tireTempCore.Length > 0 ? tireTempCore[0] : 0f,
                LastHotTemp = GetSdkValue<float>(data, "LFhotTemp") ?? 0f,
                ColdTemp = _lastInferredFLColdTemp,
                Wear = 0,
                TreadRemaining = 0,
                SlipAngle = 0,
                SlipRatio = 0,
                Load = 0,
                Deflection = 0,
                RollVelocity = 0,
                GroundVelocity = 0,
                LateralForce = 0,
                LongitudinalForce = 0
            },
            FrontRightTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "RFpress") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "RFhotPressure") ?? 0f,
                ColdPressure = _lastInferredFRColdPressure,
                CurrentTempInternal = GetSdkValue<float>(data, "RFtempCL") ?? 0f,
                CurrentTempMiddle = GetSdkValue<float>(data, "RFtempCM") ?? 0f,
                CurrentTempExternal = GetSdkValue<float>(data, "RFtempCR") ?? 0f,
                CoreTemp = tireTempCore.Length > 1 ? tireTempCore[1] : 0f,
                LastHotTemp = GetSdkValue<float>(data, "RFhotTemp") ?? 0f,
                ColdTemp = _lastInferredFRColdTemp,
                Wear = 0,
                TreadRemaining = 0,
                SlipAngle = 0,
                SlipRatio = 0,
                Load = 0,
                Deflection = 0,
                RollVelocity = 0,
                GroundVelocity = 0,
                LateralForce = 0,
                LongitudinalForce = 0
            },
            RearLeftTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "LRpress") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "LRhotPressure") ?? 0f,
                ColdPressure = _lastInferredLRColdPressure,
                CurrentTempInternal = GetSdkValue<float>(data, "LRtempCL") ?? 0f,
                CurrentTempMiddle = GetSdkValue<float>(data, "LRtempCM") ?? 0f,
                CurrentTempExternal = GetSdkValue<float>(data, "LRtempCR") ?? 0f,
                CoreTemp = tireTempCore.Length > 2 ? tireTempCore[2] : 0f,
                LastHotTemp = GetSdkValue<float>(data, "LRhotTemp") ?? 0f,
                ColdTemp = _lastInferredLRColdTemp,
                Wear = 0,
                TreadRemaining = 0,
                SlipAngle = 0,
                SlipRatio = 0,
                Load = 0,
                Deflection = 0,
                RollVelocity = 0,
                GroundVelocity = 0,
                LateralForce = 0,
                LongitudinalForce = 0
            },
            RearRightTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "RRpress") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "RRhotPressure") ?? 0f,
                ColdPressure = _lastInferredRRColdPressure,
                CurrentTempInternal = GetSdkValue<float>(data, "RRtempCL") ?? 0f,
                CurrentTempMiddle = GetSdkValue<float>(data, "RRtempCM") ?? 0f,
                CurrentTempExternal = GetSdkValue<float>(data, "RRtempCR") ?? 0f,
                CoreTemp = tireTempCore.Length > 3 ? tireTempCore[3] : 0f,
                LastHotTemp = GetSdkValue<float>(data, "RRhotTemp") ?? 0f,
                ColdTemp = _lastInferredRRColdTemp,
                Wear = 0,
                TreadRemaining = 0,
                SlipAngle = 0,
                SlipRatio = 0,
                Load = 0,
                Deflection = 0,
                RollVelocity = 0,
                GroundVelocity = 0,
                LateralForce = 0,
                LongitudinalForce = 0
            },
            Speed = GetSdkValue<float>(data, "Speed") ?? 0f,
            Rpm = GetSdkValue<float>(data, "RPM") ?? 0f,
            VerticalAcceleration = GetSdkValue<float>(data, "VertAcc") ?? 0f,
            LateralAcceleration = GetSdkValue<float>(data, "LatAcc") ?? 0f,
            LongitudinalAcceleration = GetSdkValue<float>(data, "LongAcc") ?? 0f,
            TireCompound = _currentTireCompound // Inclui o composto de pneu no snapshot
        };

        // Adiciona o snapshot ao lote
        lock (telemetryBatch) // Protege o acesso à lista compartilhada
        {
            telemetryBatch.Add(currentSnapshot);

            // Verifica se é hora de enviar o lote (atingiu o tamanho ou o intervalo de tempo)
            if (telemetryBatch.Count >= batchSize || (DateTime.UtcNow - lastBatchSendTime) >= batchInterval)
            {
                // Cria uma cópia do lote para enviar, para que a lista original possa ser limpa e continuar coletando
                var batchToSend = new List<TelemetrySnapshot>(telemetryBatch);
                telemetryBatch.Clear(); // Limpa o lote atual
                lastBatchSendTime = DateTime.UtcNow; // Reseta o tempo do último envio

                // Envia o lote de forma assíncrona para não bloquear a thread de telemetria
                _ = SendTelemetryBatchAsync(batchToSend);
            }
        }
    }

    // Método assíncrono simulado para enviar dados para o backend
    private async Task SendTelemetryBatchAsync(List<TelemetrySnapshot> batch)
    {
        if (batch == null || batch.Count == 0)
        {
            return;
        }

        try
        {
            // Simula um atraso de rede ou processamento de backend
            await Task.Delay(50);

            // Serializa o lote de snapshots para JSON
            var jsonBatch = JsonSerializer.Serialize(batch);

            // Imprime uma mensagem no console (em um cenário real, você enviaria isso para uma API)
            Console.WriteLine($"Enviando lote de {batch.Count} snapshots. Primeiro timestamp: {batch[0].Timestamp:HH:mm:ss.fff}, Composto: {batch[0].TireCompound}");

            // Exemplo de como você enviaria para uma API HTTP:
            // using (var httpClient = new HttpClient())
            // {
            //     var content = new StringContent(jsonBatch, System.Text.Encoding.UTF8, "application/json");
            //     var response = await httpClient.PostAsync("SUA_URL_DA_API_DE_BACKEND", content);
            //     response.EnsureSuccessStatusCode(); // Lança exceção se o status code não for de sucesso
            //     Console.WriteLine($"Lote enviado com sucesso. Resposta: {await response.Content.ReadAsStringAsync()}");
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar lote de telemetria para o backend: {ex.Message}");
            // Implementar lógica de retry, logging de erro, ou armazenamento local para reenvio posterior
        }
    }

}

}
