using System;
using System.Linq;
using IRSDKSharper;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
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
        }

        private void PopulateAdvancedVehicleData(IRacingSdkData d, TelemetryModel t)
        {
            t.Vehicle.ThrottleRaw = GetSdkValue<float>(d, "ThrottleRaw") ?? 0f;
            t.Vehicle.BrakeRaw = GetSdkValue<float>(d, "BrakeRaw") ?? 0f;
            t.Vehicle.HandBrake = GetSdkValue<float>(d, "HandBrake") ?? 0f;
            t.Vehicle.HandBrakeRaw = GetSdkValue<float>(d, "HandBrakeRaw") ?? 0f;
            t.Vehicle.SteeringWheelPctTorque = GetSdkValue<float>(d, "SteeringWheelPctTorque") ?? 0f;
            t.Vehicle.SteeringWheelLimiter = GetSdkValue<int>(d, "SteeringWheelLimiter") ?? 0;
            t.Vehicle.SteeringWheelPeakForceNm = GetSdkValue<float>(d, "SteeringWheelPeakForceNm") ?? 0f;
            t.Vehicle.Voltage = GetSdkValue<float>(d, "Voltage") ?? 0f;
            t.Vehicle.OilLevel = GetSdkValue<float>(d, "OilLevel") ?? 0f;
            t.Vehicle.WaterLevel = GetSdkValue<float>(d, "WaterLevel") ?? 0f;
        }

        private void PopulateForceDetailData(IRacingSdkData d, TelemetryModel t)
        {
            t.Vehicle.SteeringWheelPctTorqueSign = GetSdkValue<float>(d, "SteeringWheelPctTorqueSign") ?? 0f;
            t.Vehicle.SteeringWheelPctTorqueSignStops = GetSdkValue<float>(d, "SteeringWheelPctTorqueSignStops") ?? 0f;
        }

        private void PopulatePowertrainData(IRacingSdkData d, TelemetryModel t)
        {
            t.Powertrain.ShiftIndicatorPct = GetSdkValue<float>(d, "ShiftIndicatorPct") ?? 0f;
            t.Powertrain.ShiftPowerPct = GetSdkValue<float>(d, "ShiftPowerPct") ?? 0f;
            t.Powertrain.ShiftGrindRpm = GetSdkValue<float>(d, "ShiftGrindRPM") ?? 0f;
            t.Powertrain.ManualBoost = GetSdkValue<bool>(d, "ManualBoost") ?? false;
            t.Powertrain.ManualNoBoost = GetSdkValue<bool>(d, "ManualNoBoost") ?? false;
            t.Powertrain.PushToPass = GetSdkValue<bool>(d, "PushToPass") ?? false;
            t.Powertrain.P2PCount = GetSdkValue<int>(d, "P2P_Count") ?? 0;
            t.Powertrain.P2PStatus = GetSdkValue<int>(d, "P2P_Status") ?? 0;
            t.Powertrain.EnergyErsBattery = GetSdkValue<float>(d, "EnergyERSBattery") ?? 0f;
            t.Powertrain.EnergyErsBatteryPct = GetSdkValue<float>(d, "EnergyERSBatteryPct") ?? 0f;
            t.Powertrain.EnergyBatteryToMguKLap = GetSdkValue<float>(d, "EnergyBatteryToMGU_KLap") ?? 0f;
            t.Powertrain.EnergyMguKLapDeployPct = GetSdkValue<float>(d, "EnergyMGU_KLapDeployPct") ?? 0f;
        }

        private void PopulatePitStrategyData(IRacingSdkData d, TelemetryModel t)
        {
            t.Pit.PitSvFuel = GetSdkValue<float>(d, "PitSvFuel") ?? 0f;
            t.Pit.PitSvFlags = GetSdkValue<int>(d, "PitSvFlags") ?? 0;
            t.Pit.PitSvTireCompound = GetSdkValue<int>(d, "PitSvTireCompound") ?? 0;
            t.Pit.PitSvLFP = GetSdkValue<float>(d, "PitSvLFP") ?? 0f;
            t.Pit.PitSvLRP = GetSdkValue<float>(d, "PitSvLRP") ?? 0f;
            t.Pit.PitSvRFP = GetSdkValue<float>(d, "PitSvRFP") ?? 0f;
            t.Pit.PitSvRRP = GetSdkValue<float>(d, "PitSvRRP") ?? 0f;
            t.Pit.FastRepairAvailable = GetSdkValue<int>(d, "FastRepairAvailable") ?? 0;
            t.Pit.FastRepairUsed = GetSdkValue<int>(d, "FastRepairUsed") ?? 0;
            t.Pit.PlayerCarInPitStall = GetSdkValue<bool>(d, "PlayerCarInPitStall") ?? false;
        }

        private void PopulateSessionEnvironmentData(IRacingSdkData d, TelemetryModel t)
        {
            t.Session.SessionUniqueID = GetSdkValue<int>(d, "SessionUniqueID") ?? 0;
            t.Session.SessionTick = GetSdkValue<int>(d, "SessionTick") ?? 0;
            t.Session.SessionTimeTotal = GetSdkValue<float>(d, "SessionTimeTotal") ?? 0f;
            t.Environment.WeatherDeclaredWet = GetSdkValue<bool>(d, "WeatherDeclaredWet") ?? false;
            t.Environment.SolarAltitude = GetSdkValue<float>(d, "SolarAltitude") ?? 0f;
            t.Environment.SolarAzimuth = GetSdkValue<float>(d, "SolarAzimuth") ?? 0f;
            t.Environment.FogLevel = GetSdkValue<float>(d, "FogLevel") ?? 0f;
            t.Environment.Precipitation = GetSdkValue<float>(d, "Precipitation") ?? 0f;
            t.Environment.TrackGripStatus = t.TrackGripStatus;
        }

        private void PopulateRadarExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.Radar.CarIdxGear = GetSdkArray<int>(d, "CarIdxGear").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxRPM = GetSdkArray<float>(d, "CarIdxRPM").Select(v => v ?? 0f).ToArray();
            t.Radar.CarIdxSteer = GetSdkArray<float>(d, "CarIdxSteer").Select(v => v ?? 0f).ToArray();
            t.Radar.CarIdxTrackSurfaceMaterial = GetSdkArray<int>(d, "CarIdxTrackSurfaceMaterial").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxEstTime = GetSdkArray<float>(d, "CarIdxEstTime").Select(v => v ?? 0f).ToArray();
            t.Radar.CarIdxPaceFlags = GetSdkArray<int>(d, "CarIdxPaceFlags").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxPaceLine = GetSdkArray<int>(d, "CarIdxPaceLine").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxPaceRow = GetSdkArray<int>(d, "CarIdxPaceRow").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxFastRepairsUsed = GetSdkArray<int>(d, "CarIdxFastRepairsUsed").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxTireCompound = GetSdkArray<int>(d, "CarIdxTireCompound").Select(v => v ?? 0).ToArray();
            t.Radar.CarIdxPowerAdjust = GetSdkArray<float>(d, "CarIdxPowerAdjust").Select(v => v ?? 0f).ToArray();
            t.Radar.CarIdxWeightPenalty = GetSdkArray<float>(d, "CarIdxWeightPenalty").Select(v => v ?? 0f).ToArray();
            t.Radar.CarLeftRight = GetSdkValue<int>(d, "CarLeftRight") ?? 0;
        }

        private void PopulateSystemPerformanceData(IRacingSdkData d, TelemetryModel t)
        {
            t.System.FrameRate = GetSdkValue<float>(d, "FrameRate") ?? 0f;
            t.System.CpuUsageFg = GetSdkValue<float>(d, "CpuUsageFG") ?? 0f;
            t.System.CpuUsageBg = GetSdkValue<float>(d, "CpuUsageBG") ?? 0f;
            t.System.GpuUsage = GetSdkValue<float>(d, "GpuUsage") ?? 0f;
            t.System.ChanLatency = GetSdkValue<float>(d, "ChanLatency") ?? 0f;
            t.System.ChanQuality = GetSdkValue<float>(d, "ChanQuality") ?? 0f;
            t.System.ChanPartnerQuality = GetSdkValue<float>(d, "ChanPartnerQuality") ?? 0f;
            t.System.ChanAvgLatency = GetSdkValue<float>(d, "ChanAvgLatency") ?? 0f;
            t.System.ChanClockSkew = GetSdkValue<float>(d, "ChanClockSkew") ?? 0f;
        }

        private void PopulateHighFreqData(IRacingSdkData d, TelemetryModel t)
        {
            t.HighFreq.LatAccel_ST = GetSdkValue<float>(d, "LatAccel_ST") ?? 0f;
            t.HighFreq.LongAccel_ST = GetSdkValue<float>(d, "LongAccel_ST") ?? 0f;
        }

        private void PopulateDamageExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.Damage.PlayerCarWeightPenalty = GetSdkValue<float>(d, "PlayerCarWeightPenalty") ?? 0f;
            t.Damage.PlayerCarPowerAdjust = GetSdkValue<float>(d, "PlayerCarPowerAdjust") ?? 0f;
            t.Damage.PlayerCarTowTime = GetSdkValue<float>(d, "PlayerCarTowTime") ?? 0f;
        }
    }
}
