// File: TelemetrySnapshot.cs

using System; // Para DateTime

namespace SuperBackendNR85IA.Collectors
{
// Esta classe representa um "instantâneo" completo dos dados de telemetria em um ponto no tempo.
public class TelemetrySnapshot
{
    public DateTime Timestamp { get; set; } // Momento em que o snapshot foi capturado (UTC)
    public int LapNumber { get; set; }      // Número da volta atual
    public double LapDistance { get; set; } // Distância percorrida na volta atual (0.0 a 1.0)

    // Dados detalhados de cada pneu, usando a classe TireData
    public TireData FrontLeftTire { get; set; }
    public TireData FrontRightTire { get; set; }
    public TireData RearLeftTire { get; set; }
    public TireData RearRightTire { get; set; }

    // Dados gerais do carro
    public double Speed { get; set; }               // Velocidade do carro (metros/segundo)
    public double Rpm { get; set; }                 // Rotações por minuto do motor
    public double VerticalAcceleration { get; set; } // Aceleração vertical (para inferir vibração)
    public double LateralAcceleration { get; set; }  // Aceleração lateral
    public double LongitudinalAcceleration { get; set; } // Aceleração longitudinal

    // Composto de pneu (incluído aqui para conveniência, mas idealmente seria metadado da sessão)
    // NOTA: Conforme discutido, este é um dado estático da sessão/setup.
    // Incluí-lo em cada snapshot gera redundância. Para um banco de dados,
    // seria melhor armazená-lo uma vez por sessão e referenciá-lo.
    public string TireCompound { get; set; }
}

}