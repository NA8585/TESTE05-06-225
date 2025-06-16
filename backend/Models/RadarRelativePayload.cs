using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class RadarCarPayload
    {
        [JsonPropertyName("carIdx")] public int CarIdx { get; set; }
        [JsonPropertyName("posX")] public float PosX { get; set; }
        [JsonPropertyName("posY")] public float PosY { get; set; }
        [JsonPropertyName("gap")] public float Gap { get; set; }
        [JsonPropertyName("deltaLap")] public int DeltaLap { get; set; }
        [JsonPropertyName("onPitRoad")] public bool OnPitRoad { get; set; }
        [JsonPropertyName("trackSurface")] public int TrackSurface { get; set; }
        [JsonPropertyName("carClassId")] public int CarClassId { get; set; }
        [JsonPropertyName("carClassShortName")] public string CarClassShortName { get; set; } = string.Empty;
        [JsonPropertyName("userName")] public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("carNumber")] public string CarNumber { get; set; } = string.Empty;
        [JsonPropertyName("license")] public string License { get; set; } = string.Empty;
        [JsonPropertyName("iRating")] public int IRating { get; set; }
        [JsonPropertyName("tireCompound")] public string TireCompound { get; set; } = string.Empty;
        [JsonPropertyName("isFastestLap")] public bool IsFastestLap { get; set; }
        [JsonPropertyName("isSameClass")] public bool IsSameClass { get; set; }
        [JsonPropertyName("isPlayer")] public bool IsPlayer { get; set; }
    }

    public class RadarRelativePayload
    {
        [JsonPropertyName("playerCarIdx")] public int PlayerCarIdx { get; set; }
        [JsonPropertyName("trackLength")] public float TrackLength { get; set; }
        [JsonPropertyName("radarCars")] public RadarCarPayload[] RadarCars { get; set; } = System.Array.Empty<RadarCarPayload>();
    }
}
