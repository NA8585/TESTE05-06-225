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
                var carNumbers = t.CarIdxCarNumbers;      Resize(ref carNumbers);      carNumbers[d.CarIdx] = d.CarNumber;      t.CarIdxCarNumbers = carNumbers;
                var userNames = t.CarIdxUserNames;        Resize(ref userNames);       userNames[d.CarIdx] = d.UserName;        t.CarIdxUserNames = userNames;
                var licStrings = t.CarIdxLicStrings;      Resize(ref licStrings);      licStrings[d.CarIdx] = d.LicString;      t.CarIdxLicStrings = licStrings;
                var iratings = t.CarIdxIRatings;          Resize(ref iratings);        iratings[d.CarIdx] = d.IRating;          t.CarIdxIRatings = iratings;
                var teamNames = t.CarIdxTeamNames;        Resize(ref teamNames);       teamNames[d.CarIdx] = d.TeamName;         t.CarIdxTeamNames = teamNames;
                var classIds = t.CarIdxCarClassIds;       Resize(ref classIds);        classIds[d.CarIdx] = d.CarClassID;       t.CarIdxCarClassIds = classIds;
                var classShort = t.CarIdxCarClassShortNames; Resize(ref classShort);   classShort[d.CarIdx] = d.CarClassShortName; t.CarIdxCarClassShortNames = classShort;
                var estTimes = t.CarIdxCarClassEstLapTimes; Resize(ref estTimes);      estTimes[d.CarIdx] = d.CarClassEstLapTime; t.CarIdxCarClassEstLapTimes = estTimes;
                var compounds = t.CarIdxTireCompounds;     Resize(ref compounds);       compounds[d.CarIdx] = d.TireCompound;     t.CarIdxTireCompounds = compounds;

                var irDeltas = t.CarIdxIRatingDeltas;      Resize(ref irDeltas);        if (_irStart.Length <= d.CarIdx) Array.Resize(ref _irStart, d.CarIdx + 1);
                if (_irStart[d.CarIdx] == 0) _irStart[d.CarIdx] = d.IRating;           irDeltas[d.CarIdx] = d.IRating - _irStart[d.CarIdx];
                t.CarIdxIRatingDeltas = irDeltas;
            }
        }
    }
}
