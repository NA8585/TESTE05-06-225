using System;

namespace SuperBackendNR85IA.Models
{
    public record TyreData
    {
        public float LfTempCl { get; set; }
        public float LfTempCm { get; set; }
        public float LfTempCr { get; set; }
        public float RfTempCl { get; set; }
        public float RfTempCm { get; set; }
        public float RfTempCr { get; set; }
        public float LrTempCl { get; set; }
        public float LrTempCm { get; set; }
        public float LrTempCr { get; set; }
        public float RrTempCl { get; set; }
        public float RrTempCm { get; set; }
        public float RrTempCr { get; set; }

        public float LfPress { get; set; }
        public float RfPress { get; set; }
        public float LrPress { get; set; }
        public float RrPress { get; set; }

        public float[] LfWear { get; set; } = Array.Empty<float>();
        public float[] RfWear { get; set; } = Array.Empty<float>();
        public float[] LrWear { get; set; } = Array.Empty<float>();
        public float[] RrWear { get; set; } = Array.Empty<float>();

        public float TreadRemainingFl { get; set; }
        public float TreadRemainingFr { get; set; }
        public float TreadRemainingRl { get; set; }
        public float TreadRemainingRr { get; set; }
    }
}
