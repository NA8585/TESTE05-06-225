using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class WeatherPayload
    {
        [JsonPropertyName("airTemp")] public float AirTemp { get; set; }
        [JsonPropertyName("trackTemp")] public float TrackTemp { get; set; }
        [JsonPropertyName("trackTempCrew")] public float TrackTempCrew { get; set; }
        [JsonPropertyName("humidity")] public float Humidity { get; set; }
        [JsonPropertyName("pressureMb")] public float PressureMb { get; set; }
        [JsonPropertyName("precipitation")] public float Precipitation { get; set; }
        [JsonPropertyName("fogLevel")] public float FogLevel { get; set; }
        [JsonPropertyName("solarAltitude")] public float SolarAltitude { get; set; }
        [JsonPropertyName("solarAzimuth")] public float SolarAzimuth { get; set; }
        [JsonPropertyName("windVel")] public float WindVel { get; set; }
        [JsonPropertyName("windDir")] public float WindDir { get; set; }
        [JsonPropertyName("weatherDeclaredWet")] public bool WeatherDeclaredWet { get; set; }
        [JsonPropertyName("trackGripStatus")] public string TrackGripStatus { get; set; } = string.Empty;
        [JsonPropertyName("forecast")] public string Forecast { get; set; } = string.Empty;
    }
}
