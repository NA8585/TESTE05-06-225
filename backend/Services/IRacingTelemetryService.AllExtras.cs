using System;
using System.Linq;
using IRSDKSharper;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private float Pos(IRacingSdkData d, string name) =>
            Utilities.DataValidator.EnsurePositive(GetSdkValue<float>(d, name) ?? 0f);

        private int NonNeg(IRacingSdkData d, string name) =>
            Utilities.DataValidator.EnsureNonNegative(GetSdkValue<int>(d, name) ?? 0);

        private long NonNegLong(IRacingSdkData d, string name) =>
            Utilities.DataValidator.EnsureNonNegative(GetSdkValue<long>(d, name) ?? 0);

        private float[] PosArray(IRacingSdkData d, string name) =>
            GetSdkArray<float>(d, name)
                .Select(v => Utilities.DataValidator.EnsurePositive(v ?? 0f))
                .ToArray();

        private int[] NonNegArray(IRacingSdkData d, string name) =>
            GetSdkArray<int>(d, name)
                .Select(v => Utilities.DataValidator.EnsureNonNegative(v ?? 0))
                .ToArray();

        private void PopulateAllExtraData(IRacingSdkData d, TelemetryModel t)
        {
            PopulateAdvancedVehicleData(d, t);
            PopulateForceDetailData(d, t);
            PopulatePowertrainData(d, t);
            PopulatePitStrategyData(d, t);
            PopulateSessionEnvironmentData(d, t);
            PopulateRadarExtraData(d, t);
            PopulateSystemPerformanceData(d, t);
            PopulateHighFreqData(d, t);
            PopulateDamageExtraData(d, t);
            PopulateReplayData(d, t);
            PopulateDcuData(d, t);
        }

        private void PopulateAdvancedVehicleData(IRacingSdkData d, TelemetryModel t)
        {
            t.Vehicle.ThrottleRaw = Pos(d, "ThrottleRaw");
            t.Vehicle.BrakeRaw = Pos(d, "BrakeRaw");
            t.Vehicle.HandBrake = Pos(d, "HandBrake");
            t.Vehicle.HandBrakeRaw = Pos(d, "HandBrakeRaw");
            t.Vehicle.SteeringWheelPctTorque = Pos(d, "SteeringWheelPctTorque");
            t.Vehicle.SteeringWheelLimiter = NonNeg(d, "SteeringWheelLimiter");
            t.Vehicle.SteeringWheelPeakForceNm = Pos(d, "SteeringWheelPeakForceNm");
            t.Vehicle.Voltage = Pos(d, "Voltage");
            t.Vehicle.OilLevel = Pos(d, "OilLevel");
            t.Vehicle.WaterLevel = Pos(d, "WaterLevel");
        }

        private void PopulateForceDetailData(IRacingSdkData d, TelemetryModel t)
        {
            t.Vehicle.SteeringWheelPctTorqueSign = Pos(d, "SteeringWheelPctTorqueSign");
            t.Vehicle.SteeringWheelPctTorqueSignStops = Pos(d, "SteeringWheelPctTorqueSignStops");
        }

        private void PopulatePowertrainData(IRacingSdkData d, TelemetryModel t)
        {
            t.Powertrain.ShiftIndicatorPct = Pos(d, "ShiftIndicatorPct");
            t.Powertrain.ShiftPowerPct = Pos(d, "ShiftPowerPct");
            t.Powertrain.ShiftGrindRpm = Pos(d, "ShiftGrindRPM");
            t.Powertrain.ManualBoost = GetSdkValue<bool>(d, "ManualBoost") ?? false;
            t.Powertrain.ManualNoBoost = GetSdkValue<bool>(d, "ManualNoBoost") ?? false;
            t.Powertrain.PushToPass = GetSdkValue<bool>(d, "PushToPass") ?? false;
            t.Powertrain.P2PCount = NonNeg(d, "P2P_Count");
            t.Powertrain.P2PStatus = NonNeg(d, "P2P_Status");
            t.Powertrain.EnergyErsBattery = Pos(d, "EnergyERSBattery");
            t.Powertrain.EnergyErsBatteryPct = Pos(d, "EnergyERSBatteryPct");
            t.Powertrain.EnergyBatteryToMguKLap = Pos(d, "EnergyBatteryToMGU_KLap");
            t.Powertrain.EnergyMguKLapDeployPct = Pos(d, "EnergyMGU_KLapDeployPct");
        }

        private void PopulatePitStrategyData(IRacingSdkData d, TelemetryModel t)
        {
            t.Pit.PitSvFuel = Pos(d, "PitSvFuel");
            t.Pit.PitSvFlags = NonNeg(d, "PitSvFlags");
            t.Pit.PitSvTireCompound = NonNeg(d, "PitSvTireCompound");
            t.Pit.PitSvLFP = Pos(d, "PitSvLFP");
            t.Pit.PitSvLRP = Pos(d, "PitSvLRP");
            t.Pit.PitSvRFP = Pos(d, "PitSvRFP");
            t.Pit.PitSvRRP = Pos(d, "PitSvRRP");
            t.Pit.FastRepairAvailable = NonNeg(d, "FastRepairAvailable");
            t.Pit.FastRepairUsed = NonNeg(d, "FastRepairUsed");
            t.Pit.PlayerCarInPitStall = GetSdkValue<bool>(d, "PlayerCarInPitStall") ?? false;
        }

        private void PopulateSessionEnvironmentData(IRacingSdkData d, TelemetryModel t)
        {
            t.Session.SessionUniqueID = NonNegLong(d, "SessionUniqueID");
            t.Session.SessionTick = NonNeg(d, "SessionTick");
            t.Session.SessionTimeTotal = Pos(d, "SessionTimeTotal");
            t.Session.DisplayUnits = (GetSdkValue<bool>(d, "DisplayUnits") ?? false) ? 1 : 0;
            t.Session.DriverMarker = GetSdkValue<bool>(d, "DriverMarker") ?? false;
            t.Environment.WeatherDeclaredWet = GetSdkValue<bool>(d, "WeatherDeclaredWet") ?? false;
            t.Environment.SolarAltitude = Pos(d, "SolarAltitude");
            t.Environment.SolarAzimuth = Pos(d, "SolarAzimuth");
            t.Environment.FogLevel = Pos(d, "FogLevel");
            t.Environment.Precipitation = Pos(d, "Precipitation");
            t.Environment.TrackGripStatus = t.TrackGripStatus;
        }

        private void PopulateRadarExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.Radar.CarIdxGear = NonNegArray(d, "CarIdxGear");
            t.Radar.CarIdxRPM = PosArray(d, "CarIdxRPM");
            t.Radar.CarIdxSteer = PosArray(d, "CarIdxSteer");
            t.Radar.CarIdxTrackSurfaceMaterial = NonNegArray(d, "CarIdxTrackSurfaceMaterial");
            t.Radar.CarIdxEstTime = PosArray(d, "CarIdxEstTime");
            t.Radar.CarIdxPaceFlags = NonNegArray(d, "CarIdxPaceFlags");
            t.Radar.CarIdxPaceLine = NonNegArray(d, "CarIdxPaceLine");
            t.Radar.CarIdxPaceRow = NonNegArray(d, "CarIdxPaceRow");
            t.Radar.CarIdxFastRepairsUsed = NonNegArray(d, "CarIdxFastRepairsUsed");
            t.Radar.CarIdxTireCompound = NonNegArray(d, "CarIdxTireCompound");
            t.Radar.CarIdxPowerAdjust = PosArray(d, "CarIdxPowerAdjust");
            t.Radar.CarIdxWeightPenalty = PosArray(d, "CarIdxWeightPenalty");
            t.Radar.CarLeftRight = NonNeg(d, "CarLeftRight");
        }

        private void PopulateSystemPerformanceData(IRacingSdkData d, TelemetryModel t)
        {
            t.System.FrameRate = Pos(d, "FrameRate");
            t.System.CpuUsageFg = Pos(d, "CpuUsageFG");
            t.System.CpuUsageBg = Pos(d, "CpuUsageBG");
            t.System.GpuUsage = Pos(d, "GpuUsage");
            t.System.ChanLatency = Pos(d, "ChanLatency");
            t.System.ChanQuality = Pos(d, "ChanQuality");
            t.System.ChanPartnerQuality = Pos(d, "ChanPartnerQuality");
            t.System.ChanAvgLatency = Pos(d, "ChanAvgLatency");
            t.System.ChanClockSkew = Pos(d, "ChanClockSkew");
        }

        private void PopulateHighFreqData(IRacingSdkData d, TelemetryModel t)
        {
            t.HighFreq.LatAccel_ST = Pos(d, "LatAccel_ST");
            t.HighFreq.LongAccel_ST = Pos(d, "LongAccel_ST");
        }

        private void PopulateDamageExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.Damage.PlayerCarWeightPenalty = Pos(d, "PlayerCarWeightPenalty");
            t.Damage.PlayerCarPowerAdjust = Pos(d, "PlayerCarPowerAdjust");
            t.Damage.PlayerCarTowTime = Pos(d, "PlayerCarTowTime");
        }

        private void PopulateReplayData(IRacingSdkData d, TelemetryModel t)
        {
            t.Replay.PlaySpeed = NonNeg(d, "ReplayPlaySpeed");
            t.Replay.PlaySlowMotion = GetSdkValue<bool>(d, "ReplayPlaySlowMotion") ?? false;
            t.Replay.SessionTime = GetSdkValue<double>(d, "ReplaySessionTime") ?? 0.0;
            t.Replay.SessionNum = NonNeg(d, "ReplaySessionNum");
        }

        private void PopulateDcuData(IRacingSdkData d, TelemetryModel t)
        {
            t.Dcu.DcLapStatus = NonNeg(d, "dcLapStatus");
            t.Dcu.DcDriversSoFar = NonNeg(d, "dcDriversSoFar");
        }
    }
}
