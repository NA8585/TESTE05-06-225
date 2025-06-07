using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    // Classe principal enviada ao frontend
    public class FrontendDataPayload
    {
        [JsonPropertyName("telemetry")] public TelemetryPayload? Telemetry { get; set; }
        [JsonPropertyName("drivers")] public List<DriverPayload>? Drivers { get; set; }
        [JsonPropertyName("sessionInfo")] public SessionInfoPayload? SessionInfo { get; set; }
        [JsonPropertyName("weekendInfo")] public WeekendInfoPayload? WeekendInfo { get; set; }
        [JsonPropertyName("results")] public List<ResultPayload>? Results { get; set; }
    }

    // --- Sub-classes para cada parte do payload ---
    public class TelemetryPayload
    {
        [JsonPropertyName("playerCarIdx")] public int PlayerCarIdx { get; set; }
        [JsonPropertyName("sessionTime")] public float SessionTime { get; set; }
        [JsonPropertyName("sessionTimeRemain")] public float SessionTimeRemain { get; set; }
        [JsonPropertyName("lapCompleted")] public int LapCompleted { get; set; }
        [JsonPropertyName("sessionLapsRemain")] public int SessionLapsRemain { get; set; }
        [JsonPropertyName("trackTemp")] public float TrackTemp { get; set; }
        [JsonPropertyName("trackTempCrew")] public float TrackTempCrew { get; set; }
        [JsonPropertyName("dcBrakeBias")] public float DcBrakeBias { get; set; }
        [JsonPropertyName("trackWetnessPCA")] public float TrackWetnessPCA { get; set; }
        [JsonPropertyName("playerCarMyIncidentCount")] public int PlayerCarMyIncidentCount { get; set; }
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
