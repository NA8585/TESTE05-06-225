using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Calculations
{
    public static class TelemetryCalculations
    {
        private static ILogger? _logger;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        private static float ValidateFloat(float value, string context)
        {
            float sanitized = Utilities.DataValidator.EnsurePositive(value);
            if (sanitized != value)
                _logger?.LogError($"Invalid float value in {context}: {value}");
            return sanitized;
        }

        private static double ValidateDouble(double value, string context)
        {
            double sanitized = Utilities.DataValidator.EnsurePositive(value);
            if (sanitized != value)
                _logger?.LogError($"Invalid double value in {context}: {value}");
            return sanitized;
        }

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
                if (effectiveLapsCompleted <= 0f)
                {
                    _logger?.LogError($"Invalid laps completed value: {effectiveLapsCompleted}");
                }
                else
                {
                    float calculatedAverageFuelPerLap = totalFuelUsedInSession / effectiveLapsCompleted;
                    calculatedAverageFuelPerLap = ValidateFloat(calculatedAverageFuelPerLap, nameof(CalculateFuelPerLap));
                    if (calculatedAverageFuelPerLap > 0)
                        return calculatedAverageFuelPerLap;
                }
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

        public static (int idxAhead, int idxBehind) GetAdjacentIndices(int playerIdx, int[] carPositions)
        {
            int ahead = -1, behind = -1;
            if (carPositions != null && playerIdx >= 0 && playerIdx < carPositions.Length)
            {
                int myPos = carPositions[playerIdx];
                for (int i = 0; i < carPositions.Length; i++)
                {
                    if (carPositions[i] == myPos - 1) ahead = i;
                    else if (carPositions[i] == myPos + 1) behind = i;
                    if (ahead >= 0 && behind >= 0) break;
                }
            }
            return (ahead, behind);
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

        // --- IRATING DELTA ---
        public static int[] CalculateIRatingDeltas(int[] positions, int[] ratings, int playerIdx, int playerInc)
        {
            int n = Math.Min(positions.Length, ratings.Length);
            int[] delta = new int[n];
            const double K = 30.0;
            for (int i = 0; i < n; i++)
            {
                double d = 0.0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    double expected = 1.0 / (1.0 + Math.Pow(10.0, (ratings[j] - ratings[i]) / 400.0));
                    double actual = positions[i] < positions[j] ? 1.0 : 0.0;
                    d += K * (actual - expected);
                }
                delta[i] = (int)Math.Round(d);
            }
            if (playerIdx >= 0 && playerIdx < n)
                delta[playerIdx] -= playerInc;
            return delta;
        }

        // --- FUNÇÕES UTILIZADAS POR OVERLAYS ---
        public static void UpdateFuelData(ref TelemetryModel model)
        {
            model.FuelUsePerLap = CalculateFuelPerLap(
                model.FuelUsedTotal,
                model.LapDistPct,
                model.LapLastLapTime,
                model.Lap,
                model.FuelUsePerLap
            );
            model.FuelUsePerLap = ValidateFloat(model.FuelUsePerLap, nameof(model.FuelUsePerLap));

            float diffLap = model.FuelLevelLapStart - model.FuelLevel;
            if (diffLap > 0 && !model.OnPitRoad)
                model.ConsumoVoltaAtual = diffLap;

            if (model.ConsumoVoltaAtual <= 0)
            {
                float[] opts = { model.FuelUsePerLap, model.FuelPerLap, model.FuelUsePerLapCalc };
                foreach (var opt in opts)
                {
                    if (opt > 0)
                    {
                        model.ConsumoVoltaAtual = opt;
                        break;
                    }
                }
            }

            model.LapsRemaining = (int)ValidateDouble(GetFuelLapsLeft(model.FuelLevel, model.ConsumoVoltaAtual), nameof(model.LapsRemaining));

            float lapsEfetivos = model.Lap + model.LapDistPct;
            float novoConsumoMedio = (lapsEfetivos > 0.5f && model.FuelUsedTotal > 0)
                ? model.FuelUsedTotal / lapsEfetivos
                : 0f;
            if (novoConsumoMedio > 0)
                model.ConsumoMedio = ValidateFloat(novoConsumoMedio, nameof(model.ConsumoMedio));

            model.VoltasRestantesMedio = model.ConsumoMedio > 0
                ? ValidateFloat(model.FuelLevel / model.ConsumoMedio, nameof(model.VoltasRestantesMedio))
                : 0;

            float consumoParaCalculo = model.ConsumoMedio > 0
                ? model.ConsumoMedio
                : model.ConsumoVoltaAtual;

            model.NecessarioFim = ValidateFloat(
                (float)GetFuelForTargetLaps(
                    model.LapsRemainingRace,
                    consumoParaCalculo),
                nameof(model.NecessarioFim));

            float faltante = Math.Max(0f, model.NecessarioFim - model.FuelLevel);
            model.RecomendacaoAbastecimento = ValidateFloat(
                faltante,
                nameof(model.RecomendacaoAbastecimento));
        }

        public static void UpdateSectorData(ref TelemetryModel model)
        {
            if (model.SectorCount <= 0)
            {
                model.SectorCount = Math.Max(
                    Math.Max(model.LapAllSectorTimes?.Length ?? 0,
                             model.SessionBestSectorTimes?.Length ?? 0),
                    model.YamlSectorInfo?.SectorCount ?? 0);
            }
            if (model.SectorCount <= 0)
                return;

            if (model.LapAllSectorTimes == null || model.LapAllSectorTimes.Length != model.SectorCount)
                model.LapAllSectorTimes = new float[model.SectorCount];

            if (model.LapDeltaToSessionBestSectorTimes == null || model.LapDeltaToSessionBestSectorTimes.Length != model.SectorCount)
                model.LapDeltaToSessionBestSectorTimes = new float[model.SectorCount];

            if (model.SessionBestSectorTimes == null || model.SessionBestSectorTimes.Length != model.SectorCount)
                model.SessionBestSectorTimes = new float[model.SectorCount];

            if (model.LapDeltaToSessionBestSectorTimes.Length != model.LapAllSectorTimes.Length)
                model.LapDeltaToSessionBestSectorTimes = new float[model.LapAllSectorTimes.Length];

            for (int i = 0; i < model.SectorCount; i++)
                model.LapDeltaToSessionBestSectorTimes[i] =
                    model.LapAllSectorTimes.ElementAtOrDefault(i) -
                    model.SessionBestSectorTimes.ElementAtOrDefault(i);

            if (model.SessionBestSectorTimes.Any(v => v > 0f))
                model.EstLapTime = model.SessionBestSectorTimes.Sum();
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

        public static void SanitizeModel(TelemetryModel model)
        {
            if (model == null) return;
            SanitizeObject(model);
        }

        private static void SanitizeObject(object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.GetIndexParameters().Length > 0)
                    continue; // skip indexer properties like List<T>.Item
                if (prop.PropertyType == typeof(float))
                {
                    float val = (float)(prop.GetValue(obj) ?? 0f);
                    if (float.IsNaN(val) || float.IsInfinity(val))
                        prop.SetValue(obj, 0f);
                }
                else if (prop.PropertyType == typeof(double))
                {
                    double val = (double)(prop.GetValue(obj) ?? 0.0);
                    if (double.IsNaN(val) || double.IsInfinity(val))
                        prop.SetValue(obj, 0.0);
                }
                else if (prop.PropertyType == typeof(float[]))
                {
                    var arr = (float[]?)prop.GetValue(obj);
                    if (arr != null)
                    {
                        for (int i = 0; i < arr.Length; i++)
                            if (float.IsNaN(arr[i]) || float.IsInfinity(arr[i]))
                                arr[i] = 0f;
                    }
                }
                else if (prop.PropertyType == typeof(double[]))
                {
                    var arr = (double[]?)prop.GetValue(obj);
                    if (arr != null)
                    {
                        for (int i = 0; i < arr.Length; i++)
                            if (double.IsNaN(arr[i]) || double.IsInfinity(arr[i]))
                                arr[i] = 0.0;
                    }
                }
                else if (prop.PropertyType.IsArray)
                {
                    // Skip arrays of non-float types to avoid reflective
                    // recursion (e.g., System.Array.SyncRoot references itself)
                    continue;
                }
                else if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string) && !prop.PropertyType.IsEnum)
                {
                    var child = prop.GetValue(obj);
                    if (child != null)
                        SanitizeObject(child);
                }
            }
        }
    }

    public static class TyreHelpers
    {
        public enum TempStatus { Cold, Ideal, Warning, Hot }

        public static TempStatus Classify(float temp)
        {
            if (temp < 60) return TempStatus.Cold;
            if (temp <= 85) return TempStatus.Ideal;
            if (temp <= 105) return TempStatus.Warning;
            return TempStatus.Hot;
        }

        public static TempStatus[] ClassifyTriplet(float inT, float midT, float outT)
            => new[] { Classify(inT), Classify(midT), Classify(outT) };
    }
}
