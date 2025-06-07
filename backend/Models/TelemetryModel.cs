using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperBackendNR85IA.Models
{
    public class TelemetryModel
    {
        // ─────────────────────────────────────────────────────────────────────────
        // Sessão
        // ─────────────────────────────────────────────────────────────────────────
        public int SessionNum { get; set; }
        public float SessionTime { get; set; }
        public float SessionTimeRemain { get; set; }
        public int SessionState { get; set; }
        public int PaceMode { get; set; }
        public int SessionFlags { get; set; }
        public int PlayerCarIdx { get; set; }
        public int TotalLaps { get; set; }
        public int LapsRemainingRace { get; set; }
        public string SessionTypeFromYaml { get; set; } = string.Empty;

        public string SessionTimeFormatted => FormatTime(SessionTime);
        public string SessionTimeRemainingFormatted => FormatTime(SessionTimeRemain);

        // ─────────────────────────────────────────────────────────────────────────
        // Dados básicos do veículo
        // ─────────────────────────────────────────────────────────────────────────
        public float Speed { get; set; }
        public float Rpm { get; set; }
        public float Throttle { get; set; }
        public float Brake { get; set; }
        public float Clutch { get; set; }
        public float SteeringWheelAngle { get; set; }
        public int Gear { get; set; }
        public float FuelLevel { get; set; }
        public float FuelLevelPct { get; set; }
        public float WaterTemp { get; set; }
        public float OilTemp { get; set; }
        public float OilPress { get; set; }
        public float FuelPress { get; set; }
        public float ManifoldPress { get; set; }
        public int EngineWarnings { get; set; }
        public bool OnPitRoad { get; set; }
        public float PlayerCarLastPitTime { get; set; }
        public int PlayerCarPitStopCount { get; set; }
        public float PitRepairLeft { get; set; }
        public float PitOptRepairLeft { get; set; }
        public float CarSpeed { get; set; } // Igual a Speed, usado para FFB parado

        // ─────────────────────────────────────────────────────────────────────────
        // Voltas e tempos
        // ─────────────────────────────────────────────────────────────────────────
        public int Lap { get; set; }
        public float LapDistPct { get; set; }
        public float LapCurrentLapTime { get; set; }
        public float LapLastLapTime { get; set; }
        public float LapBestLapTime { get; set; }
        public float LapDeltaToSessionBestLap { get; set; }
        public float LapDeltaToSessionOptimalLap { get; set; }
        public float LapDeltaToDriverBestLap { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Setores
        // ─────────────────────────────────────────────────────────────────────────
        public float[] LapAllSectorTimes { get; set; } = Array.Empty<float>();
        public float[] LapDeltaToSessionBestSectorTimes { get; set; } = Array.Empty<float>();
        public float[] SessionBestSectorTimes { get; set; } = Array.Empty<float>();
        public float EstLapTime { get; set; }  // Ótima soma de setores ou LapOptimalLapTime
        public int SectorCount { get; set; }
        public float[] SectorDeltas { get; set; } = Array.Empty<float>();
        public bool[] SectorIsBest { get; set; } = Array.Empty<bool>();

        // ─────────────────────────────────────────────────────────────────────────
        // Force Feedback (FFB)
        // ─────────────────────────────────────────────────────────────────────────
        public float FfbPercent { get; set; }  // 0–1
        public bool FfbClip { get; set; }  // true se ≥ 98%

        // ─────────────────────────────────────────────────────────────────────────
        // Distância relativa (carro à frente / atrás)
        // ─────────────────────────────────────────────────────────────────────────
        public float[] CarIdxLapDistPct { get; set; } = Array.Empty<float>();
        public int[] CarIdxPosition { get; set; } = Array.Empty<int>();
        public int[] CarIdxLap { get; set; } = Array.Empty<int>();
        public bool[] CarIdxOnPitRoad { get; set; } = Array.Empty<bool>();
        public int[] CarIdxTrackSurface { get; set; } = Array.Empty<int>();
        public float[] CarIdxLastLapTime { get; set; } = Array.Empty<float>();
        public float[] CarIdxF2Time { get; set; } = Array.Empty<float>();
        public float DistanceAhead { get; set; }  // em metros (ou -1 se não houver)
        public float DistanceBehind { get; set; }  // em metros (ou -1 se não houver)
        public float TimeDeltaToCarAhead { get; set; }
        public float TimeDeltaToCarBehind { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Pneus e temperaturas
        // ─────────────────────────────────────────────────────────────────────────
        public float LfTempCl { get; set; }
        public float LfTempCm { get; set; }
        public float LfTempCr { get; set; }
        public float RfTempCl { get; set; }
        public float RfTempCm { get; set; }
        public float RfTempCr { get; set; }
        public float LrTempCl { get; set; }
        public float LrTempCm { get; set; }
        public float LrTempCr { get; set; }
        public float RrTempCl { get; set; }
        public float RrTempCm { get; set; }
        public float RrTempCr { get; set; }

        public float LfPress { get; set; }
        public float RfPress { get; set; }
        public float LrPress { get; set; }
        public float RrPress { get; set; }

        public float[] LfWear { get; set; } = Array.Empty<float>(); // [L, M, R]
        public float[] RfWear { get; set; } = Array.Empty<float>();
        public float[] LrWear { get; set; } = Array.Empty<float>();
        public float[] RrWear { get; set; } = Array.Empty<float>();

        public float TreadRemainingFl { get; set; }
        public float TreadRemainingFr { get; set; }
        public float TreadRemainingRl { get; set; }
        public float TreadRemainingRr { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Freios e controles
        // ─────────────────────────────────────────────────────────────────────────
        public float[] BrakeTemp { get; set; } = Array.Empty<float>(); // 4 rodas
        public float LfBrakeLinePress { get; set; }
        public float RfBrakeLinePress { get; set; }
        public float LrBrakeLinePress { get; set; }
        public float RrBrakeLinePress { get; set; }
        public float DcBrakeBias { get; set; }
        public int DcAbs { get; set; }
        public int DcTractionControl { get; set; }
        public int DcFrontWing { get; set; }
        public int DcRearWing { get; set; }
        public int DcDiffEntry { get; set; }
        public int DcDiffMiddle { get; set; }
        public int DcDiffExit { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Dinâmica do veículo
        // ─────────────────────────────────────────────────────────────────────────
        public float LfSuspPos { get; set; }
        public float RfSuspPos { get; set; }
        public float LrSuspPos { get; set; }
        public float RrSuspPos { get; set; }
        public float LfSuspVel { get; set; }
        public float RfSuspVel { get; set; }
        public float LrSuspVel { get; set; }
        public float RrSuspVel { get; set; }
        public float LfRideHeight { get; set; }
        public float RfRideHeight { get; set; }
        public float LrRideHeight { get; set; }
        public float RrRideHeight { get; set; }
        public float LatAccel { get; set; }
        public float LonAccel { get; set; }
        public float VertAccel { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Danos
        // ─────────────────────────────────────────────────────────────────────────
        public float LfDamage { get; set; }
        public float RfDamage { get; set; }
        public float LrDamage { get; set; }
        public float RrDamage { get; set; }
        public float FrontWingDamage { get; set; }
        public float RearWingDamage { get; set; }
        public float EngineDamage { get; set; }
        public float GearboxDamage { get; set; }
        public float SuspensionDamage { get; set; }
        public float ChassisDamage { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Sistemas especiais
        // ─────────────────────────────────────────────────────────────────────────
        public int DrsStatus { get; set; }
        public int[] CarIdxP2PCount { get; set; } = Array.Empty<int>();
        public int[] CarIdxP2PStatus { get; set; } = Array.Empty<int>();
        public int DcEnginePower { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // Condições da pista
        // ─────────────────────────────────────────────────────────────────────────
        public float TrackSurfaceTemp { get; set; }
        public float TrackTempCrew { get; set; }
        public int TempUnits { get; set; } // 0=imperial,1=metric
        public float SessionTimeOfDay { get; set; }
        public int TrackSurfaceMaterial { get; set; }
        public string TrackGripStatus { get; set; } = string.Empty;
        public string TrackStatus { get; set; } = string.Empty;

        // ─────────────────────────────────────────────────────────────────────────
        // Combustível
        // ─────────────────────────────────────────────────────────────────────────
        public float FuelUsePerHour { get; set; }
        public float FuelUsePerLap { get; set; }
        public float FuelPerLap { get; set; }
        public float FuelCapacity { get; set; }
        public float LapDistTotal { get; set; }
        public float FuelLevelLapStart { get; set; }
        public float FuelUsedTotal { get; set; }

        // Valores calculados
        public float FuelUsePerLapCalc { get; set; }
        public float EstLapTimeCalc { get; set; }
        public float ConsumoVoltaAtual { get; set; }
        public int LapsRemaining { get; set; }
        public float ConsumoMedio { get; set; }
        public float VoltasRestantesMedio { get; set; }
        public float ConsumoUltimaVolta { get; set; }
        public float VoltasRestantesUltimaVolta { get; set; }
        public float NecessarioFim { get; set; }
        public float RecomendacaoAbastecimento { get; set; }
        public float FuelRemaining { get; set; }
        public float FuelEta { get; set; }

        public FuelStatus FuelStatus { get; set; } = new FuelStatus();

        // ─────────────────────────────────────────────────────────────────────────
        // Dados YAML (pilot, weekend, session) - preenchido pelo SessionYamlParser
        // ─────────────────────────────────────────────────────────────────────────
        public string UserName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string CarNumber { get; set; } = string.Empty;
        public int IRating { get; set; }
        public string LicString { get; set; } = string.Empty;
        public float LicSafetyRating { get; set; }
        public int PlayerCarClassID { get; set; }
        public int PlayerCarTeamIncidentCount { get; set; }
        public string TrackNumTurns { get; set; }
        public string TrackDisplayName { get; set; } = string.Empty;
        public string TrackConfigName { get; set; } = string.Empty;
        public float TrackLength { get; set; }
        public string Skies { get; set; } = string.Empty;
        public string ForecastType { get; set; } = string.Empty;
        public float TrackWindVel { get; set; }
        public float AirPressure { get; set; }
        public float RelativeHumidity { get; set; }
        public float ChanceOfRain { get; set; }
        public int IncidentLimit { get; set; }
        public float TrackAirTemp { get; set; }
        public string SessionInfoYaml { get; set; } = string.Empty;
        
        // ─────────────────────────────────────────────────────────────────────────
        // Métodos auxiliares
        // ─────────────────────────────────────────────────────────────────────────
        public static string FormatTime(float seconds)
{
    if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0 || seconds > 60 * 60 * 24 * 365)
        return "--:--:--"; // Valor fora do esperado, exibe placeholder

    TimeSpan t = TimeSpan.FromSeconds(seconds);
    return $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
}

    }

    public class FuelStatus
    {
        public string Text { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
    }
}
