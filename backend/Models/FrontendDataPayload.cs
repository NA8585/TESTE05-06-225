using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    // Classe principal enviada ao frontend
    public class FrontendDataPayload
    {
        [JsonPropertyName("telemetry")] public TelemetryModel? Telemetry { get; set; }
        [JsonPropertyName("drivers")] public List<DriverPayload> Drivers { get; set; } = new();
        [JsonPropertyName("sessionInfo")] public SessionInfoPayload? SessionInfo { get; set; }
        [JsonPropertyName("weekendInfo")] public WeekendInfoPayload? WeekendInfo { get; set; }
        [JsonPropertyName("results")] public List<ResultPayload> Results { get; set; } = new();
        [JsonPropertyName("proximityCars")] public List<ProximityCar> ProximityCars { get; set; } = new();

        public bool HasResults => Results.Count > 0;
    }


    public class DriverPayload
    {
        [JsonPropertyName("carIdx")] public int CarIdx { get; set; }
        [JsonPropertyName("userName")] public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("iRating")] public int IRating { get; set; }
        [JsonPropertyName("licLevel")] public int LicLevel { get; set; }
        [JsonPropertyName("licSubLevel")] public int LicSubLevel { get; set; }
        [JsonPropertyName("carClassID")] public int CarClassID { get; set; }
        [JsonPropertyName("carClassShortName")] public string CarClassShortName { get; set; } = string.Empty;
        [JsonPropertyName("carPath")] public string CarPath { get; set; } = string.Empty;
        [JsonPropertyName("teamIncidentCount")] public int TeamIncidentCount { get; set; }
    }

    public class SessionInfoPayload
    {
        [JsonPropertyName("sessionType")] public string SessionType { get; set; } = string.Empty;
        [JsonPropertyName("incidentLimit")] public int IncidentLimit { get; set; }
        [JsonPropertyName("currentSessionTotalLaps")] public int CurrentSessionTotalLaps { get; set; }
    }

    public class WeekendInfoPayload
    {
        [JsonPropertyName("trackDisplayName")] public string TrackDisplayName { get; set; } = string.Empty;
        [JsonPropertyName("trackAirTemp")] public float TrackAirTemp { get; set; }
    }

    public class ResultPayload
    {
        [JsonPropertyName("carIdx")] public int CarIdx { get; set; }
        [JsonPropertyName("position")] public int Position { get; set; }
        [JsonPropertyName("time")] public float Time { get; set; }
        [JsonPropertyName("interval")] public float Interval { get; set; }
        [JsonPropertyName("fastestTime")] public float FastestTime { get; set; }
        [JsonPropertyName("lastTime")] public float LastTime { get; set; }
        [JsonPropertyName("newIRating")] public int NewIRating { get; set; }
    }
}
