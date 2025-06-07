namespace SuperBackendNR85IA.Models
{
    public record VehicleData
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
    }
}
