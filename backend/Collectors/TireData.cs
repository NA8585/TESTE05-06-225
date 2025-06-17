// Esta classe representa todos os dados de um único pneu em um determinado instante.
public class TireData
{
    // Pressões (geralmente em psi ou kPa, dependendo da configuração do iRacing)
    public double CurrentPressure { get; set; }     // Pressão atual do pneu
    public double LastHotPressure { get; set; }     // Última pressão quente registrada (após o carro parar ou pit stop)
    public double ColdPressure { get; set; }        // Pressão fria inferida (capturada quando o carro está parado no box)

    // Temperaturas (geralmente em Celsius)
    public double CurrentTempInternal { get; set; } // Temperatura da banda de rodagem - lado interno
    public double CurrentTempMiddle { get; set; }   // Temperatura da banda de rodagem - meio
    public double CurrentTempExternal { get; set; } // Temperatura da banda de rodagem - lado externo
    public double CoreTemp { get; set; }            // Temperatura do núcleo do pneu
    public double LastHotTemp { get; set; }         // Última temperatura quente registrada (após o carro parar ou pit stop)
    public double ColdTemp { get; set; }            // Temperatura fria inferida (capturada quando o carro está parado no box)

    // Desgaste e Borracha Restante (valores de 0.0 a 1.0)
    public double Wear { get; set; }                // Desgaste do pneu (0.0 = novo, 1.0 = 100% desgastado)
    public double TreadRemaining { get; set; }      // Borracha restante (1.0 = 100% restante, 0.0 = 0% restante)

    // Dinâmica do Pneu
    public double SlipAngle { get; set; }           // Ângulo de deslizamento do pneu (radianos)
    public double SlipRatio { get; set; }           // Razão de deslizamento do pneu
    public double Load { get; set; }                // Carga vertical no pneu (Newtons)
    public double Deflection { get; set; }          // Deflexão/compressão do pneu (metros)
    public double RollVelocity { get; set; }        // Velocidade de rotação do pneu (radianos/segundo)
    public double GroundVelocity { get; set; }      // Velocidade do pneu em relação ao solo (metros/segundo)
    public double LateralForce { get; set; }        // Força lateral gerada pelo pneu (Newtons)
    public double LongitudinalForce { get; set; }   // Força longitudinal gerada pelo pneu (Newtons)

    // Você pode adicionar mais propriedades aqui se o iRacing SDK fornecer e forem relevantes para sua análise,
    // como TireTempInnerAir, ou dados de suspensão específicos por roda se preferir agrupá-los aqui.
}
