using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class CarStatusPayload
    {
        // Dados básicos de movimento
        [JsonPropertyName("speed")] public float Speed { get; set; }
        [JsonPropertyName("rpm")] public float Rpm { get; set; }
        [JsonPropertyName("gear")] public int Gear { get; set; }

        // Pedais
        [JsonPropertyName("throttle")] public float Throttle { get; set; }
        [JsonPropertyName("throttleRaw")] public float ThrottleRaw { get; set; }
        [JsonPropertyName("brake")] public float Brake { get; set; }
        [JsonPropertyName("brakeRaw")] public float BrakeRaw { get; set; }
        [JsonPropertyName("clutch")] public float Clutch { get; set; }
        [JsonPropertyName("handBrake")] public float HandBrake { get; set; }
        [JsonPropertyName("handBrakeRaw")] public float HandBrakeRaw { get; set; }

        // Direção e FFB
        [JsonPropertyName("steeringWheelAngle")] public float SteeringWheelAngle { get; set; }
        [JsonPropertyName("steeringWheelAngleMax")] public float SteeringWheelAngleMax { get; set; }
        [JsonPropertyName("steeringWheelTorque")] public float SteeringWheelTorque { get; set; }
        [JsonPropertyName("steeringWheelPeakForceNm")] public float SteeringWheelPeakForceNm { get; set; }
        [JsonPropertyName("steeringWheelPctDamper")] public float SteeringWheelPctDamper { get; set; }
        [JsonPropertyName("steeringWheelPctTorque")] public float SteeringWheelPctTorque { get; set; }

        // ABS/Traction/Assistências
        [JsonPropertyName("brakeABSactive")] public bool BrakeABSactive { get; set; }
        [JsonPropertyName("brakeABSCutPct")] public float BrakeABSCutPct { get; set; }

        // Combustível
        [JsonPropertyName("fuelLevel")] public float FuelLevel { get; set; }
        [JsonPropertyName("fuelLevelPct")] public float FuelLevelPct { get; set; }
        [JsonPropertyName("fuelUsePerLap")] public float FuelUsePerLap { get; set; }
        [JsonPropertyName("fuelUsePerLapCalc")] public float FuelUsePerLapCalc { get; set; }
        [JsonPropertyName("consumoMedio")] public float ConsumoMedio { get; set; }
        [JsonPropertyName("voltasRestantesMedio")] public float VoltasRestantesMedio { get; set; }

        // Temperaturas e Pressões do carro
        [JsonPropertyName("waterTemp")] public float WaterTemp { get; set; }
        [JsonPropertyName("oilTemp")] public float OilTemp { get; set; }
        [JsonPropertyName("oilPress")] public float OilPress { get; set; }
        [JsonPropertyName("fuelPress")] public float FuelPress { get; set; }
        [JsonPropertyName("manifoldPress")] public float ManifoldPress { get; set; }

        // Status e alertas
        [JsonPropertyName("engineWarnings")] public int EngineWarnings { get; set; }
        [JsonPropertyName("onPitRoad")] public bool OnPitRoad { get; set; }

        // Extras úteis para overlays de inputs
        [JsonPropertyName("yawRate")] public float YawRate { get; set; }
        [JsonPropertyName("pitchRate")] public float PitchRate { get; set; }
        [JsonPropertyName("rollRate")] public float RollRate { get; set; }
    }
}
