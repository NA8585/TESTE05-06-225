using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SuperBackendNR85IA.Calculations;

namespace SuperBackendNR85IA.Models
{
    public partial class TelemetryModel
    {
        public SessionData Session { get; set; } = new SessionData();
        public VehicleData Vehicle { get; set; } = new VehicleData();
        public TyreData Tyres { get; set; } = new TyreData();
        public DamageData Damage { get; set; } = new DamageData();

        // Wrapper properties to keep legacy flat structure
        // ---- Session ----
        public int SessionNum { get => Session.SessionNum; set => Session.SessionNum = value; }
        public double SessionTime { get => Session.SessionTime; set => Session.SessionTime = value; }
        public double SessionTimeRemain { get => Session.SessionTimeRemain; set => Session.SessionTimeRemain = value; }
        public bool SessionTimeRemainValid { get => Session.SessionTimeRemainValid; set => Session.SessionTimeRemainValid = value; }
        public int SessionState { get => Session.SessionState; set => Session.SessionState = value; }
        public int PaceMode { get => Session.PaceMode; set => Session.PaceMode = value; }
        public int SessionFlags { get => Session.SessionFlags; set => Session.SessionFlags = value; }
        public int PlayerCarIdx { get => Session.PlayerCarIdx; set => Session.PlayerCarIdx = value; }
        public int TotalLaps { get => Session.TotalLaps; set => Session.TotalLaps = value; }
        public int LapsRemainingRace { get => Session.LapsRemainingRace; set => Session.LapsRemainingRace = value; }
        public float SessionTimeTotal { get => Session.SessionTimeTotal; set => Session.SessionTimeTotal = value; }
        public int SessionLapsTotal { get => Session.SessionLapsTotal; set => Session.SessionLapsTotal = value; }
        public int SessionLapsRemain { get => Session.SessionLapsRemain; set => Session.SessionLapsRemain = value; }
        public int RaceLaps { get => Session.RaceLaps; set => Session.RaceLaps = value; }
        public bool PitsOpen { get => Session.PitsOpen; set => Session.PitsOpen = value; }
        public long SessionUniqueID { get => Session.SessionUniqueID; set => Session.SessionUniqueID = value; }
        public int SessionTick { get => Session.SessionTick; set => Session.SessionTick = value; }
        public bool SessionOnJokerLap { get => Session.SessionOnJokerLap; set => Session.SessionOnJokerLap = value; }
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
        public float ThrottleRaw { get => Vehicle.ThrottleRaw; set => Vehicle.ThrottleRaw = value; }
        public float BrakeRaw { get => Vehicle.BrakeRaw; set => Vehicle.BrakeRaw = value; }
        public bool BrakeABSactive { get => Vehicle.BrakeABSactive; set => Vehicle.BrakeABSactive = value; }
        public float BrakeABSCutPct { get => Vehicle.BrakeABSCutPct; set => Vehicle.BrakeABSCutPct = value; }
        public float HandBrake { get => Vehicle.HandBrake; set => Vehicle.HandBrake = value; }
        public float HandBrakeRaw { get => Vehicle.HandBrakeRaw; set => Vehicle.HandBrakeRaw = value; }
        public float SteeringWheelAngleMax { get => Vehicle.SteeringWheelAngleMax; set => Vehicle.SteeringWheelAngleMax = value; }
        public int SteeringWheelLimiter { get => Vehicle.SteeringWheelLimiter; set => Vehicle.SteeringWheelLimiter = value; }
        public float SteeringWheelTorque { get => Vehicle.SteeringWheelTorque; set => Vehicle.SteeringWheelTorque = value; }
        public float SteeringWheelPeakForceNm { get => Vehicle.SteeringWheelPeakForceNm; set => Vehicle.SteeringWheelPeakForceNm = value; }
        public float YawRate { get => Vehicle.YawRate; set => Vehicle.YawRate = value; }
        public float PitchRate { get => Vehicle.PitchRate; set => Vehicle.PitchRate = value; }
        public float RollRate { get => Vehicle.RollRate; set => Vehicle.RollRate = value; }
        public float SteeringWheelPctDamper { get => Vehicle.SteeringWheelPctDamper; set => Vehicle.SteeringWheelPctDamper = value; }
        public float SteeringWheelPctTorque { get => Vehicle.SteeringWheelPctTorque; set => Vehicle.SteeringWheelPctTorque = value; }
        public float SteeringWheelPctTorqueSign { get => Vehicle.SteeringWheelPctTorqueSign; set => Vehicle.SteeringWheelPctTorqueSign = value; }
        public float SteeringWheelPctTorqueSignStops { get => Vehicle.SteeringWheelPctTorqueSignStops; set => Vehicle.SteeringWheelPctTorqueSignStops = value; }

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
        public float LfColdPress { get => Tyres.LfColdPress; set => Tyres.LfColdPress = value; }
        public float RfColdPress { get => Tyres.RfColdPress; set => Tyres.RfColdPress = value; }
        public float LrColdPress { get => Tyres.LrColdPress; set => Tyres.LrColdPress = value; }
        public float RrColdPress { get => Tyres.RrColdPress; set => Tyres.RrColdPress = value; }

        public float LfSetupPressure { get => Tyres.LfSetupPressure; set => Tyres.LfSetupPressure = value; }
        public float RfSetupPressure { get => Tyres.RfSetupPressure; set => Tyres.RfSetupPressure = value; }
        public float LrSetupPressure { get => Tyres.LrSetupPressure; set => Tyres.LrSetupPressure = value; }
        public float RrSetupPressure { get => Tyres.RrSetupPressure; set => Tyres.RrSetupPressure = value; }

        public float LfHotPressure { get => Tyres.LfHotPressure; set => Tyres.LfHotPressure = value; }
        public float RfHotPressure { get => Tyres.RfHotPressure; set => Tyres.RfHotPressure = value; }
        public float LrHotPressure { get => Tyres.LrHotPressure; set => Tyres.LrHotPressure = value; }
        public float RrHotPressure { get => Tyres.RrHotPressure; set => Tyres.RrHotPressure = value; }
        public float[] LfWear { get => Tyres.LfWear; set => Tyres.LfWear = value; }
        public float[] RfWear { get => Tyres.RfWear; set => Tyres.RfWear = value; }
        public float[] LrWear { get => Tyres.LrWear; set => Tyres.LrWear = value; }
        public float[] RrWear { get => Tyres.RrWear; set => Tyres.RrWear = value; }
        public float LfWearAvg { get => Tyres.LfWearAvg; set => Tyres.LfWearAvg = value; }
        public float RfWearAvg { get => Tyres.RfWearAvg; set => Tyres.RfWearAvg = value; }
        public float LrWearAvg { get => Tyres.LrWearAvg; set => Tyres.LrWearAvg = value; }
        public float RrWearAvg { get => Tyres.RrWearAvg; set => Tyres.RrWearAvg = value; }
        public float[] LfTreadRemainingParts { get => Tyres.LfTreadRemainingParts; set => Tyres.LfTreadRemainingParts = value; }
        public float[] RfTreadRemainingParts { get => Tyres.RfTreadRemainingParts; set => Tyres.RfTreadRemainingParts = value; }
        public float[] LrTreadRemainingParts { get => Tyres.LrTreadRemainingParts; set => Tyres.LrTreadRemainingParts = value; }
        public float[] RrTreadRemainingParts { get => Tyres.RrTreadRemainingParts; set => Tyres.RrTreadRemainingParts = value; }
        public float LfLastHotPress { get => Tyres.LfLastHotPress; set => Tyres.LfLastHotPress = value; }
        public float RfLastHotPress { get => Tyres.RfLastHotPress; set => Tyres.RfLastHotPress = value; }
        public float LrLastHotPress { get => Tyres.LrLastHotPress; set => Tyres.LrLastHotPress = value; }
        public float RrLastHotPress { get => Tyres.RrLastHotPress; set => Tyres.RrLastHotPress = value; }
        public float LfColdTempCl { get => Tyres.LfColdTempCl; set => Tyres.LfColdTempCl = value; }
        public float LfColdTempCm { get => Tyres.LfColdTempCm; set => Tyres.LfColdTempCm = value; }
        public float LfColdTempCr { get => Tyres.LfColdTempCr; set => Tyres.LfColdTempCr = value; }
        public float RfColdTempCl { get => Tyres.RfColdTempCl; set => Tyres.RfColdTempCl = value; }
        public float RfColdTempCm { get => Tyres.RfColdTempCm; set => Tyres.RfColdTempCm = value; }
        public float RfColdTempCr { get => Tyres.RfColdTempCr; set => Tyres.RfColdTempCr = value; }
        public float LrColdTempCl { get => Tyres.LrColdTempCl; set => Tyres.LrColdTempCl = value; }
        public float LrColdTempCm { get => Tyres.LrColdTempCm; set => Tyres.LrColdTempCm = value; }
        public float LrColdTempCr { get => Tyres.LrColdTempCr; set => Tyres.LrColdTempCr = value; }
        public float RrColdTempCl { get => Tyres.RrColdTempCl; set => Tyres.RrColdTempCl = value; }
        public float RrColdTempCm { get => Tyres.RrColdTempCm; set => Tyres.RrColdTempCm = value; }
        public float RrColdTempCr { get => Tyres.RrColdTempCr; set => Tyres.RrColdTempCr = value; }
        public float LfLastTempCl { get => Tyres.LfLastTempCl; set => Tyres.LfLastTempCl = value; }
        public float LfLastTempCm { get => Tyres.LfLastTempCm; set => Tyres.LfLastTempCm = value; }
        public float LfLastTempCr { get => Tyres.LfLastTempCr; set => Tyres.LfLastTempCr = value; }
        public float RfLastTempCl { get => Tyres.RfLastTempCl; set => Tyres.RfLastTempCl = value; }
        public float RfLastTempCm { get => Tyres.RfLastTempCm; set => Tyres.RfLastTempCm = value; }
        public float RfLastTempCr { get => Tyres.RfLastTempCr; set => Tyres.RfLastTempCr = value; }
        public float LrLastTempCl { get => Tyres.LrLastTempCl; set => Tyres.LrLastTempCl = value; }
        public float LrLastTempCm { get => Tyres.LrLastTempCm; set => Tyres.LrLastTempCm = value; }
        public float LrLastTempCr { get => Tyres.LrLastTempCr; set => Tyres.LrLastTempCr = value; }
        public float RrLastTempCl { get => Tyres.RrLastTempCl; set => Tyres.RrLastTempCl = value; }
        public float RrLastTempCm { get => Tyres.RrLastTempCm; set => Tyres.RrLastTempCm = value; }
        public float RrLastTempCr { get => Tyres.RrLastTempCr; set => Tyres.RrLastTempCr = value; }
        public float FrontStagger { get => Tyres.FrontStagger; set => Tyres.FrontStagger = value; }
        public float RearStagger  { get => Tyres.RearStagger;  set => Tyres.RearStagger = value; }
        public float TreadRemainingFl { get => Tyres.TreadRemainingFl; set => Tyres.TreadRemainingFl = value; }
        public float TreadRemainingFr { get => Tyres.TreadRemainingFr; set => Tyres.TreadRemainingFr = value; }
        public float TreadRemainingRl { get => Tyres.TreadRemainingRl; set => Tyres.TreadRemainingRl = value; }
        public float TreadRemainingRr { get => Tyres.TreadRemainingRr; set => Tyres.TreadRemainingRr = value; }
        public float StartTreadFl { get => Tyres.StartTreadFl; set => Tyres.StartTreadFl = value; }
        public float StartTreadFr { get => Tyres.StartTreadFr; set => Tyres.StartTreadFr = value; }
        public float StartTreadRl { get => Tyres.StartTreadRl; set => Tyres.StartTreadRl = value; }
        public float StartTreadRr { get => Tyres.StartTreadRr; set => Tyres.StartTreadRr = value; }
        // Backwards compatibility with older property names
        public float LfStartTread
        {
            get => StartTreadFl;
            set => StartTreadFl = value;
        }
        public float RfStartTread
        {
            get => StartTreadFr;
            set => StartTreadFr = value;
        }
        public float LrStartTread
        {
            get => StartTreadRl;
            set => StartTreadRl = value;
        }
        public float RrStartTread
        {
            get => StartTreadRr;
            set => StartTreadRr = value;
        }
        public float TreadWearDiffFl { get => Tyres.TreadWearDiffFl; set => Tyres.TreadWearDiffFl = value; }
        public float TreadWearDiffFr { get => Tyres.TreadWearDiffFr; set => Tyres.TreadWearDiffFr = value; }
        public float TreadWearDiffRl { get => Tyres.TreadWearDiffRl; set => Tyres.TreadWearDiffRl = value; }
        public float TreadWearDiffRr { get => Tyres.TreadWearDiffRr; set => Tyres.TreadWearDiffRr = value; }
        public float? TreadLF { get => Tyres.TreadLF; set => Tyres.TreadLF = value; }
        public float? TreadRF { get => Tyres.TreadRF; set => Tyres.TreadRF = value; }
        public float? TreadLR { get => Tyres.TreadLR; set => Tyres.TreadLR = value; }
        public float? TreadRR { get => Tyres.TreadRR; set => Tyres.TreadRR = value; }

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
        public bool AreSectorsValid { get; set; }
        public string SectorTimesDebug { get; set; } = string.Empty;
        public float FfbPercent { get; set; }
        public bool FfbClip { get; set; }
        public float[] CarIdxLapDistPct { get; set; } = Array.Empty<float>();
        public int[] CarIdxPosition { get; set; } = Array.Empty<int>();
        public int[] CarIdxLap { get; set; } = Array.Empty<int>();
        public bool[] CarIdxOnPitRoad { get; set; } = Array.Empty<bool>();
        public int[] CarIdxTrackSurface { get; set; } = Array.Empty<int>();
        public float[] CarIdxLastLapTime { get; set; } = Array.Empty<float>();
        public float[] CarIdxBestLapTime { get; set; } = Array.Empty<float>();
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
        public string TireCompound { get; set; } = string.Empty;
        // Alias enviado ao frontend para compatibilidade
        public string PlayerCarTireCompound
        {
            get => TireCompound;
            set => TireCompound = value;
        }
        public string Compound { get => Tyres.Compound; set => Tyres.Compound = value; }
        public int[] CarIdxGear { get => Radar.CarIdxGear; set => Radar.CarIdxGear = value; }
        public float[] CarIdxRPM { get => Radar.CarIdxRPM; set => Radar.CarIdxRPM = value; }
        public int[] CarIdxPaceFlags { get => Radar.CarIdxPaceFlags; set => Radar.CarIdxPaceFlags = value; }
        public int[] CarIdxPaceLine { get => Radar.CarIdxPaceLine; set => Radar.CarIdxPaceLine = value; }
        public int[] CarIdxPaceRow { get => Radar.CarIdxPaceRow; set => Radar.CarIdxPaceRow = value; }
        public int[] CarIdxTrackSurfaceMaterial { get => Radar.CarIdxTrackSurfaceMaterial; set => Radar.CarIdxTrackSurfaceMaterial = value; }
        public bool IsMultiClassSession { get; set; }
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
        public float LfRideHeight { get => Tyres.LfRideHeight; set => Tyres.LfRideHeight = value; }
        public float RfRideHeight { get => Tyres.RfRideHeight; set => Tyres.RfRideHeight = value; }
        public float LrRideHeight { get => Tyres.LrRideHeight; set => Tyres.LrRideHeight = value; }
        public float RrRideHeight { get => Tyres.RrRideHeight; set => Tyres.RrRideHeight = value; }
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
        public float TrackWetnessPCA { get; set; }
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
        public int PlayerCarMyIncidentCount { get; set; }
        public string TrackNumTurns { get; set; } = string.Empty;
        public string TrackDisplayName { get; set; } = string.Empty;
        public string TrackConfigName { get; set; } = string.Empty;
        public float TrackLength { get; set; }
        public string Skies { get; set; } = string.Empty;
        public string ForecastType { get; set; } = string.Empty;
        public float TrackWindVel { get; set; }
        public float WindSpeed { get; set; }
        public float WindDir { get; set; }
        public float AirTemp { get; set; }
        public float TrackAltitude { get; set; }
        public float TrackLatitude { get; set; }
        public float TrackLongitude { get; set; }
        public float AirPressure { get; set; }
        public float RelativeHumidity { get; set; }
        public float AirDensity { get; set; }
        public float FogLevel { get; set; }
        public float Precipitation { get; set; }
        public bool WeatherDeclaredWet { get; set; }
        public float SolarAltitude { get; set; }
        public float SolarAzimuth { get; set; }
        public int CarLeftRight { get; set; }
        public float ChanceOfRain { get; set; }
        public int IncidentLimit { get; set; }
        public float TrackAirTemp { get; set; }
        public int[] CarIdxIRatingDeltas { get; set; } = Array.Empty<int>();
        // Parsed YAML objects
        public WeekendInfo? YamlWeekendInfo { get; set; }
        public SessionInfo? YamlSessionInfo { get; set; }
        public SectorInfo? YamlSectorInfo { get; set; }
        public DriverInfo? YamlPlayerDriver { get; set; }
        public List<DriverInfo> YamlDrivers { get; set; } = new();
        public string SessionInfoYaml { get; set; } = string.Empty;
        public List<ResultPosition> Results { get; set; } = new();

        // Raw values captured directly from the iRacing SDK. Keys follow the
        // original variable names as provided by the SDK and values can be
        // scalars or arrays depending on the variable type.
        [JsonIgnore]
        public Dictionary<string, object?> SdkRaw { get; set; } = new();

        public TyreStatus LfTempStatus { get; set; } = new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold);
        public TyreStatus RfTempStatus { get; set; } = new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold);
        public TyreStatus LrTempStatus { get; set; } = new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold);
        public TyreStatus RrTempStatus { get; set; } = new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold);

        public TyreStatusSet TyreStatus { get; set; } = new(
            new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold),
            new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold),
            new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold),
            new(TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold, TyreHelpers.TempStatus.Cold));

        public void Sanitize() => TelemetryCalculations.SanitizeModel(this);

        public static string FormatTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0 || seconds > 60 * 60 * 24 * 365)
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

    public record TyreStatus(
        TyreHelpers.TempStatus In,
        TyreHelpers.TempStatus Mid,
        TyreHelpers.TempStatus Out
    );

    public record TyreStatusSet(
        TyreStatus Lf,
        TyreStatus Rf,
        TyreStatus Lr,
        TyreStatus Rr
    );
}
