using System;

namespace SuperBackendNR85IA.Models
{
    // Extra fields for existing records
    public partial record VehicleData
    {
        public float Voltage { get; set; }
        public float OilLevel { get; set; }
        public float WaterLevel { get; set; }
    }

    public partial record DamageData
    {
        public float PlayerCarWeightPenalty { get; set; }
        public float PlayerCarPowerAdjust { get; set; }
        public float PlayerCarTowTime { get; set; }
    }

    // New records for extended telemetry
    public record PowertrainData
    {
        public float ShiftIndicatorPct { get; set; }
        public float ShiftPowerPct { get; set; }
        public float ShiftGrindRpm { get; set; }
        public bool ManualBoost { get; set; }
        public bool ManualNoBoost { get; set; }
        public bool PushToPass { get; set; }
        public int P2PCount { get; set; }
        public int P2PStatus { get; set; }
        public float EnergyErsBattery { get; set; }
        public float EnergyErsBatteryPct { get; set; }
        public float EnergyBatteryToMguKLap { get; set; }
        public float EnergyMguKLapDeployPct { get; set; }
    }

    public record PitData
    {
        public float PitSvFuel { get; set; }
        public int PitSvFlags { get; set; }
        public int PitSvTireCompound { get; set; }
        public float PitSvLFP { get; set; }
        public float PitSvLRP { get; set; }
        public float PitSvRFP { get; set; }
        public float PitSvRRP { get; set; }
        public int FastRepairAvailable { get; set; }
        public int FastRepairUsed { get; set; }
        public bool PlayerCarInPitStall { get; set; }

        public bool NeedsService => PitSvFuel > 0 || PitSvLFP > 0 || PitSvLRP > 0 || PitSvRFP > 0 || PitSvRRP > 0;
    }

    public record EnvironmentData
    {
        public bool WeatherDeclaredWet { get; set; }
        public float SolarAltitude { get; set; }
        public float SolarAzimuth { get; set; }
        public float FogLevel { get; set; }
        public float Precipitation { get; set; }
        public string TrackGripStatus { get; set; } = string.Empty;

        public bool HasPrecipitation => Precipitation > 0f;
    }

    public record SystemPerfData
    {
        public float FrameRate { get; set; }
        public float CpuUsageFg { get; set; }
        public float CpuUsageBg { get; set; }
        public float GpuUsage { get; set; }
        public float ChanLatency { get; set; }
        public float ChanQuality { get; set; }
        public float ChanPartnerQuality { get; set; }
        public float ChanAvgLatency { get; set; }
        public float ChanClockSkew { get; set; }

        public float AvgCpuUsage => (CpuUsageFg + CpuUsageBg) / 2f;
    }

    public record RadarData
    {
        public int[] CarIdxGear { get; set; } = Array.Empty<int>();
        public float[] CarIdxRPM { get; set; } = Array.Empty<float>();
        public float[] CarIdxSteer { get; set; } = Array.Empty<float>();
        public int[] CarIdxTrackSurfaceMaterial { get; set; } = Array.Empty<int>();
        public float[] CarIdxEstTime { get; set; } = Array.Empty<float>();
        public int[] CarIdxPaceFlags { get; set; } = Array.Empty<int>();
        public int[] CarIdxPaceLine { get; set; } = Array.Empty<int>();
        public int[] CarIdxPaceRow { get; set; } = Array.Empty<int>();
        public int[] CarIdxFastRepairsUsed { get; set; } = Array.Empty<int>();
        public int[] CarIdxTireCompound { get; set; } = Array.Empty<int>();
        public float[] CarIdxPowerAdjust { get; set; } = Array.Empty<float>();
        public float[] CarIdxWeightPenalty { get; set; } = Array.Empty<float>();
        public int CarLeftRight { get; set; }

        public int CarCount => CarIdxGear.Length;
    }

    public record HighFreqData
    {
        public float LatAccel_ST { get; set; }
        public float LongAccel_ST { get; set; }

        public float TotalAccel => System.MathF.Sqrt((LatAccel_ST * LatAccel_ST) + (LongAccel_ST * LongAccel_ST));
    }

    // Partial TelemetryModel with new groups and convenience wrappers
    public partial class TelemetryModel
    {
        public PowertrainData Powertrain { get; set; } = new PowertrainData();
        public PitData Pit { get; set; } = new PitData();
        public EnvironmentData Environment { get; set; } = new EnvironmentData();
        public SystemPerfData System { get; set; } = new SystemPerfData();
        public RadarData Radar { get; set; } = new RadarData();
        public HighFreqData HighFreq { get; set; } = new HighFreqData();

        public float ShiftIndicatorPct { get => Powertrain.ShiftIndicatorPct; set => Powertrain.ShiftIndicatorPct = value; }
        public float ShiftPowerPct { get => Powertrain.ShiftPowerPct; set => Powertrain.ShiftPowerPct = value; }
        public float ShiftGrindRpm { get => Powertrain.ShiftGrindRpm; set => Powertrain.ShiftGrindRpm = value; }
        public bool ManualBoost { get => Powertrain.ManualBoost; set => Powertrain.ManualBoost = value; }
        public bool ManualNoBoost { get => Powertrain.ManualNoBoost; set => Powertrain.ManualNoBoost = value; }
        public bool PushToPass { get => Powertrain.PushToPass; set => Powertrain.PushToPass = value; }
        public int P2PCount { get => Powertrain.P2PCount; set => Powertrain.P2PCount = value; }
        public int P2PStatus { get => Powertrain.P2PStatus; set => Powertrain.P2PStatus = value; }
        public float EnergyErsBattery { get => Powertrain.EnergyErsBattery; set => Powertrain.EnergyErsBattery = value; }
        public float EnergyErsBatteryPct { get => Powertrain.EnergyErsBatteryPct; set => Powertrain.EnergyErsBatteryPct = value; }
        public float EnergyBatteryToMguKLap { get => Powertrain.EnergyBatteryToMguKLap; set => Powertrain.EnergyBatteryToMguKLap = value; }
        public float EnergyMguKLapDeployPct { get => Powertrain.EnergyMguKLapDeployPct; set => Powertrain.EnergyMguKLapDeployPct = value; }
    }
}
