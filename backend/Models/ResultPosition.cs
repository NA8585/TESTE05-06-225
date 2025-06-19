namespace SuperBackendNR85IA.Models
{
    public class ResultPosition
    {
        public int Position { get; set; }
        public int CarIdx { get; set; }
        public float FastestTime { get; set; }
        public float LastTime { get; set; }
        public float Time { get; set; }
        public float Interval { get; set; }
        public bool OnPitRoad { get; set; }
        public bool InGarage { get; set; }
        public int PitStopCount { get; set; }
        public int NewIRating { get; set; }

        public bool Finished => Position > 0 && Time > 0f;
    }
}
