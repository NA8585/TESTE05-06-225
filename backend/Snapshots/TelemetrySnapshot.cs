// File: TelemetrySnapshot.cs
using System;

namespace SuperBackendNR85IA.Snapshots
{
    // Esta classe representa um "instantâneo" completo dos dados de telemetria em um ponto no tempo.
    public class TelemetrySnapshot
    {
        public DateTime Timestamp { get; set; } // Momento em que o snapshot foi capturado (UTC)
        public int LapNumber { get; set; }      // Número da volta atual
        public float LapDistance { get; set; } // Distância percorrida na volta atual (0.0 a 1.0)

        // Dados detalhados de cada pneu, usando a classe TireData
        public TireData FrontLeftTire { get; set; } = new TireData();
        public TireData FrontRightTire { get; set; } = new TireData();
        public TireData RearLeftTire { get; set; } = new TireData();
        public TireData RearRightTire { get; set; } = new TireData();

        // Dados gerais do carro
        public float Speed { get; set; }               // Velocidade do carro (metros/segundo)
        public float Rpm { get; set; }                 // Rotações por minuto do motor
        public float VerticalAcceleration { get; set; } // Aceleração vertical (para inferir vibração)
        public float LateralAcceleration { get; set; }  // Aceleração lateral
        public float LongitudinalAcceleration { get; set; } // Aceleração longitudinal

        // Composto de pneu (incluído aqui para conveniência, mas idealmente seria metadado da sessão)
        public string TireCompound { get; set; } = string.Empty;
    }
}
