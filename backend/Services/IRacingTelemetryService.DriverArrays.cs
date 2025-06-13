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
            void Resize<T>(ref T[] arr)
            {
                if (arr == null) arr = Array.Empty<T>();
                if (arr.Length <= maxIdx)
                    Array.Resize(ref arr, maxIdx + 1);
            }

            foreach (var d in drv)
            {
                Resize(ref t.CarIdxCarNumbers);      t.CarIdxCarNumbers[d.CarIdx] = d.CarNumber;
                Resize(ref t.CarIdxUserNames);       t.CarIdxUserNames[d.CarIdx] = d.UserName;
                Resize(ref t.CarIdxLicStrings);      t.CarIdxLicStrings[d.CarIdx] = d.LicString;
                Resize(ref t.CarIdxIRatings);        t.CarIdxIRatings[d.CarIdx] = d.IRating;
                Resize(ref t.CarIdxTeamNames);       t.CarIdxTeamNames[d.CarIdx] = d.TeamName;
                Resize(ref t.CarIdxCarClassIds);     t.CarIdxCarClassIds[d.CarIdx] = d.CarClassID;
                Resize(ref t.CarIdxCarClassShortNames); t.CarIdxCarClassShortNames[d.CarIdx] = d.CarClassShortName;
                Resize(ref t.CarIdxCarClassEstLapTimes); t.CarIdxCarClassEstLapTimes[d.CarIdx] = d.CarClassEstLapTime;
                Resize(ref t.CarIdxTireCompounds);   t.CarIdxTireCompounds[d.CarIdx] = d.TireCompound;
                Resize(ref t.CarIdxIRatingDeltas);
                if (_irStart.Length <= d.CarIdx)
                    Array.Resize(ref _irStart, d.CarIdx + 1);
                if (_irStart[d.CarIdx] == 0)
                    _irStart[d.CarIdx] = d.IRating;
                t.CarIdxIRatingDeltas[d.CarIdx] = d.IRating - _irStart[d.CarIdx];
            }
        }
    }
}
