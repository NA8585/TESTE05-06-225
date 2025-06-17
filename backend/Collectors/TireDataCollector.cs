// File: TireDataCollector.cs

using System;
using System.Collections.Generic;
using System.Linq; // Para usar FirstOrDefault
using System.Threading;
using System.Threading.Tasks;
using irsdksharper; // Biblioteca correta
using Newtonsoft.Json;
using YamlDotNet.Serialization; // Para desserializar YAML
using YamlDotNet.Serialization.NamingConventions;

namespace SuperBackendNR85IA.Collectors
{
// Esta classe é responsável por coletar dados de telemetria e sessão do iRacing.
public class TireDataCollector
{
    private IrSdkClient irsdkClient; // Instância do IRSDKSharper
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

    // Desserializador para o YAML da SessionInfo
    private IDeserializer _sessionInfoDeserializer;

    public TireDataCollector()
    {

        irsdkClient = new IrSdkClient();

        // irsdksharper utiliza eventos para notificar novas amostras
        irsdkClient.OnNewData += OnTelemetryUpdated;
        irsdkClient.OnSessionInfoUpdated += OnSessionInfoUpdated;
        irsdkClient.OnConnected += OnConnected;
        irsdkClient.OnDisconnected += OnDisconnected;

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
        irsdkClient.Start();
        Console.WriteLine("Coletor de dados de telemetria iniciado. Aguardando conexão com iRacing...");
    }

    // Para o monitoramento e a coleta de dados
    public void StopCollecting()
    {
        cancellationTokenSource?.Cancel();
        irsdkClient.Stop();
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

    // Evento disparado quando as informações da sessão são atualizadas
    private void OnSessionInfoUpdated(object sender, EventArgs e)
    {
        Console.WriteLine("Informações da sessão atualizadas.");
        try
        {
            var sessionInfoYaml = irsdkClient.SessionInfo;
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
    private void OnTelemetryUpdated(object sender, EventArgs e)
    {
        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        float speed = irsdkClient.GetTelemetryValue<float>("Speed");
        int playerCarIdx = irsdkClient.GetTelemetryValue<int>("PlayerCarIdx");
        int[] carIdxTrackSurface = irsdkClient.GetTelemetryValue<int[]>("CarIdxTrackSurface");

        bool isInPitStallAndStopped = (speed < 0.1f && carIdxTrackSurface != null &&
                                        playerCarIdx >= 0 && playerCarIdx < carIdxTrackSurface.Length &&
                                        carIdxTrackSurface[playerCarIdx] == (int)TrackSurface.InPitStall);

        if (isInPitStallAndStopped)
        {
            _lastInferredFLColdTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 0);
            _lastInferredFRColdTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 1);
            _lastInferredLRColdTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 2);
            _lastInferredRRColdTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 3);

            _lastInferredFLColdPressure = irsdkClient.GetTelemetryValue<float>("TireLFPressure");
            _lastInferredFRColdPressure = irsdkClient.GetTelemetryValue<float>("TireRFPressure");
            _lastInferredLRColdPressure = irsdkClient.GetTelemetryValue<float>("TireLRPressure");
            _lastInferredRRColdPressure = irsdkClient.GetTelemetryValue<float>("TireRRPressure");
        }

        var currentSnapshot = new TelemetrySnapshot
        {
            Timestamp = DateTime.UtcNow,
            LapNumber = irsdkClient.GetTelemetryValue<int>("Lap"),
            LapDistance = irsdkClient.GetTelemetryValue<float>("LapDistPct"),

            // Popula os dados de cada pneu
            FrontLeftTire = new TireData
            {

                CurrentPressure = irsdkClient.GetTelemetryValue<float>("TireLFPressure"),
                LastHotPressure = irsdkClient.GetTelemetryValue<float>("TireLFLastHotPressure"),
                ColdPressure = _lastInferredFLColdPressure,
                CurrentTempInternal = irsdkClient.GetTelemetryValue<float>("TireTempL", 0),
                CurrentTempMiddle = irsdkClient.GetTelemetryValue<float>("TireTempM", 0),
                CurrentTempExternal = irsdkClient.GetTelemetryValue<float>("TireTempR", 0),
                CoreTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 0),
                LastHotTemp = irsdkClient.GetTelemetryValue<float>("TireLFLastHotTemp"),
                ColdTemp = _lastInferredFLColdTemp, // Usa o último valor inferido
                Wear = irsdkClient.GetTelemetryValue<float>("TireLFWear"),
                TreadRemaining = irsdkClient.GetTelemetryValue<float>("TireLFTreadRemaining"),
                SlipAngle = irsdkClient.GetTelemetryValue<float>("TireLFSliptAngle"),
                SlipRatio = irsdkClient.GetTelemetryValue<float>("TireLFSliptRatio"),
                Load = irsdkClient.GetTelemetryValue<float>("TireLFLoad"),
                Deflection = irsdkClient.GetTelemetryValue<float>("TireLFDeflection"),
                RollVelocity = irsdkClient.GetTelemetryValue<float>("TireLFRollVel"),
                GroundVelocity = irsdkClient.GetTelemetryValue<float>("TireLFGroundVel"),
                LateralForce = irsdkClient.GetTelemetryValue<float>("TireLFLatForce"),
                LongitudinalForce = irsdkClient.GetTelemetryValue<float>("TireLFLongForce")
            },
            FrontRightTire = new TireData
            {
                CurrentPressure = irsdkClient.GetTelemetryValue<float>("TireRFPressure"),
                LastHotPressure = irsdkClient.GetTelemetryValue<float>("TireRFLastHotPressure"),
                ColdPressure = _lastInferredFRColdPressure,
                CurrentTempInternal = irsdkClient.GetTelemetryValue<float>("TireTempL", 1),
                CurrentTempMiddle = irsdkClient.GetTelemetryValue<float>("TireTempM", 1),
                CurrentTempExternal = irsdkClient.GetTelemetryValue<float>("TireTempR", 1),
                CoreTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 1),
                LastHotTemp = irsdkClient.GetTelemetryValue<float>("TireRFLastHotTemp"),
                ColdTemp = _lastInferredFRColdTemp,
                Wear = irsdkClient.GetTelemetryValue<float>("TireRFWear"),
                TreadRemaining = irsdkClient.GetTelemetryValue<float>("TireRFTreadRemaining"),
                SlipAngle = irsdkClient.GetTelemetryValue<float>("TireRFSliptAngle"),
                SlipRatio = irsdkClient.GetTelemetryValue<float>("TireRFSliptRatio"),
                Load = irsdkClient.GetTelemetryValue<float>("TireRFLoad"),
                Deflection = irsdkClient.GetTelemetryValue<float>("TireRFDeflection"),
                RollVelocity = irsdkClient.GetTelemetryValue<float>("TireRFRollVel"),
                GroundVelocity = irsdkClient.GetTelemetryValue<float>("TireRFGroundVel"),
                LateralForce = irsdkClient.GetTelemetryValue<float>("TireRFLatForce"),
                LongitudinalForce = irsdkClient.GetTelemetryValue<float>("TireRFLongForce")
            },
            RearLeftTire = new TireData
            {
                CurrentPressure = irsdkClient.GetTelemetryValue<float>("TireLRPressure"),
                LastHotPressure = irsdkClient.GetTelemetryValue<float>("TireLRLastHotPressure"),
                ColdPressure = _lastInferredLRColdPressure,
                CurrentTempInternal = irsdkClient.GetTelemetryValue<float>("TireTempL", 2),
                CurrentTempMiddle = irsdkClient.GetTelemetryValue<float>("TireTempM", 2),
                CurrentTempExternal = irsdkClient.GetTelemetryValue<float>("TireTempR", 2),
                CoreTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 2),
                LastHotTemp = irsdkClient.GetTelemetryValue<float>("TireLRLastHotTemp"),
                ColdTemp = _lastInferredLRColdTemp,
                Wear = irsdkClient.GetTelemetryValue<float>("TireLRWear"),
                TreadRemaining = irsdkClient.GetTelemetryValue<float>("TireLRTreadRemaining"),
                SlipAngle = irsdkClient.GetTelemetryValue<float>("TireLRSliptAngle"),
                SlipRatio = irsdkClient.GetTelemetryValue<float>("TireLRSliptRatio"),
                Load = irsdkClient.GetTelemetryValue<float>("TireLRLoad"),
                Deflection = irsdkClient.GetTelemetryValue<float>("TireLRDeflection"),
                RollVelocity = irsdkClient.GetTelemetryValue<float>("TireLRRollVel"),
                GroundVelocity = irsdkClient.GetTelemetryValue<float>("TireLRGroundVel"),
                LateralForce = irsdkClient.GetTelemetryValue<float>("TireLRLatForce"),
                LongitudinalForce = irsdkClient.GetTelemetryValue<float>("TireLRLongForce")
            },
            RearRightTire = new TireData
            {
                CurrentPressure = irsdkClient.GetTelemetryValue<float>("TireRRPressure"),
                LastHotPressure = irsdkClient.GetTelemetryValue<float>("TireRRLastHotPressure"),
                ColdPressure = _lastInferredRRColdPressure,
                CurrentTempInternal = irsdkClient.GetTelemetryValue<float>("TireTempL", 3),
                CurrentTempMiddle = irsdkClient.GetTelemetryValue<float>("TireTempM", 3),
                CurrentTempExternal = irsdkClient.GetTelemetryValue<float>("TireTempR", 3),
                CoreTemp = irsdkClient.GetTelemetryValue<float>("TireTempCore", 3),
                LastHotTemp = irsdkClient.GetTelemetryValue<float>("TireRRLastHotTemp"),
                ColdTemp = _lastInferredRRColdTemp,
                Wear = irsdkClient.GetTelemetryValue<float>("TireRRWear"),
                TreadRemaining = irsdkClient.GetTelemetryValue<float>("TireRRTreadRemaining"),
                SlipAngle = irsdkClient.GetTelemetryValue<float>("TireRRSliptAngle"),
                SlipRatio = irsdkClient.GetTelemetryValue<float>("TireRRSliptRatio"),
                Load = irsdkClient.GetTelemetryValue<float>("TireRRLoad"),
                Deflection = irsdkClient.GetTelemetryValue<float>("TireRRDeflection"),
                RollVelocity = irsdkClient.GetTelemetryValue<float>("TireRRRollVel"),
                GroundVelocity = irsdkClient.GetTelemetryValue<float>("TireRRGroundVel"),
                LateralForce = irsdkClient.GetTelemetryValue<float>("TireRRLatForce"),
                LongitudinalForce = irsdkClient.GetTelemetryValue<float>("TireRRLongForce")
            },
            Speed = speed,
            Rpm = irsdkClient.GetTelemetryValue<float>("RPM"),
            VerticalAcceleration = irsdkClient.GetTelemetryValue<float>("VertAcc"),
            LateralAcceleration = irsdkClient.GetTelemetryValue<float>("LatAcc"),
            LongitudinalAcceleration = irsdkClient.GetTelemetryValue<float>("LongAcc"),
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
