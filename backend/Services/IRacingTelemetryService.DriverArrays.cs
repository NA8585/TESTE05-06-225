using System;
using System.Linq;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private int[] _irStart = Array.Empty<int>();

        private void PopulateDriverArrays(DriverInfo[] drv, TelemetryModel t)
        {
            if (drv == null || drv.Length == 0) return;
            int maxIdx = drv.Max(x => x.CarIdx);
            static T[] SetValue<T>(T[] arr, int size, int idx, T value)
            {
                Utilities.DataValidator.EnsureArraySize(ref arr, Math.Max(size, idx + 1));
                arr[idx] = value;
                return arr;
            }

            foreach (var d in drv)
            {
                t.CarIdxCarNumbers        = SetValue(t.CarIdxCarNumbers,        maxIdx + 1, d.CarIdx, d.CarNumber);
                t.CarIdxUserNames         = SetValue(t.CarIdxUserNames,         maxIdx + 1, d.CarIdx, d.UserName);
                t.CarIdxLicStrings        = SetValue(t.CarIdxLicStrings,        maxIdx + 1, d.CarIdx, d.LicString);
                t.CarIdxIRatings          = SetValue(t.CarIdxIRatings,          maxIdx + 1, d.CarIdx, d.IRating);
                t.CarIdxTeamNames         = SetValue(t.CarIdxTeamNames,         maxIdx + 1, d.CarIdx, d.TeamName);
                t.CarIdxCarClassIds       = SetValue(t.CarIdxCarClassIds,       maxIdx + 1, d.CarIdx, d.CarClassID);
                t.CarIdxCarClassShortNames= SetValue(t.CarIdxCarClassShortNames,maxIdx + 1, d.CarIdx, d.CarClassShortName);
                t.CarIdxCarClassEstLapTimes= SetValue(t.CarIdxCarClassEstLapTimes, maxIdx + 1, d.CarIdx, d.CarClassEstLapTime);
                t.CarIdxTireCompounds     = SetValue(t.CarIdxTireCompounds,     maxIdx + 1, d.CarIdx, d.TireCompound);
                if (d.CarIdx == t.PlayerCarIdx)
                {
                    t.TireCompound   = d.TireCompound;
                    t.Tyres.Compound = d.TireCompound;
                }
                if (_irStart.Length <= d.CarIdx)
                    Array.Resize(ref _irStart, d.CarIdx + 1);
                if (_irStart[d.CarIdx] == 0)
                    _irStart[d.CarIdx] = d.IRating;
                t.CarIdxIRatingDeltas = SetValue(t.CarIdxIRatingDeltas, maxIdx + 1, d.CarIdx, d.IRating - _irStart[d.CarIdx]);
            }
        }
    }
}
