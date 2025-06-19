// Models/SessionDetailFromYaml.cs
using System.Collections.Generic;
namespace SuperBackendNR85IA.Models
{
    public class SessionDetailFromYaml
    {
        public int SessionNum { get; set; }
        public string? SessionName { get; set; }
        public string? SessionType { get; set; }
        public int IncidentLimit { get; set; }
        public int SessionLaps { get; set; } // Adicionada para armazenar as voltas da sessão do YAML
        public List<ResultPosition>? ResultsPositions { get; set; }

        public bool HasResults => ResultsPositions != null && ResultsPositions.Count > 0;
    }
}