using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperBackendNR85IA.Calculations
{
    public static class TelemetryCalculations
    {
        // --- SESSÃO ---
        public static double GetSessionTime(double? sessionTime) => sessionTime ?? 0;

        // --- COMBUSTÍVEL ---
        public static double GetFuelLapsLeft(double fuelLevel, double fuelUsePerLap) =>
            fuelUsePerLap > 0 ? fuelLevel / fuelUsePerLap : 0;

        public static bool GetFuelWarning(double fuelLevel, double fuelUsePerLap) =>
            GetFuelLapsLeft(fuelLevel, fuelUsePerLap) < 2;

        public static double GetFuelForTargetLaps(int laps, double fuelUsePerLap) =>
            laps * fuelUsePerLap;

        public static double GetFuelEfficiency(double lapDistTotal, double fuelUsedTotal) =>
            fuelUsedTotal > 0 ? lapDistTotal / fuelUsedTotal : 0;

        // Consumo de combustível por volta (média)
        public static float CalculateFuelPerLap(
            float totalFuelUsedInSession,
            float currentLapDistPct,
            float lapLastLapTime,
            int currentLapNumber,
            float sdkReportedFuelPerLap)
        {
            if (currentLapNumber <= 1 && currentLapDistPct < 0.90f)
                return sdkReportedFuelPerLap > 0 ? sdkReportedFuelPerLap : 0f;

            float effectiveLapsCompleted = (currentLapNumber - 1) + currentLapDistPct;
            if (effectiveLapsCompleted > 0.1f && totalFuelUsedInSession > 0.01f)
            {
                float calculatedAverageFuelPerLap = totalFuelUsedInSession / effectiveLapsCompleted;
                if (calculatedAverageFuelPerLap > 0)
                    return calculatedAverageFuelPerLap;
            }
            return sdkReportedFuelPerLap > 0 ? sdkReportedFuelPerLap : 0f;
        }

        // --- PNEUS ---
        public static Dictionary<string, double> GetTireWear(double? fl, double? fr, double? rl, double? rr) => new()
        {
            {"FL", 100 - (fl ?? 0)},
            {"FR", 100 - (fr ?? 0)},
            {"RL", 100 - (rl ?? 0)},
            {"RR", 100 - (rr ?? 0)}
        };

        public static Dictionary<string, double[]> GetTireTemps(double[] fl, double[] fr, double[] rl, double[] rr) => new()
        {
            {"FL", fl ?? Array.Empty<double>()},
            {"FR", fr ?? Array.Empty<double>()},
            {"RL", rl ?? Array.Empty<double>()},
            {"RR", rr ?? Array.Empty<double>()}
        };

        // --- DELTA/SETORES ---
        public static Dictionary<string, double> GetDeltaTimes(double? current, double? last, double? best) => new()
        {
            {"Current", current ?? 0},
            {"Last", last ?? 0},
            {"Best", best ?? 0}
        };

        public static List<double> CalculateSectorTimes(List<double> sessionTimes, List<double> lapDistPct)
        {
            List<double> sectorTimes = new List<double>();
            double[] sectorLimits = { 0.33, 0.66, 1.0 };
            int sectorIdx = 0;
            double lastTime = sessionTimes.FirstOrDefault();

            for (int i = 1; i < lapDistPct.Count; i++)
            {
                if (sectorIdx >= sectorLimits.Length)
                    break;

                if (lapDistPct[i] >= sectorLimits[sectorIdx])
                {
                    double sectorTime = sessionTimes[i] - lastTime;
                    sectorTimes.Add(sectorTime);
                    lastTime = sessionTimes[i];
                    sectorIdx++;
                }
            }
            return sectorTimes;
        }

        // --- RELATIVE/GAPS ---
        public static (double? gapAhead, double? gapBehind) CalculateGaps(
            int playerIdx, List<int> positions, List<double> estTimes)
        {
            var ordered = positions
                .Select((pos, idx) => new { pos, idx })
                .OrderBy(x => x.pos)
                .ToList();

            int playerPos = ordered.FindIndex(x => x.idx == playerIdx);
            double? gapAhead = null, gapBehind = null;

            if (playerPos > 0)
                gapAhead = estTimes[ordered[playerPos - 1].idx] - estTimes[playerIdx];
            if (playerPos < ordered.Count - 1)
                gapBehind = estTimes[ordered[playerPos + 1].idx] - estTimes[playerIdx];

            return (gapAhead, gapBehind);
        }

        // --- STANDINGS ---
        public static List<T> GetRelativeData<T>(List<T> relativeCars) =>
            relativeCars ?? new List<T>();

        public static List<T> GetStandings<T>(List<T> standings) =>
            standings ?? new List<T>();

        // --- CLIMA ---
        public static (double? mediaAirTemp, double? mediaTrackTemp) GetMediaClima(List<double> airTemps, List<double> trackTemps)
        {
            double? mediaAir = airTemps != null && airTemps.Count > 0 ? airTemps.Average() : (double?)null;
            double? mediaTrack = trackTemps != null && trackTemps.Count > 0 ? trackTemps.Average() : (double?)null;
            return (mediaAir, mediaTrack);
        }

        // --- TRACKMAP ---
        public static (double? pctVolta, int? statusSuperficie) CalculateTrackmap(int playerIdx, List<double> lapDistPct, List<int> trackSurface)
        {
            double? pct = null;
            int? status = null;

            if (playerIdx >= 0 && playerIdx < lapDistPct.Count)
                pct = lapDistPct[playerIdx];

            if (playerIdx >= 0 && playerIdx < trackSurface.Count)
                status = trackSurface[playerIdx];

            return (pct, status);
        }

        // --- RADAR / ALERTAS ---
        public static bool DetectarCarroAproximando(double gapFrente, double velocidadeRelativa, double limiteGap = 2.0, double limiteVelocidade = 5.0)
        {
            return gapFrente < limiteGap && velocidadeRelativa > limiteVelocidade;
        }

        // --- SESSÃO (TIPO) ---
        public static string GetTipoSessao(string sessionTypeRaw)
        {
            switch (sessionTypeRaw?.ToLower())
            {
                case "race": return "Corrida";
                case "practice": return "Treino";
                case "qualify": return "Qualificação";
                default: return sessionTypeRaw ?? "Desconhecido";
            }
        }
    }
}
