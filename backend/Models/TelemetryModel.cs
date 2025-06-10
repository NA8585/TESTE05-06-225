using System;
using System.Collections.Generic;

namespace SuperBackendNR85IA.Models
{
    public class TelemetryModel
    {
        public SessionData Session { get; set; } = new SessionData();
        public VehicleData Vehicle { get; set; } = new VehicleData();
        public TyreData Tyres { get; set; } = new TyreData();
        public DamageData Damage { get; set; } = new DamageData();

        // Wrapper properties to keep legacy flat structure
        // ---- Session ----
        public int SessionNum { get => Session.SessionNum; set => Session.SessionNum = value; }
        public float SessionTime { get => Session.SessionTime; set => Session.SessionTime = value; }
        public float SessionTimeRemain { get => Session.SessionTimeRemain; set => Session.SessionTimeRemain = value; }
        public int SessionState { get => Session.SessionState; set => Session.SessionState = value; }
        public int PaceMode { get => Session.PaceMode; set => Session.PaceMode = value; }
        public int SessionFlags { get => Session.SessionFlags; set => Session.SessionFlags = value; }
        public int PlayerCarIdx { get => Session.PlayerCarIdx; set => Session.PlayerCarIdx = value; }
        public int TotalLaps { get => Session.TotalLaps; set => Session.TotalLaps = value; }
        public int LapsRemainingRace { get => Session.LapsRemainingRace; set => Session.LapsRemainingRace = value; }
        public string SessionTypeFromYaml { get => Session.SessionTypeFromYaml; set => Session.SessionTypeFromYaml = value; }

        public string SessionTimeFormatted => FormatTime(Session.SessionTime);
        public string SessionTimeRemainingFormatted => FormatTime(Session.SessionTimeRemain);

        // ---- Vehicle ----
        public float Speed { get => Vehicle.Speed; set => Vehicle.Speed = value; }
        public float Rpm { get => Vehicle.Rpm; set => Vehicle.Rpm = value; }
        public float Throttle { get => Vehicle.Throttle; set => Vehicle.Throttle = value; }
        public float Brake { get => Vehicle.Brake; set => Vehicle.Brake = value; }
        public float Clutch { get => Vehicle.Clutch; set => Vehicle.Clutch = value; }
        public float SteeringWheelAngle { get => Vehicle.SteeringWheelAngle; set => Vehicle.SteeringWheelAngle = value; }
        public int Gear { get => Vehicle.Gear; set => Vehicle.Gear = value; }
        public float FuelLevel { get => Vehicle.FuelLevel; set => Vehicle.FuelLevel = value; }
        public float FuelLevelPct { get => Vehicle.FuelLevelPct; set => Vehicle.FuelLevelPct = value; }
        public float WaterTemp { get => Vehicle.WaterTemp; set => Vehicle.WaterTemp = value; }
        public float OilTemp { get => Vehicle.OilTemp; set => Vehicle.OilTemp = value; }
        public float OilPress { get => Vehicle.OilPress; set => Vehicle.OilPress = value; }
        public float FuelPress { get => Vehicle.FuelPress; set => Vehicle.FuelPress = value; }
        public float ManifoldPress { get => Vehicle.ManifoldPress; set => Vehicle.ManifoldPress = value; }
        public int EngineWarnings { get => Vehicle.EngineWarnings; set => Vehicle.EngineWarnings = value; }
        public bool OnPitRoad { get => Vehicle.OnPitRoad; set => Vehicle.OnPitRoad = value; }
        public float PlayerCarLastPitTime { get => Vehicle.PlayerCarLastPitTime; set => Vehicle.PlayerCarLastPitTime = value; }
        public int PlayerCarPitStopCount { get => Vehicle.PlayerCarPitStopCount; set => Vehicle.PlayerCarPitStopCount = value; }
        public float PitRepairLeft { get => Vehicle.PitRepairLeft; set => Vehicle.PitRepairLeft = value; }
        public float PitOptRepairLeft { get => Vehicle.PitOptRepairLeft; set => Vehicle.PitOptRepairLeft = value; }
        public float CarSpeed { get => Vehicle.CarSpeed; set => Vehicle.CarSpeed = value; }

        // ---- Tyres ----
        public float LfTempCl { get => Tyres.LfTempCl; set => Tyres.LfTempCl = value; }
        public float LfTempCm { get => Tyres.LfTempCm; set => Tyres.LfTempCm = value; }
        public float LfTempCr { get => Tyres.LfTempCr; set => Tyres.LfTempCr = value; }
        public float RfTempCl { get => Tyres.RfTempCl; set => Tyres.RfTempCl = value; }
        public float RfTempCm { get => Tyres.RfTempCm; set => Tyres.RfTempCm = value; }
        public float RfTempCr { get => Tyres.RfTempCr; set => Tyres.RfTempCr = value; }
        public float LrTempCl { get => Tyres.LrTempCl; set => Tyres.LrTempCl = value; }
        public float LrTempCm { get => Tyres.LrTempCm; set => Tyres.LrTempCm = value; }
        public float LrTempCr { get => Tyres.LrTempCr; set => Tyres.LrTempCr = value; }
        public float RrTempCl { get => Tyres.RrTempCl; set => Tyres.RrTempCl = value; }
        public float RrTempCm { get => Tyres.RrTempCm; set => Tyres.RrTempCm = value; }
        public float RrTempCr { get => Tyres.RrTempCr; set => Tyres.RrTempCr = value; }
        public float LfPress { get => Tyres.LfPress; set => Tyres.LfPress = value; }
        public float RfPress { get => Tyres.RfPress; set => Tyres.RfPress = value; }
        public float LrPress { get => Tyres.LrPress; set => Tyres.LrPress = value; }
        public float RrPress { get => Tyres.RrPress; set => Tyres.RrPress = value; }
        public float[] LfWear { get => Tyres.LfWear; set => Tyres.LfWear = value; }
        public float[] RfWear { get => Tyres.RfWear; set => Tyres.RfWear = value; }
        public float[] LrWear { get => Tyres.LrWear; set => Tyres.LrWear = value; }
        public float[] RrWear { get => Tyres.RrWear; set => Tyres.RrWear = value; }
        public float TreadRemainingFl { get => Tyres.TreadRemainingFl; set => Tyres.TreadRemainingFl = value; }
        public float TreadRemainingFr { get => Tyres.TreadRemainingFr; set => Tyres.TreadRemainingFr = value; }
        public float TreadRemainingRl { get => Tyres.TreadRemainingRl; set => Tyres.TreadRemainingRl = value; }
        public float TreadRemainingRr { get => Tyres.TreadRemainingRr; set => Tyres.TreadRemainingRr = value; }

        // ---- Damage ----
        public float LfDamage { get => Damage.LfDamage; set => Damage.LfDamage = value; }
        public float RfDamage { get => Damage.RfDamage; set => Damage.RfDamage = value; }
        public float LrDamage { get => Damage.LrDamage; set => Damage.LrDamage = value; }
        public float RrDamage { get => Damage.RrDamage; set => Damage.RrDamage = value; }
        public float FrontWingDamage { get => Damage.FrontWingDamage; set => Damage.FrontWingDamage = value; }
        public float RearWingDamage { get => Damage.RearWingDamage; set => Damage.RearWingDamage = value; }
        public float EngineDamage { get => Damage.EngineDamage; set => Damage.EngineDamage = value; }
        public float GearboxDamage { get => Damage.GearboxDamage; set => Damage.GearboxDamage = value; }
        public float SuspensionDamage { get => Damage.SuspensionDamage; set => Damage.SuspensionDamage = value; }
        public float ChassisDamage { get => Damage.ChassisDamage; set => Damage.ChassisDamage = value; }

        // ---- Other existing properties ----
        public int Lap { get; set; }
        public float LapDistPct { get; set; }
        public float LapCurrentLapTime { get; set; }
        public float LapLastLapTime { get; set; }
        public float LapBestLapTime { get; set; }
        public float LapDeltaToSessionBestLap { get; set; }
        public float LapDeltaToSessionOptimalLap { get; set; }
        public float LapDeltaToDriverBestLap { get; set; }
        public float LapDeltaToBestLap
        {
            get => LapDeltaToDriverBestLap;
            set => LapDeltaToDriverBestLap = value;
        }

        public float[] LapAllSectorTimes { get; set; } = Array.Empty<float>();
        public float[] LapDeltaToSessionBestSectorTimes { get; set; } = Array.Empty<float>();
        public float[] SessionBestSectorTimes { get; set; } = Array.Empty<float>();
        public float EstLapTime { get; set; }
        public int SectorCount { get; set; }
        public float[] SectorDeltas { get; set; } = Array.Empty<float>();
        public bool[] SectorIsBest { get; set; } = Array.Empty<bool>();

        public float FfbPercent { get; set; }
        public bool FfbClip { get; set; }

        public float[] CarIdxLapDistPct { get; set; } = Array.Empty<float>();
        public int[] CarIdxPosition { get; set; } = Array.Empty<int>();
        public int[] CarIdxLap { get; set; } = Array.Empty<int>();
        public bool[] CarIdxOnPitRoad { get; set; } = Array.Empty<bool>();
        public int[] CarIdxTrackSurface { get; set; } = Array.Empty<int>();
        public float[] CarIdxLastLapTime { get; set; } = Array.Empty<float>();
        public float[] CarIdxF2Time { get; set; } = Array.Empty<float>();
        public float DistanceAhead { get; set; }
        public float DistanceBehind { get; set; }
        public float TimeDeltaToCarAhead { get; set; }
        public float TimeDeltaToCarBehind { get; set; }
        public string[] CarIdxUserNames { get; set; } = Array.Empty<string>();
        public string[] CarIdxCarNumbers { get; set; } = Array.Empty<string>();
        public string[] CarIdxTeamNames { get; set; } = Array.Empty<string>();
        public int[] CarIdxIRatings { get; set; } = Array.Empty<int>();
        public string[] CarIdxLicStrings { get; set; } = Array.Empty<string>();
        public int[] CarIdxCarClassIds { get; set; } = Array.Empty<int>();
        public string[] CarIdxCarClassShortNames { get; set; } = Array.Empty<string>();
        public float[] CarIdxCarClassEstLapTimes { get; set; } = Array.Empty<float>();
        public string[] CarIdxTireCompounds { get; set; } = Array.Empty<string>();
        public string CarAheadName { get; set; } = string.Empty;
        public string CarBehindName { get; set; } = string.Empty;

        public float[] BrakeTemp { get; set; } = Array.Empty<float>();
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

        public int DrsStatus { get; set; }
        public int[] CarIdxP2PCount { get; set; } = Array.Empty<int>();
        public int[] CarIdxP2PStatus { get; set; } = Array.Empty<int>();
        public int DcEnginePower { get; set; }

        public float TrackSurfaceTemp { get; set; }
        public float TrackTempCrew { get; set; }
        public int TempUnits { get; set; }
        public float SessionTimeOfDay { get; set; }
        public int TrackSurfaceMaterial { get; set; }
        public string TrackGripStatus { get; set; } = string.Empty;
        public string TrackStatus { get; set; } = string.Empty;

        public float FuelUsePerHour { get; set; }
        public float FuelUsePerLap { get; set; }
        public float FuelPerLap { get; set; }
        public float FuelCapacity { get; set; }
        public float LapDistTotal { get; set; }
        public float FuelLevelLapStart { get; set; }
        public float FuelUsedTotal { get; set; }

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
        // Parsed YAML objects
        public WeekendInfo? YamlWeekendInfo { get; set; }
        public SessionInfo? YamlSessionInfo { get; set; }
        public SectorInfo? YamlSectorInfo { get; set; }
        public DriverInfo? YamlPlayerDriver { get; set; }
        public List<DriverInfo> YamlDrivers { get; set; } = new();
        public string SessionInfoYaml { get; set; } = string.Empty;

        public static string FormatTime(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0 || seconds > 60 * 60 * 24 * 365)
                return "--:--:--";
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
