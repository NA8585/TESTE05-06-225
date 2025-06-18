namespace SuperBackendNR85IA.Snapshots
{
    // Esta classe representa todos os dados de um único pneu em um determinado instante.
    public class TireData
    {
        // Pressões (geralmente em psi ou kPa, dependendo da configuração do iRacing)
        public float CurrentPressure { get; set; }     // Pressão atual do pneu
        public float LastHotPressure { get; set; }     // Última pressão quente registrada (após o carro parar ou pit stop)
        public float ColdPressure { get; set; }        // Pressão fria inferida (capturada quando o carro está parado no box)

        // Temperaturas (geralmente em Celsius)
        public float CurrentTempInternal { get; set; } // Temperatura da banda de rodagem - lado interno (TireTempL)
        public float CurrentTempMiddle { get; set; }   // Temperatura da banda de rodagem - meio (TireTempM)
        public float CurrentTempExternal { get; set; } // Temperatura da banda de rodagem - lado externo (TireTempR)

        // Temperaturas registradas em diferentes momentos
        public float LastHotTempInternal { get; set; } // Temperatura interna na última parada
        public float LastHotTempMiddle { get; set; }   // Temperatura central na última parada
        public float LastHotTempExternal { get; set; } // Temperatura externa na última parada
        public float ColdTempInternal { get; set; }    // Temperatura interna quando frio
        public float ColdTempMiddle { get; set; }      // Temperatura central quando frio
        public float ColdTempExternal { get; set; }    // Temperatura externa quando frio

        public float CoreTemp { get; set; }            // Temperatura média do núcleo do pneu
        public float LastHotTemp { get; set; }         // Temperatura média quente registrada
        public float ColdTemp { get; set; }            // Temperatura média fria inferida

        // Desgaste e Borracha Restante (valores de 0.0 a 1.0)
        public float Wear { get; set; }                // Desgaste do pneu (0.0 = novo, 1.0 = 100% desgastado)
        public float TreadRemaining { get; set; }      // Borracha restante (1.0 = 100% restante, 0.0 = 0% restante)

        // Dinâmica do Pneu
        public float SlipAngle { get; set; }           // Ângulo de deslizamento do pneu (radianos)
        public float SlipRatio { get; set; }           // Razão de deslizamento do pneu
        public float Load { get; set; }                // Carga vertical no pneu (Newtons)
        public float Deflection { get; set; }          // Deflexão/compressão do pneu (metros)
        public float RollVelocity { get; set; }        // Velocidade de rotação do pneu (radianos/segundo)
        public float GroundVelocity { get; set; }      // Velocidade do pneu em relação ao solo (metros/segundo)
        public float LateralForce { get; set; }        // Força lateral gerada pelo pneu (Newtons)
        public float LongitudinalForce { get; set; }   // Força longitudinal gerada pelo pneu (Newtons)
    }
}
