using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class DamagePayload
    {
        [JsonPropertyName("lfDamage")] public float LfDamage { get; set; }
        [JsonPropertyName("rfDamage")] public float RfDamage { get; set; }
        [JsonPropertyName("lrDamage")] public float LrDamage { get; set; }
        [JsonPropertyName("rrDamage")] public float RrDamage { get; set; }
        [JsonPropertyName("frontWingDamage")] public float FrontWingDamage { get; set; }
        [JsonPropertyName("rearWingDamage")] public float RearWingDamage { get; set; }
        [JsonPropertyName("engineDamage")] public float EngineDamage { get; set; }
        [JsonPropertyName("gearboxDamage")] public float GearboxDamage { get; set; }
        [JsonPropertyName("suspensionDamage")] public float SuspensionDamage { get; set; }
        [JsonPropertyName("chassisDamage")] public float ChassisDamage { get; set; }

        // Extras do seu modelo
        [JsonPropertyName("playerCarWeightPenalty")] public float PlayerCarWeightPenalty { get; set; }
        [JsonPropertyName("playerCarPowerAdjust")] public float PlayerCarPowerAdjust { get; set; }
        [JsonPropertyName("playerCarTowTime")] public float PlayerCarTowTime { get; set; }
    }
}
