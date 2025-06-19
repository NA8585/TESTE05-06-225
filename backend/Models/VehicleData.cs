namespace SuperBackendNR85IA.Models
{
    public partial record VehicleData
    {
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
        public float CarSpeed { get; set; }

        // Extra controls and dynamics
        public float ThrottleRaw { get; set; }
        public float BrakeRaw { get; set; }
        public bool BrakeABSactive { get; set; }
        public float BrakeABSCutPct { get; set; }
        public float HandBrake { get; set; }
        public float HandBrakeRaw { get; set; }
        public float SteeringWheelAngleMax { get; set; }
        public int SteeringWheelLimiter { get; set; }
        public float SteeringWheelTorque { get; set; }
        public float SteeringWheelPeakForceNm { get; set; }
        public float YawRate { get; set; }
        public float PitchRate { get; set; }
        public float RollRate { get; set; }

        // Additional dynamics
        public float SteeringWheelPctDamper { get; set; }
        public float SteeringWheelPctTorque { get; set; }
        public float SteeringWheelPctTorqueSign { get; set; }
        public float SteeringWheelPctTorqueSignStops { get; set; }

        public bool NeedsRepair => PitRepairLeft > 0f || PitOptRepairLeft > 0f;
    }
}
