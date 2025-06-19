// Models/SessionInfo.cs
using System.Collections.Generic;

namespace SuperBackendNR85IA.Models
{
    public class SessionInfo
    {
        public int SessionNum { get; set; }
        public string? SessionName { get; set; }
        public string? SessionType { get; set; }
        public int NumTrackSessions { get; set; }
        public List<SessionDetailFromYaml>? AllSessionsFromYaml { get; set; }
        public int IncidentLimit { get; set; }
        public int CurrentSessionTotalLaps { get; set; } // Adicionada para o total de voltas da sessão atual

        public string DisplayName => $"{SessionName} ({SessionType})";
    }
}