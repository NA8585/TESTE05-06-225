namespace SuperBackendNR85IA.Models
{
    public partial record SessionData
    {
        public int SessionNum { get; set; }
        public double SessionTime { get; set; }
        public double SessionTimeRemain { get; set; }
        public bool SessionTimeRemainValid { get; set; }
        public int SessionState { get; set; }
        public int PaceMode { get; set; }
        public int SessionFlags { get; set; }
        public int PlayerCarIdx { get; set; }
        public int TotalLaps { get; set; }
        public int LapsRemainingRace { get; set; }
        public string SessionTypeFromYaml { get; set; } = string.Empty;

        // Additional session telemetry
        public float SessionTimeTotal { get; set; }
        public int SessionLapsTotal { get; set; }
        public int SessionLapsRemain { get; set; }
        public int RaceLaps { get; set; }
        public bool PitsOpen { get; set; }

        public long SessionUniqueID { get; set; }
        public int SessionTick { get; set; }
        public bool SessionOnJokerLap { get; set; }

        public System.TimeSpan TimeSpan => System.TimeSpan.FromSeconds(SessionTime);
        public System.TimeSpan TimeRemainingSpan => System.TimeSpan.FromSeconds(SessionTimeRemain);
    }
}
