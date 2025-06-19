using System.Collections.Generic;

namespace SuperBackendNR85IA.Services
{
    public static class EnumTranslations
    {
        private static readonly Dictionary<int, string> SessionStates = new()
        {
            [0] = "Invalid",
            [1] = "GetInCar",
            [2] = "Warmup",
            [3] = "ParadeLaps",
            [4] = "Racing",
            [5] = "Checkered",
            [6] = "CoolDown"
        };

        public static string TranslateSessionState(int state) =>
            SessionStates.TryGetValue(state, out var s) ? s : "Unknown";

        private static readonly Dictionary<int, string> PaceModes = new()
        {
            [0] = "SingleFileStart",
            [1] = "DoubleFileStart",
            [2] = "SingleFileRestart",
            [3] = "DoubleFileRestart",
            [4] = "NotPacing",
            [5] = "Pacing",
            [6] = "CautionLap",
            [7] = "LastLap"
        };

        public static string TranslatePaceMode(int mode) =>
            PaceModes.TryGetValue(mode, out var p) ? p : "Unknown";

        private static readonly Dictionary<int, string> SkiesDict = new()
        {
            [0] = "Clear",
            [1] = "Partly Cloudy",
            [2] = "Mostly Cloudy",
            [3] = "Overcast"
        };

        public static string TranslateSkies(int skies) =>
            SkiesDict.TryGetValue(skies, out var s) ? s : "Unknown";

        public static List<string> TranslateSessionFlags(int flags)
        {
            var lst = new List<string>();
            if ((flags & 0x00000001) != 0) lst.Add("Checkered");
            if ((flags & 0x00000002) != 0) lst.Add("White");
            if ((flags & 0x00000004) != 0) lst.Add("Green");
            if ((flags & 0x00000008) != 0) lst.Add("Yellow");
            if ((flags & 0x00000010) != 0) lst.Add("Red");
            if ((flags & 0x00000020) != 0) lst.Add("Blue");
            if ((flags & 0x00000040) != 0) lst.Add("Debris");
            if ((flags & 0x00000080) != 0) lst.Add("Crossed");
            if ((flags & 0x00000100) != 0) lst.Add("Black");
            if ((flags & 0x00000200) != 0) lst.Add("DQ");
            if ((flags & 0x00000400) != 0) lst.Add("Servicible");
            if ((flags & 0x00001000) != 0) lst.Add("Meatball");
            if ((flags & 0x01000000) != 0) lst.Add("Caution");
            if ((flags & 0x02000000) != 0) lst.Add("CautionWaving");
            return lst;
        }

        public static List<string> TranslateEngineWarnings(int warnings)
        {
            var lst = new List<string>();
            if ((warnings & 0x01) != 0) lst.Add("WaterTemp");
            if ((warnings & 0x02) != 0) lst.Add("FuelPressure");
            if ((warnings & 0x04) != 0) lst.Add("OilPressure");
            if ((warnings & 0x08) != 0) lst.Add("EngineStalled");
            if ((warnings & 0x10) != 0) lst.Add("PitSpeedLimiter");
            if ((warnings & 0x20) != 0) lst.Add("RevLimiterActive");
            if ((warnings & 0x40) != 0) lst.Add("OilTemp");
            return lst;
        }

        // Novo: interpreta o enum CarIdxTrackSurface / TrkLoc
        public static string TranslateTrackSurface(int surface) =>
            surface switch
            {
                0 => "OffTrack",
                1 => "InPitStall",
                2 => "ApproachingPits",
                3 => "OnTrack",
                4 => "NotInWorld",
                5 => "InGarage",
                6 => "ApproachingGrid",
                7 => "OnGrid",
                _ => "Unknown",
            };
    }
}
