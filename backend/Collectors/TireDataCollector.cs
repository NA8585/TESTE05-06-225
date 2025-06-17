// File: TireDataCollector.cs

using System;
using System.Collections.Generic;
using System.Linq; // Para usar FirstOrDefault
using System.Threading;
using System.Threading.Tasks;
using IRSDKSharper; // Biblioteca correta
using Newtonsoft.Json;
using YamlDotNet.Serialization; // Para desserializar YAML
using YamlDotNet.Serialization.NamingConventions;

namespace SuperBackendNR85IA.Collectors
{
// Esta classe é responsável por coletar dados de telemetria e sessão do iRacing.
public class TireDataCollector
{
    private SdkWrapper sdk; // Wrapper simplificado do IRSDKSharper
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
    private string _lastSessionInfoYaml = string.Empty;

    // Desserializador para o YAML da SessionInfo
    private IDeserializer _sessionInfoDeserializer;

    public TireDataCollector()
    {
        sdk = new SdkWrapper();

        // SdkWrapper utiliza eventos para notificar novas amostras
        sdk.TelemetryUpdated += OnTelemetryUpdated;
        sdk.Connected += OnConnected;
        sdk.Disconnected += OnDisconnected;

        telemetryBatch = new List<TelemetrySnapshot>();
        lastBatchSendTime = DateTime.UtcNow;

        // Configura o desserializador YAML para camelCase (padrão do iRacing)
        _sessionInfoDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    // Inicia o monitoramento e a coleta de dados
    public void StartCollecting()
    {
        cancellationTokenSource = new CancellationTokenSource();
        sdk.Start();
        Console.WriteLine("Coletor de dados de telemetria iniciado. Aguardando conexão com iRacing...");
    }

    // Para o monitoramento e a coleta de dados
    public void StopCollecting()
    {
        cancellationTokenSource?.Cancel();
        sdk.Stop();
        if (telemetryBatch.Count > 0)
        {
            _ = SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
            telemetryBatch.Clear();
        }
        Console.WriteLine("Coletor de dados de telemetria parado.");
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

    // Atualiza dados da SessionInfo quando o YAML muda
    private void UpdateSessionInfo(string sessionInfoYaml)
    {
        Console.WriteLine("Informações da sessão atualizadas.");
        try
        {
            if (string.IsNullOrEmpty(sessionInfoYaml))
            {
                Console.WriteLine("SessionInfo YAML está vazio.");
                return;
            }

            var sessionInfoData = _sessionInfoDeserializer.Deserialize<SessionInfoData>(sessionInfoYaml);

            int playerCarIdx = sessionInfoData.DriverInfo.DriverCarIdx;
            var playerDriver = sessionInfoData.DriverInfo.Drivers.FirstOrDefault(d => d.CarIdx == playerCarIdx);

            if (playerDriver != null && playerDriver.CarSetup != null && playerDriver.CarSetup.Tires != null)
            {
                string compound = playerDriver.CarSetup.Tires.Compound;
                if (!string.IsNullOrEmpty(compound))
                {
                    _currentTireCompound = compound;
                    Console.WriteLine($"Composto de Pneu (do seu setup): {_currentTireCompound}");
                }
                else
                {
                    _currentTireCompound = sessionInfoData.WeekendInfo.WeekendOptions.TireCompound ?? "Não especificado";
                    Console.WriteLine($"Composto de Pneu (padrão da sessão): {_currentTireCompound}");
                }
            }
            else
            {
                _currentTireCompound = sessionInfoData.WeekendInfo.WeekendOptions.TireCompound ?? "Não especificado";
                Console.WriteLine($"Composto de Pneu (padrão da sessão): {_currentTireCompound}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter composto de pneu da SessionInfo: {ex.Message}");
            _currentTireCompound = "Erro";
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

        if (data.SessionInfoYaml != _lastSessionInfoYaml)
        {
            _lastSessionInfoYaml = data.SessionInfoYaml;
            UpdateSessionInfo(_lastSessionInfoYaml);
        }

        float speed = GetSdkValue<float>(data, "Speed") ?? 0f;
        int playerCarIdx = GetSdkValue<int>(data, "PlayerCarIdx") ?? -1;
        int[] carIdxTrackSurface = GetSdkArray<int>(data, "CarIdxTrackSurface");

        bool isInPitStallAndStopped = (speed < 0.1f && carIdxTrackSurface != null &&
                                        playerCarIdx >= 0 && playerCarIdx < carIdxTrackSurface.Length &&
                                        carIdxTrackSurface[playerCarIdx] == (int)TrackSurface.InPitStall);

        if (isInPitStallAndStopped)
        {
            var coreTemps = GetSdkArray<float>(data, "TireTempCore");
            _lastInferredFLColdTemp = coreTemps.ElementAtOrDefault(0);
            _lastInferredFRColdTemp = coreTemps.ElementAtOrDefault(1);
            _lastInferredLRColdTemp = coreTemps.ElementAtOrDefault(2);
            _lastInferredRRColdTemp = coreTemps.ElementAtOrDefault(3);

            _lastInferredFLColdPressure = GetSdkValue<float>(data, "TireLFPressure") ?? 0f;
            _lastInferredFRColdPressure = GetSdkValue<float>(data, "TireRFPressure") ?? 0f;
            _lastInferredLRColdPressure = GetSdkValue<float>(data, "TireLRPressure") ?? 0f;
            _lastInferredRRColdPressure = GetSdkValue<float>(data, "TireRRPressure") ?? 0f;
        }

        var currentSnapshot = new TelemetrySnapshot
        {
            Timestamp = DateTime.UtcNow,
            LapNumber = GetSdkValue<int>(data, "Lap") ?? 0,
            LapDistance = GetSdkValue<float>(data, "LapDistPct") ?? 0f,

            // Popula os dados de cada pneu
            FrontLeftTire = new TireData
            {

                CurrentPressure = GetSdkValue<float>(data, "TireLFPressure") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "TireLFLastHotPressure") ?? 0f,
                ColdPressure = _lastInferredFLColdPressure,
                CurrentTempInternal = GetSdkArray<float>(data, "TireTempL").ElementAtOrDefault(0),
                CurrentTempMiddle = GetSdkArray<float>(data, "TireTempM").ElementAtOrDefault(0),
                CurrentTempExternal = GetSdkArray<float>(data, "TireTempR").ElementAtOrDefault(0),
                CoreTemp = GetSdkArray<float>(data, "TireTempCore").ElementAtOrDefault(0),
                LastHotTemp = GetSdkValue<float>(data, "TireLFLastHotTemp") ?? 0f,
                ColdTemp = _lastInferredFLColdTemp, // Usa o último valor inferido
                Wear = GetSdkValue<float>(data, "TireLFWear") ?? 0f,
                TreadRemaining = GetSdkValue<float>(data, "TireLFTreadRemaining") ?? 0f,
                SlipAngle = GetSdkValue<float>(data, "TireLFSliptAngle") ?? 0f,
                SlipRatio = GetSdkValue<float>(data, "TireLFSliptRatio") ?? 0f,
                Load = GetSdkValue<float>(data, "TireLFLoad") ?? 0f,
                Deflection = GetSdkValue<float>(data, "TireLFDeflection") ?? 0f,
                RollVelocity = GetSdkValue<float>(data, "TireLFRollVel") ?? 0f,
                GroundVelocity = GetSdkValue<float>(data, "TireLFGroundVel") ?? 0f,
                LateralForce = GetSdkValue<float>(data, "TireLFLatForce") ?? 0f,
                LongitudinalForce = GetSdkValue<float>(data, "TireLFLongForce") ?? 0f
            },
            FrontRightTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "TireRFPressure") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "TireRFLastHotPressure") ?? 0f,
                ColdPressure = _lastInferredFRColdPressure,
                CurrentTempInternal = GetSdkArray<float>(data, "TireTempL").ElementAtOrDefault(1),
                CurrentTempMiddle = GetSdkArray<float>(data, "TireTempM").ElementAtOrDefault(1),
                CurrentTempExternal = GetSdkArray<float>(data, "TireTempR").ElementAtOrDefault(1),
                CoreTemp = GetSdkArray<float>(data, "TireTempCore").ElementAtOrDefault(1),
                LastHotTemp = GetSdkValue<float>(data, "TireRFLastHotTemp") ?? 0f,
                ColdTemp = _lastInferredFRColdTemp,
                Wear = GetSdkValue<float>(data, "TireRFWear") ?? 0f,
                TreadRemaining = GetSdkValue<float>(data, "TireRFTreadRemaining") ?? 0f,
                SlipAngle = GetSdkValue<float>(data, "TireRFSliptAngle") ?? 0f,
                SlipRatio = GetSdkValue<float>(data, "TireRFSliptRatio") ?? 0f,
                Load = GetSdkValue<float>(data, "TireRFLoad") ?? 0f,
                Deflection = GetSdkValue<float>(data, "TireRFDeflection") ?? 0f,
                RollVelocity = GetSdkValue<float>(data, "TireRFRollVel") ?? 0f,
                GroundVelocity = GetSdkValue<float>(data, "TireRFGroundVel") ?? 0f,
                LateralForce = GetSdkValue<float>(data, "TireRFLatForce") ?? 0f,
                LongitudinalForce = GetSdkValue<float>(data, "TireRFLongForce") ?? 0f
            },
            RearLeftTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "TireLRPressure") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "TireLRLastHotPressure") ?? 0f,
                ColdPressure = _lastInferredLRColdPressure,
                CurrentTempInternal = GetSdkArray<float>(data, "TireTempL").ElementAtOrDefault(2),
                CurrentTempMiddle = GetSdkArray<float>(data, "TireTempM").ElementAtOrDefault(2),
                CurrentTempExternal = GetSdkArray<float>(data, "TireTempR").ElementAtOrDefault(2),
                CoreTemp = GetSdkArray<float>(data, "TireTempCore").ElementAtOrDefault(2),
                LastHotTemp = GetSdkValue<float>(data, "TireLRLastHotTemp") ?? 0f,
                ColdTemp = _lastInferredLRColdTemp,
                Wear = GetSdkValue<float>(data, "TireLRWear") ?? 0f,
                TreadRemaining = GetSdkValue<float>(data, "TireLRTreadRemaining") ?? 0f,
                SlipAngle = GetSdkValue<float>(data, "TireLRSliptAngle") ?? 0f,
                SlipRatio = GetSdkValue<float>(data, "TireLRSliptRatio") ?? 0f,
                Load = GetSdkValue<float>(data, "TireLRLoad") ?? 0f,
                Deflection = GetSdkValue<float>(data, "TireLRDeflection") ?? 0f,
                RollVelocity = GetSdkValue<float>(data, "TireLRRollVel") ?? 0f,
                GroundVelocity = GetSdkValue<float>(data, "TireLRGroundVel") ?? 0f,
                LateralForce = GetSdkValue<float>(data, "TireLRLatForce") ?? 0f,
                LongitudinalForce = GetSdkValue<float>(data, "TireLRLongForce") ?? 0f
            },
            RearRightTire = new TireData
            {
                CurrentPressure = GetSdkValue<float>(data, "TireRRPressure") ?? 0f,
                LastHotPressure = GetSdkValue<float>(data, "TireRRLastHotPressure") ?? 0f,
                ColdPressure = _lastInferredRRColdPressure,
                CurrentTempInternal = GetSdkArray<float>(data, "TireTempL").ElementAtOrDefault(3),
                CurrentTempMiddle = GetSdkArray<float>(data, "TireTempM").ElementAtOrDefault(3),
                CurrentTempExternal = GetSdkArray<float>(data, "TireTempR").ElementAtOrDefault(3),
                CoreTemp = GetSdkArray<float>(data, "TireTempCore").ElementAtOrDefault(3),
                LastHotTemp = GetSdkValue<float>(data, "TireRRLastHotTemp") ?? 0f,
                ColdTemp = _lastInferredRRColdTemp,
                Wear = GetSdkValue<float>(data, "TireRRWear") ?? 0f,
                TreadRemaining = GetSdkValue<float>(data, "TireRRTreadRemaining") ?? 0f,
                SlipAngle = GetSdkValue<float>(data, "TireRRSliptAngle") ?? 0f,
                SlipRatio = GetSdkValue<float>(data, "TireRRSliptRatio") ?? 0f,
                Load = GetSdkValue<float>(data, "TireRRLoad") ?? 0f,
                Deflection = GetSdkValue<float>(data, "TireRRDeflection") ?? 0f,
                RollVelocity = GetSdkValue<float>(data, "TireRRRollVel") ?? 0f,
                GroundVelocity = GetSdkValue<float>(data, "TireRRGroundVel") ?? 0f,
                LateralForce = GetSdkValue<float>(data, "TireRRLatForce") ?? 0f,
                LongitudinalForce = GetSdkValue<float>(data, "TireRRLongForce") ?? 0f
            },
            Speed = speed,
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
            var jsonBatch = JsonConvert.SerializeObject(batch);

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
