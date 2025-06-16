using System.Text.Json.Serialization;

namespace SuperBackendNR85IA.Models
{
    public class SectorsDeltaPayload
    {
        // Contagem de setores (varia por pista)
        [JsonPropertyName("sectorCount")] public int SectorCount { get; set; }

        // Tempos da volta atual por setor
        [JsonPropertyName("lapAllSectorTimes")] public float[] LapAllSectorTimes { get; set; } = System.Array.Empty<float>();

        // Melhores setores da sessão
        [JsonPropertyName("sessionBestSectorTimes")] public float[] SessionBestSectorTimes { get; set; } = System.Array.Empty<float>();

        // Delta da volta atual para melhor setor da sessão
        [JsonPropertyName("lapDeltaToSessionBestSectorTimes")] public float[] LapDeltaToSessionBestSectorTimes { get; set; } = System.Array.Empty<float>();

        // Indica se cada setor foi o melhor do piloto
        [JsonPropertyName("sectorIsBest")] public bool[] SectorIsBest { get; set; } = System.Array.Empty<bool>();

        // Delta total da volta para melhor volta da sessão
        [JsonPropertyName("lapDeltaToSessionBestLap")] public float LapDeltaToSessionBestLap { get; set; }

        // Delta para volta ótima (soma dos melhores setores)
        [JsonPropertyName("lapDeltaToSessionOptimalLap")] public float LapDeltaToSessionOptimalLap { get; set; }

        // Delta para sua melhor volta
        [JsonPropertyName("lapDeltaToDriverBestLap")] public float LapDeltaToDriverBestLap { get; set; }

        // Tempo estimado da volta
        [JsonPropertyName("estLapTime")] public float EstLapTime { get; set; }

        // Melhor volta da sessão
        [JsonPropertyName("lapBestLapTime")] public float LapBestLapTime { get; set; }
        // Última volta
        [JsonPropertyName("lapLastLapTime")] public float LapLastLapTime { get; set; }
        // Tempo da volta atual
        [JsonPropertyName("lapCurrentLapTime")] public float LapCurrentLapTime { get; set; }
    }
}
