using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class TyrePayload
    {
        // Pressão instantânea (live)
        [JsonPropertyName("lfPress")] public float LfPress { get; set; }
        [JsonPropertyName("rfPress")] public float RfPress { get; set; }
        [JsonPropertyName("lrPress")] public float LrPress { get; set; }
        [JsonPropertyName("rrPress")] public float RrPress { get; set; }

        // Pressão fria (ao sair do box)
        [JsonPropertyName("lfColdPress")] public float LfColdPress { get; set; }
        [JsonPropertyName("rfColdPress")] public float RfColdPress { get; set; }
        [JsonPropertyName("lrColdPress")] public float LrColdPress { get; set; }
        [JsonPropertyName("rrColdPress")] public float RrColdPress { get; set; }

        // Pressão de setup
        [JsonPropertyName("lfSetupPressure")] public float LfSetupPressure { get; set; }
        [JsonPropertyName("rfSetupPressure")] public float RfSetupPressure { get; set; }
        [JsonPropertyName("lrSetupPressure")] public float LrSetupPressure { get; set; }
        [JsonPropertyName("rrSetupPressure")] public float RrSetupPressure { get; set; }

        // Pressão quente (direto SDK/lasthotpressure)
        [JsonPropertyName("lfHotPressure")] public float LfHotPressure { get; set; }
        [JsonPropertyName("rfHotPressure")] public float RfHotPressure { get; set; }
        [JsonPropertyName("lrHotPressure")] public float LrHotPressure { get; set; }
        [JsonPropertyName("rrHotPressure")] public float RrHotPressure { get; set; }

        // Temperaturas atuais (CL, CM, CR)
        [JsonPropertyName("lfTempCl")] public float LfTempCl { get; set; }
        [JsonPropertyName("lfTempCm")] public float LfTempCm { get; set; }
        [JsonPropertyName("lfTempCr")] public float LfTempCr { get; set; }
        [JsonPropertyName("rfTempCl")] public float RfTempCl { get; set; }
        [JsonPropertyName("rfTempCm")] public float RfTempCm { get; set; }
        [JsonPropertyName("rfTempCr")] public float RfTempCr { get; set; }
        [JsonPropertyName("lrTempCl")] public float LrTempCl { get; set; }
        [JsonPropertyName("lrTempCm")] public float LrTempCm { get; set; }
        [JsonPropertyName("lrTempCr")] public float LrTempCr { get; set; }
        [JsonPropertyName("rrTempCl")] public float RrTempCl { get; set; }
        [JsonPropertyName("rrTempCm")] public float RrTempCm { get; set; }
        [JsonPropertyName("rrTempCr")] public float RrTempCr { get; set; }

        // Temperaturas frias (ao sair do box)
        [JsonPropertyName("lfColdTempCl")] public float LfColdTempCl { get; set; }
        [JsonPropertyName("lfColdTempCm")] public float LfColdTempCm { get; set; }
        [JsonPropertyName("lfColdTempCr")] public float LfColdTempCr { get; set; }
        [JsonPropertyName("rfColdTempCl")] public float RfColdTempCl { get; set; }
        [JsonPropertyName("rfColdTempCm")] public float RfColdTempCm { get; set; }
        [JsonPropertyName("rfColdTempCr")] public float RfColdTempCr { get; set; }
        [JsonPropertyName("lrColdTempCl")] public float LrColdTempCl { get; set; }
        [JsonPropertyName("lrColdTempCm")] public float LrColdTempCm { get; set; }
        [JsonPropertyName("lrColdTempCr")] public float LrColdTempCr { get; set; }
        [JsonPropertyName("rrColdTempCl")] public float RrColdTempCl { get; set; }
        [JsonPropertyName("rrColdTempCm")] public float RrColdTempCm { get; set; }
        [JsonPropertyName("rrColdTempCr")] public float RrColdTempCr { get; set; }

        // Wear médio (0~100%) e arrays completos
        [JsonPropertyName("lfWearAvg")] public float LfWearAvg { get; set; }
        [JsonPropertyName("rfWearAvg")] public float RfWearAvg { get; set; }
        [JsonPropertyName("lrWearAvg")] public float LrWearAvg { get; set; }
        [JsonPropertyName("rrWearAvg")] public float RrWearAvg { get; set; }
        [JsonPropertyName("lfWear")] public float[] LfWear { get; set; } = System.Array.Empty<float>();
        [JsonPropertyName("rfWear")] public float[] RfWear { get; set; } = System.Array.Empty<float>();
        [JsonPropertyName("lrWear")] public float[] LrWear { get; set; } = System.Array.Empty<float>();
        [JsonPropertyName("rrWear")] public float[] RrWear { get; set; } = System.Array.Empty<float>();

        // Tread por roda (valor absoluto)
        [JsonPropertyName("treadLF")] public float? TreadLF { get; set; }
        [JsonPropertyName("treadRF")] public float? TreadRF { get; set; }
        [JsonPropertyName("treadLR")] public float? TreadLR { get; set; }
        [JsonPropertyName("treadRR")] public float? TreadRR { get; set; }

        // Composto atual
        [JsonPropertyName("compound")] public string Compound { get; set; } = string.Empty;
    }
}
