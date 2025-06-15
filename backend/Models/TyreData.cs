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

        // Pressões registradas ao sair dos boxes (pneus frios)
        public float LfColdPress { get; set; }
        public float RfColdPress { get; set; }
        public float LrColdPress { get; set; }
        public float RrColdPress { get; set; }

        public float[] LfWear { get; set; } = Array.Empty<float>();
        public float[] RfWear { get; set; } = Array.Empty<float>();
        public float[] LrWear { get; set; } = Array.Empty<float>();
        public float[] RrWear { get; set; } = Array.Empty<float>();

        public float[] LfTreadRemainingParts { get; set; } = Array.Empty<float>();
        public float[] RfTreadRemainingParts { get; set; } = Array.Empty<float>();
        public float[] LrTreadRemainingParts { get; set; } = Array.Empty<float>();
        public float[] RrTreadRemainingParts { get; set; } = Array.Empty<float>();

        public float LfLastHotPress { get; set; }
        public float RfLastHotPress { get; set; }
        public float LrLastHotPress { get; set; }
        public float RrLastHotPress { get; set; }

        // Temperaturas registradas ao sair dos boxes (pneus frios)
        public float LfColdTempCl { get; set; }
        public float LfColdTempCm { get; set; }
        public float LfColdTempCr { get; set; }
        public float RfColdTempCl { get; set; }
        public float RfColdTempCm { get; set; }
        public float RfColdTempCr { get; set; }
        public float LrColdTempCl { get; set; }
        public float LrColdTempCm { get; set; }
        public float LrColdTempCr { get; set; }
        public float RrColdTempCl { get; set; }
        public float RrColdTempCm { get; set; }
        public float RrColdTempCr { get; set; }

        // Last recorded tire temperatures when entering the pits
        public float LfLastTempCl { get; set; }
        public float LfLastTempCm { get; set; }
        public float LfLastTempCr { get; set; }
        public float RfLastTempCl { get; set; }
        public float RfLastTempCm { get; set; }
        public float RfLastTempCr { get; set; }
        public float LrLastTempCl { get; set; }
        public float LrLastTempCm { get; set; }
        public float LrLastTempCr { get; set; }
        public float RrLastTempCl { get; set; }
        public float RrLastTempCm { get; set; }
        public float RrLastTempCr { get; set; }

        // Approximate stagger values computed from ride heights (mm)
        public float FrontStagger { get; set; }
        public float RearStagger { get; set; }

        // Current tire compound for the player's car (from YAML)
        public string Compound { get; set; } = string.Empty;

        public float TreadRemainingFl { get; set; }
        public float TreadRemainingFr { get; set; }
        public float TreadRemainingRl { get; set; }
        public float TreadRemainingRr { get; set; }

        // Current ride heights for stagger calculations (meters)
        public float LfRideHeight { get; set; }
        public float RfRideHeight { get; set; }
        public float LrRideHeight { get; set; }
        public float RrRideHeight { get; set; }
    }
}
