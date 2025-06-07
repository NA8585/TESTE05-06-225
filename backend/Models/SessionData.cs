namespace SuperBackendNR85IA.Models
{
    public record SessionData
    {
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
    }
}
