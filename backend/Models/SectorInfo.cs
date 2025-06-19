using System;

namespace SuperBackendNR85IA.Models
{
    public class SectorInfo
    {
        public int SectorCount { get; set; }
        public float[] SectorTimes { get; set; } = Array.Empty<float>();
        public float[] BestSectorTimes { get; set; } = Array.Empty<float>();

        public float TotalBestTime => BestSectorTimes.Length == 0 ? 0f : System.Linq.Enumerable.Sum(BestSectorTimes);
    }
}