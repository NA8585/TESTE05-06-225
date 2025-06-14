// Extensões de cálculo e preenchimento para overlays
using System;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Calculations
{
    public static class TelemetryCalculationsOverlay
    {
        public static void PreencherOverlayTanque(ref TelemetryModel model)
        {
            TelemetryCalculations.UpdateFuelData(ref model);

            float faltante = model.NecessarioFim - model.FuelLevel;
            model.FuelStatus = new FuelStatus();
            if (faltante <= 0)
            {
                model.FuelStatus.Text = "OK";
                model.FuelStatus.Class = "status-ok";
            }
            else if (faltante <= 5)
            {
                model.FuelStatus.Text = "Atenção";
                model.FuelStatus.Class = "status-warning";
            }
            else
            {
                model.FuelStatus.Text = "Crítico";
                model.FuelStatus.Class = "status-danger";
            }
        }

        public static void PreencherOverlayPneus(ref TelemetryModel model)
        {
            model.Tyres.Compound = string.IsNullOrEmpty(model.Tyres.Compound)
                ? model.TireCompound
                : model.Tyres.Compound;
            model.Tyres.LfWear ??= new float[3];
            model.Tyres.RfWear ??= new float[3];
            model.Tyres.LrWear ??= new float[3];
            model.Tyres.RrWear ??= new float[3];

            model.Tyres.LfTreadRemainingParts ??= model.Tyres.LfWear;
            model.Tyres.RfTreadRemainingParts ??= model.Tyres.RfWear;
            model.Tyres.LrTreadRemainingParts ??= model.Tyres.LrWear;
            model.Tyres.RrTreadRemainingParts ??= model.Tyres.RrWear;

            model.Tyres.LfPress = model.Tyres.LfPress;
            model.Tyres.RfPress = model.Tyres.RfPress;
            model.Tyres.LrPress = model.Tyres.LrPress;
            model.Tyres.RrPress = model.Tyres.RrPress;

            model.Tyres.LfColdPress = model.Tyres.LfColdPress;
            model.Tyres.RfColdPress = model.Tyres.RfColdPress;
            model.Tyres.LrColdPress = model.Tyres.LrColdPress;
            model.Tyres.RrColdPress = model.Tyres.RrColdPress;

            model.Tyres.LfColdTempCl = model.Tyres.LfColdTempCl;
            model.Tyres.LfColdTempCm = model.Tyres.LfColdTempCm;
            model.Tyres.LfColdTempCr = model.Tyres.LfColdTempCr;
            model.Tyres.RfColdTempCl = model.Tyres.RfColdTempCl;
            model.Tyres.RfColdTempCm = model.Tyres.RfColdTempCm;
            model.Tyres.RfColdTempCr = model.Tyres.RfColdTempCr;
            model.Tyres.LrColdTempCl = model.Tyres.LrColdTempCl;
            model.Tyres.LrColdTempCm = model.Tyres.LrColdTempCm;
            model.Tyres.LrColdTempCr = model.Tyres.LrColdTempCr;
            model.Tyres.RrColdTempCl = model.Tyres.RrColdTempCl;
            model.Tyres.RrColdTempCm = model.Tyres.RrColdTempCm;
            model.Tyres.RrColdTempCr = model.Tyres.RrColdTempCr;

            model.Tyres.FrontStagger = (model.RfRideHeight - model.LfRideHeight) * 1000f;
            model.Tyres.RearStagger  = (model.RrRideHeight - model.LrRideHeight) * 1000f;
        }

        public static void PreencherOverlaySetores(ref TelemetryModel model)
        {
            TelemetryCalculations.UpdateSectorData(ref model);
        }

        public static void PreencherOverlayDelta(ref TelemetryModel model)
        {
            // Delta de tempo para o carro imediatamente à frente e atrás
            model.TimeDeltaToCarAhead = 0f;
            model.TimeDeltaToCarBehind = 0f;
            model.CarAheadName = string.Empty;
            model.CarBehindName = string.Empty;

            bool aheadFound = false, behindFound = false;
            int aheadIdx = -1, behindIdx = -1;

            if (model.CarIdxPosition.Length == model.CarIdxF2Time.Length &&
                model.PlayerCarIdx >= 0 && model.PlayerCarIdx < model.CarIdxPosition.Length)
            {
                int myPos = model.CarIdxPosition[model.PlayerCarIdx];

                for (int i = 0; i < model.CarIdxPosition.Length; i++)
                {
                    if (!aheadFound && model.CarIdxPosition[i] == myPos - 1 && i < model.CarIdxF2Time.Length)
                    {
                        // CarIdxF2Time[i] representa a diferença do carro i para o jogador
                        float val = -model.CarIdxF2Time[i];
                        if (Math.Abs(val) < 300f)
                        {
                            model.TimeDeltaToCarAhead = val;
                            aheadFound = true;
                            aheadIdx = i;
                        }

                    }
                    else if (!behindFound && model.CarIdxPosition[i] == myPos + 1 && i < model.CarIdxF2Time.Length)
                    {
                        float val = model.CarIdxF2Time[i];
                        if (Math.Abs(val) < 300f)
                        {
                            model.TimeDeltaToCarBehind = val;
                            behindFound = true;
                            behindIdx = i;
                        }
                    }

                    if (aheadFound && behindFound)
                        break;
                }
            }

            // Cálculo alternativo baseado em distância na pista caso F2Time não esteja válido
            if ((!aheadFound || !behindFound) &&
                model.CarIdxLapDistPct.Length == model.CarIdxPosition.Length &&
                model.CarIdxLap.Length == model.CarIdxPosition.Length &&
                model.PlayerCarIdx >= 0 && model.PlayerCarIdx < model.CarIdxPosition.Length &&
                model.TrackLength > 0f)
            {
                int myPos = model.CarIdxPosition[model.PlayerCarIdx];
                int myLap = model.CarIdxLap[model.PlayerCarIdx];
                float myPct = model.CarIdxLapDistPct[model.PlayerCarIdx];
                float trackMeters = model.TrackLength * 1000f;
                float speed = model.CarSpeed > 0.1f ? model.CarSpeed : 0f;

                for (int i = 0; i < model.CarIdxPosition.Length; i++)
                {
                    if (!aheadFound && model.CarIdxPosition[i] == myPos - 1)
                    {
                        float otherPct = model.CarIdxLap[i] + model.CarIdxLapDistPct[i];
                        float myPctAbs = myLap + myPct;
                        float t = GetDeltaTime(myPctAbs, otherPct, trackMeters, speed);
                            if (Math.Abs(t) > 0.001f)
                            {
                                model.TimeDeltaToCarAhead = t;
                                aheadFound = true;
                                aheadIdx = i;
                            }
                    }
                    else if (!behindFound && model.CarIdxPosition[i] == myPos + 1)
                    {
                        float otherPct = model.CarIdxLap[i] + model.CarIdxLapDistPct[i];
                        float myPctAbs = myLap + myPct;
                        float t = GetDeltaTime(myPctAbs, otherPct, trackMeters, speed);
                            if (Math.Abs(t) > 0.001f)
                            {
                                model.TimeDeltaToCarBehind = t;
                                behindFound = true;
                                behindIdx = i;
                            }
                    }

                    if (aheadFound && behindFound)
                        break;
                }
            }

            // Fallback usando distâncias pré-calculadas se nada mais deu certo
            float fallbackSpeed = model.CarSpeed > 0.1f ? model.CarSpeed : 0f;
            if (!aheadFound && model.DistanceAhead > 0f && fallbackSpeed > 0f)
                model.TimeDeltaToCarAhead = model.DistanceAhead / fallbackSpeed;
            if (!behindFound && model.DistanceBehind > 0f && fallbackSpeed > 0f)
                model.TimeDeltaToCarBehind = model.DistanceBehind / fallbackSpeed;

            var (idxA, idxB) = TelemetryCalculations.GetAdjacentIndices(
                model.PlayerCarIdx,
                model.CarIdxPosition ?? Array.Empty<int>());

            if (aheadIdx < 0) aheadIdx = idxA;
            if (behindIdx < 0) behindIdx = idxB;

            model.CarAheadName = (aheadIdx >= 0 && aheadIdx < model.CarIdxUserNames.Length)
                ? model.CarIdxUserNames[aheadIdx]
                : string.Empty;
            model.CarBehindName = (behindIdx >= 0 && behindIdx < model.CarIdxUserNames.Length)
                ? model.CarIdxUserNames[behindIdx]
                : string.Empty;

            model.SectorDeltas = model.LapDeltaToSessionBestSectorTimes ?? Array.Empty<float>();

            if (model.LapAllSectorTimes.Length == model.SessionBestSectorTimes.Length &&
                model.LapAllSectorTimes.Length > 0)
            {
                int len = model.LapAllSectorTimes.Length;
                var flags = new bool[len];
                for (int i = 0; i < len; i++)
                {
                    float lap = model.LapAllSectorTimes[i];
                    float best = model.SessionBestSectorTimes[i];
                    flags[i] = lap > 0 && Math.Abs(lap - best) < 1e-4f;
                }
                model.SectorIsBest = flags;
            }
            else
            {
                model.SectorIsBest = Array.Empty<bool>();
            }
        }

        private static float GetDeltaTime(float fromPct, float toPct, float trackMeters, float speed)
        {
            if (trackMeters <= 0f || speed <= 0f)
                return 0f;

            float delta = toPct - fromPct;
            if (delta > 0.5f)
                delta -= 1f;
            else if (delta < -0.5f)
                delta += 1f;

            return -(delta * trackMeters) / speed;
        }
    }
}
