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
            void SetValue<T>(ref T[] arr, int idx, T value)
            {
                Utilities.DataValidator.EnsureArraySize(ref arr, Math.Max(maxIdx + 1, idx + 1));
                arr[idx] = value;
            }

            foreach (var d in drv)
            {
                SetValue(ref t.CarIdxCarNumbers, d.CarIdx, d.CarNumber);
                SetValue(ref t.CarIdxUserNames, d.CarIdx, d.UserName);
                SetValue(ref t.CarIdxLicStrings, d.CarIdx, d.LicString);
                SetValue(ref t.CarIdxIRatings, d.CarIdx, d.IRating);
                SetValue(ref t.CarIdxTeamNames, d.CarIdx, d.TeamName);
                SetValue(ref t.CarIdxCarClassIds, d.CarIdx, d.CarClassID);
                SetValue(ref t.CarIdxCarClassShortNames, d.CarIdx, d.CarClassShortName);
                SetValue(ref t.CarIdxCarClassEstLapTimes, d.CarIdx, d.CarClassEstLapTime);
                SetValue(ref t.CarIdxTireCompounds, d.CarIdx, d.TireCompound);
                if (d.CarIdx == t.PlayerCarIdx)
                {
                    t.TireCompound   = d.TireCompound;
                    t.Tyres.Compound = d.TireCompound;
                }
                if (_irStart.Length <= d.CarIdx)
                    Array.Resize(ref _irStart, d.CarIdx + 1);
                if (_irStart[d.CarIdx] == 0)
                    _irStart[d.CarIdx] = d.IRating;
                SetValue(ref t.CarIdxIRatingDeltas, d.CarIdx, d.IRating - _irStart[d.CarIdx]);
            }
        }
    }
}
