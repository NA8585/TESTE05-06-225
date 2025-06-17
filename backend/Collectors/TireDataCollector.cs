// File: TireDataCollector.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Para usar FirstOrDefault
using System.Threading;
using System.Threading.Tasks;
using iRacingSdkWrapper;
using Newtonsoft.Json;

// Esta classe é responsável por coletar dados de telemetria e sessão do iRacing.
public class TireDataCollector
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
        iracingSdk.SessionInfoUpdated += OnSessionInfoUpdated; // Assina o evento de atualização de informações da sessão
        iracingSdk.Connected += OnConnected;       // Assina o evento de conexão
        iracingSdk.Disconnected += OnDisconnected; // Assina o evento de desconexão

        telemetryBatch = new List<TelemetrySnapshot>();
        lastBatchSendTime = DateTime.UtcNow;
    }

    // Inicia o monitoramento e a coleta de dados
    public void StartCollecting()
    {
        cancellationTokenSource = new CancellationTokenSource();
        iracingSdk.Start();
        Console.WriteLine("Coletor de dados de telemetria iniciado. Aguardando conexão com iRacing...");
    }

    // Para o monitoramento e a coleta de dados
    public void StopCollecting()
    {
        cancellationTokenSource?.Cancel(); // Sinaliza para cancelar operações assíncronas
        iracingSdk.Stop(); // Para o SDK
        // Envia qualquer lote restante antes de parar completamente
        if (telemetryBatch.Count > 0)
        {
            _ = SendTelemetryBatchAsync(new List<TelemetrySnapshot>(telemetryBatch));
            telemetryBatch.Clear();
        }
        Console.WriteLine("Coletor de dados de telemetria parado.");
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
        _currentTireCompound = "Unknown"; // Será atualizado pelo OnSessionInfoUpdated
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

    // Evento disparado quando as informações da sessão são atualizadas (ex: início da sessão, mudança de setup)
    private void OnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
    {
        Console.WriteLine("Informações da sessão atualizadas.");
        try
        {
            // Tenta obter o composto de pneu do setup do jogador
            int playerCarIdx = e.SessionInfo.DriverInfo.DriverCarIdx;
            var playerDriver = e.SessionInfo.DriverInfo.Drivers.FirstOrDefault(d => d.CarIdx == playerCarIdx);

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
                    // Fallback para o composto padrão da sessão se não estiver no setup do jogador
                    _currentTireCompound = e.SessionInfo.WeekendInfo.WeekendOptions.TireCompound ?? "Não especificado";
                    Console.WriteLine($"Composto de Pneu (padrão da sessão): {_currentTireCompound}");
                }
            }
            else
            {
                // Fallback para o composto padrão da sessão se o setup do jogador não estiver disponível
                _currentTireCompound = e.SessionInfo.WeekendInfo.WeekendOptions.TireCompound ?? "Não especificado";
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
    private void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdateEventArgs e)
    {
        if (cancellationTokenSource.IsCancellationRequested)
        {
            return; // Sai se o monitoramento foi parado
        }

        // Lógica para inferir Pressão e Temperatura Fria:
        // Consideramos "frio" quando o carro está parado no box.
        bool isInPitStallAndStopped = (e.Telemetry.Speed.Value < 0.1 &&
                                       e.Telemetry.CarIdxTrackSurface.Value[e.Telemetry.PlayerCarIdx.Value] == (int)iRacingSdkWrapper.Bitfields.TrackSurface.InPitStall);

        if (isInPitStallAndStopped)
        {
            // Captura as temperaturas e pressões atuais como "frias"
            // Usamos TireTempCore para temperatura fria, pois é mais estável.
            _lastInferredFLColdTemp = e.Telemetry.TireTempCore.Value[0];
            _lastInferredFRColdTemp = e.Telemetry.TireTempCore.Value[1];
            _lastInferredLRColdTemp = e.Telemetry.TireTempCore.Value[2];
            _lastInferredRRColdTemp = e.Telemetry.TireTempCore.Value[3];

            _lastInferredFLColdPressure = e.Telemetry.TireLFPressure.Value;
            _lastInferredFRColdPressure = e.Telemetry.TireRFPressure.Value;
            _lastInferredLRColdPressure = e.Telemetry.TireLRPressure.Value;
            _lastInferredRRColdPressure = e.Telemetry.TireRRPressure.Value;
        }

        // Cria um novo snapshot de telemetria
        var currentSnapshot = new TelemetrySnapshot
        {
            Timestamp = DateTime.UtcNow,
            LapNumber = e.Telemetry.Lap.Value,
            LapDistance = e.Telemetry.LapDistPct.Value,

            // Popula os dados de cada pneu
            FrontLeftTire = new TireData
            {
                CurrentPressure = e.Telemetry.TireLFPressure.Value,
                LastHotPressure = e.Telemetry.TireLFLastHotPressure.Value,
                ColdPressure = _lastInferredFLColdPressure, // Usa o último valor inferido
                CurrentTempInternal = e.Telemetry.TireTempL.Value[0],
                CurrentTempMiddle = e.Telemetry.TireTempM.Value[0],
                CurrentTempExternal = e.Telemetry.TireTempR.Value[0],
                CoreTemp = e.Telemetry.TireTempCore.Value[0],
                LastHotTemp = e.Telemetry.TireLFLastHotTemp.Value,
                ColdTemp = _lastInferredFLColdTemp, // Usa o último valor inferido
                Wear = e.Telemetry.TireLFWear.Value,
                TreadRemaining = e.Telemetry.TireLFTreadRemaining.Value,
                SlipAngle = e.Telemetry.TireLFSliptAngle.Value,
                SlipRatio = e.Telemetry.TireLFSliptRatio.Value,
                Load = e.Telemetry.TireLFLoad.Value,
                Deflection = e.Telemetry.TireLFDeflection.Value,
                RollVelocity = e.Telemetry.TireLFRollVel.Value,
                GroundVelocity = e.Telemetry.TireLFGroundVel.Value,
                LateralForce = e.Telemetry.TireLFLatForce.Value,
                LongitudinalForce = e.Telemetry.TireLFLongForce.Value
            },
            FrontRightTire = new TireData
            {
                CurrentPressure = e.Telemetry.TireRFPressure.Value,
                LastHotPressure = e.Telemetry.TireRFLastHotPressure.Value,
                ColdPressure = _lastInferredFRColdPressure,
                CurrentTempInternal = e.Telemetry.TireTempL.Value[1],
                CurrentTempMiddle = e.Telemetry.TireTempM.Value[1],
                CurrentTempExternal = e.Telemetry.TireTempR.Value[1],
                CoreTemp = e.Telemetry.TireTempCore.Value[1],
                LastHotTemp = e.Telemetry.TireRFLastHotTemp.Value,
                ColdTemp = _lastInferredFRColdTemp,
                Wear = e.Telemetry.TireRFWear.Value,
                TreadRemaining = e.Telemetry.TireRFTreadRemaining.Value,
                SlipAngle = e.Telemetry.TireRFSliptAngle.Value,
                SlipRatio = e.Telemetry.TireRFSliptRatio.Value,
                Load = e.Telemetry.TireRFLoad.Value,
                Deflection = e.Telemetry.TireRFDeflection.Value,
                RollVelocity = e.Telemetry.TireRFRollVel.Value,
                GroundVelocity = e.Telemetry.TireRFGroundVel.Value,
                LateralForce = e.Telemetry.TireRFLatForce.Value,
                LongitudinalForce = e.Telemetry.TireRFLongForce.Value
            },
            RearLeftTire = new TireData
            {
                CurrentPressure = e.Telemetry.TireLRPressure.Value,
                LastHotPressure = e.Telemetry.TireLRLastHotPressure.Value,
                ColdPressure = _lastInferredLRColdPressure,
                CurrentTempInternal = e.Telemetry.TireTempL.Value[2],
                CurrentTempMiddle = e.Telemetry.TireTempM.Value[2],
                CurrentTempExternal = e.Telemetry.TireTempR.Value[2],
                CoreTemp = e.Telemetry.TireTempCore.Value[2],
                LastHotTemp = e.Telemetry.TireLRLastHotTemp.Value,
                ColdTemp = _lastInferredLRColdTemp,
                Wear = e.Telemetry.TireLRWear.Value,
                TreadRemaining = e.Telemetry.TireLRTreadRemaining.Value,
                SlipAngle = e.Telemetry.TireLRSliptAngle.Value,
                SlipRatio = e.Telemetry.TireLRSliptRatio.Value,
                Load = e.Telemetry.TireLRLoad.Value,
                Deflection = e.Telemetry.TireLRDeflection.Value,
                RollVelocity = e.Telemetry.TireLRRollVel.Value,
                GroundVelocity = e.Telemetry.TireLRGroundVel.Value,
                LateralForce = e.Telemetry.TireLRLatForce.Value,
                LongitudinalForce = e.Telemetry.TireLRLongForce.Value
            },
            RearRightTire = new TireData
            {
                CurrentPressure = e.Telemetry.TireRRPressure.Value,
                LastHotPressure = e.Telemetry.TireRRLastHotPressure.Value,
                ColdPressure = _lastInferredRRColdPressure,
                CurrentTempInternal = e.Telemetry.TireTempL.Value[3],
                CurrentTempMiddle = e.Telemetry.TireTempM.Value[3],
                CurrentTempExternal = e.Telemetry.TireTempR.Value[3],
                CoreTemp = e.Telemetry.TireTempCore.Value[3],
                LastHotTemp = e.Telemetry.TireRRLastHotTemp.Value,
                ColdTemp = _lastInferredRRColdTemp,
                Wear = e.Telemetry.TireRRWear.Value,
                TreadRemaining = e.Telemetry.TireRRTreadRemaining.Value,
                SlipAngle = e.Telemetry.TireRRSliptAngle.Value,
                SlipRatio = e.Telemetry.TireRRSliptRatio.Value,
                Load = e.Telemetry.TireRRLoad.Value,
                Deflection = e.Telemetry.TireRRDeflection.Value,
                RollVelocity = e.Telemetry.TireRRRollVel.Value,
                GroundVelocity = e.Telemetry.TireRRGroundVel.Value,
                LateralForce = e.Telemetry.TireRRLatForce.Value,
                LongitudinalForce = e.Telemetry.TireRRLongForce.Value
            },
            Speed = e.Telemetry.Speed.Value,
            Rpm = e.Telemetry.RPM.Value,
            VerticalAcceleration = e.Telemetry.VertAcc.Value,
            LateralAcceleration = e.Telemetry.LatAcc.Value,
            LongitudinalAcceleration = e.Telemetry.LongAcc.Value,
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
            var jsonBatch = JsonConvert.SerializeObject(batch, Formatting.None);

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

    // Método Main, ponto de entrada da aplicação
    public static void Main(string[] args)
    {
        var collector = new TireDataCollector();
        collector.StartCollecting();

        Console.WriteLine("\nColetor de Telemetria iRacing em execução. Pressione qualquer tecla para parar.");
        Console.ReadKey(); // Espera por uma tecla para encerrar

        collector.StopCollecting();
        Console.WriteLine("Aplicação encerrada.");
    }
}
