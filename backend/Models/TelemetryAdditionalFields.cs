using System;

namespace SuperBackendNR85IA.Models
{
    // Additional telemetry fields not covered elsewhere
    public partial record VehicleData
    {
        public bool IsOnTrack { get; set; }
        public bool IsInGarage { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float VelocityZ { get; set; }
        public float YawNorth { get; set; }
    }

    public partial record SessionData
    {
        public int DisplayUnits { get; set; }
        public bool DriverMarker { get; set; }
    }

    public record ReplayData
    {
        public int PlaySpeed { get; set; }
        public bool PlaySlowMotion { get; set; }
        public double SessionTime { get; set; }
        public int SessionNum { get; set; }
    }

    public record DcuData
    {
        public int DcLapStatus { get; set; }
        public int DcDriversSoFar { get; set; }
    }

    public partial class TelemetryModel
    {
        public ReplayData Replay { get; set; } = new ReplayData();
        public DcuData Dcu { get; set; } = new DcuData();

        public bool IsOnTrack { get => Vehicle.IsOnTrack; set => Vehicle.IsOnTrack = value; }
        public bool IsInGarage { get => Vehicle.IsInGarage; set => Vehicle.IsInGarage = value; }
        public float VelocityX { get => Vehicle.VelocityX; set => Vehicle.VelocityX = value; }
        public float VelocityY { get => Vehicle.VelocityY; set => Vehicle.VelocityY = value; }
        public float VelocityZ { get => Vehicle.VelocityZ; set => Vehicle.VelocityZ = value; }
        public float YawNorth { get => Vehicle.YawNorth; set => Vehicle.YawNorth = value; }

        public int DisplayUnits { get => Session.DisplayUnits; set => Session.DisplayUnits = value; }
        public bool DriverMarker { get => Session.DriverMarker; set => Session.DriverMarker = value; }

        public int ReplayPlaySpeed { get => Replay.PlaySpeed; set => Replay.PlaySpeed = value; }
        public bool ReplayPlaySlowMotion { get => Replay.PlaySlowMotion; set => Replay.PlaySlowMotion = value; }
        public double ReplaySessionTime { get => Replay.SessionTime; set => Replay.SessionTime = value; }
        public int ReplaySessionNum { get => Replay.SessionNum; set => Replay.SessionNum = value; }

        public int DcLapStatus { get => Dcu.DcLapStatus; set => Dcu.DcLapStatus = value; }
        public int DcDriversSoFar { get => Dcu.DcDriversSoFar; set => Dcu.DcDriversSoFar = value; }
    }
}
