using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using IRSDKSharper;
using SuperBackendNR85IA.Models;
using SuperBackendNR85IA.Calculations;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private T? GetSdkValue<T>(IRacingSdkData data, string varName) where T : struct
        {
            try
            {
                if (!data.TelemetryDataProperties.TryGetValue(varName, out var datum) || datum.Count == 0)
                {
                    if (_missingVarWarned.Add(varName))
                        _log.LogWarning($"Campo {varName} não está disponível no SDK.");
                    return null;
                }

                object? value = null;
                if (typeof(T) == typeof(float)) value = data.GetFloat(datum);
                else if (typeof(T) == typeof(int)) value = data.GetInt(datum);
                else if (typeof(T) == typeof(long)) value = (long)data.GetInt(datum);
                else if (typeof(T) == typeof(bool)) value = data.GetBool(datum);
                else if (typeof(T) == typeof(double)) value = data.GetDouble(datum);
                else
                {
                    _log.LogWarning($"Tipo não suportado em GetSdkValue: {typeof(T)} para variável {varName}");
                    return null;
                }
                return (T?)value;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, $"Erro ao acessar variável {varName} como {typeof(T)}");
                return null;
            }
        }

        private string? GetSdkString(IRacingSdkData data, string varName)
        {
            try
            {
                if (!data.TelemetryDataProperties.TryGetValue(varName, out var datum) || datum.Count == 0)
                {
                    if (_missingVarWarned.Add(varName))
                        _log.LogWarning($"Campo {varName} não está disponível no SDK.");
                    return null;
                }
                var value = data.GetValue(datum);
                if (value is char[] charArray) return new string(charArray).TrimEnd('\0');
                return value?.ToString()?.TrimEnd('\0');
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, $"Erro ao acessar string {varName}");
                return null;
            }
        }

        private T?[] GetSdkArray<T>(IRacingSdkData data, string varName) where T : struct
        {
            try
            {
                if (!data.TelemetryDataProperties.TryGetValue(varName, out var datum) || datum.Count == 0)
                {
                    if (_missingVarWarned.Add(varName))
                        _log.LogWarning($"Campo {varName} não está disponível no SDK.");
                    return Array.Empty<T?>();
                }

                var arr = new T?[datum.Count];
                if (typeof(T) == typeof(float))
                {
                    float[] floatArr = new float[datum.Count];
                    data.GetFloatArray(datum, floatArr, 0, datum.Count);
                    for (int i = 0; i < datum.Count; i++) arr[i] = (T?)(object)floatArr[i];
                }
                else if (typeof(T) == typeof(int))
                {
                    int[] intArr = new int[datum.Count];
                    data.GetIntArray(datum, intArr, 0, datum.Count);
                    for (int i = 0; i < datum.Count; i++) arr[i] = (T?)(object)intArr[i];
                }
                else if (typeof(T) == typeof(bool))
                {
                    bool[] boolArr = new bool[datum.Count];
                    data.GetBoolArray(datum, boolArr, 0, datum.Count);
                    for (int i = 0; i < datum.Count; i++) arr[i] = (T?)(object)boolArr[i];
                }
                else if (typeof(T) == typeof(double))
                {
                    double[] doubleArr = new double[datum.Count];
                    data.GetDoubleArray(datum, doubleArr, 0, datum.Count);
                    for (int i = 0; i < datum.Count; i++) arr[i] = (T?)(object)doubleArr[i];
                }
                else
                {
                    _log.LogWarning($"Tipo de array não suportado em GetSdkArray: {typeof(T)} para variável {varName}");
                    return Array.Empty<T?>();
                }
                return arr;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, $"Erro ao acessar array {varName} como {typeof(T)}[]");
                return Array.Empty<T?>();
            }
        }

        private float KPaToPsi(float kpa) => kpa * 0.1450377f;

        private void PopulateVehicleData(IRacingSdkData d, TelemetryModel t)
        {
            t.Vehicle.Speed              = GetSdkValue<float>(d, "Speed") ?? 0f;
            t.Vehicle.Rpm                = GetSdkValue<float>(d, "RPM") ?? 0f;
            t.Vehicle.Throttle           = GetSdkValue<float>(d, "Throttle") ?? 0f;
            t.Vehicle.Brake              = GetSdkValue<float>(d, "Brake") ?? 0f;
            t.Vehicle.Clutch             = GetSdkValue<float>(d, "Clutch") ?? 0f;
            t.Vehicle.SteeringWheelAngle = GetSdkValue<float>(d, "SteeringWheelAngle") ?? 0f;
            t.Vehicle.Gear               = GetSdkValue<int>(d, "Gear") ?? 0;
            t.Vehicle.FuelLevel          = GetSdkValue<float>(d, "FuelLevel") ?? 0f;
            t.Vehicle.FuelLevelPct       = GetSdkValue<float>(d, "FuelLevelPct") ?? 0f;
            t.FuelCapacity               = t.Vehicle.FuelLevelPct > 0f ? t.Vehicle.FuelLevel / t.Vehicle.FuelLevelPct : 0f;
            t.Vehicle.WaterTemp          = GetSdkValue<float>(d, "WaterTemp") ?? 0f;
            t.Vehicle.OilTemp            = GetSdkValue<float>(d, "OilTemp") ?? 0f;
            t.Vehicle.OilPress           = GetSdkValue<float>(d, "OilPress") ?? 0f;
            t.Vehicle.FuelPress          = GetSdkValue<float>(d, "FuelPress") ?? 0f;
            t.Vehicle.ManifoldPress      = GetSdkValue<float>(d, "ManifoldPress") ?? 0f;
            t.Vehicle.EngineWarnings     = GetSdkValue<int>(d, "EngineWarnings") ?? 0;
            t.Vehicle.OnPitRoad          = GetSdkValue<bool>(d, "OnPitRoad") ?? false;
            t.Vehicle.PlayerCarLastPitTime = GetSdkValue<float>(d, "PlayerCarLastPitTime") ?? 0f;
            t.Vehicle.PlayerCarPitStopCount = GetSdkValue<int>(d, "PlayerCarPitStopCount") ?? 0;
            t.Vehicle.PitRepairLeft      = GetSdkValue<float>(d, "PitRepairLeft") ?? 0f;
            t.Vehicle.PitOptRepairLeft   = GetSdkValue<float>(d, "PitOptRepairLeft") ?? 0f;
            t.Vehicle.CarSpeed = t.Vehicle.Speed;
            t.Vehicle.ThrottleRaw        = GetSdkValue<float>(d, "ThrottleRaw") ?? 0f;
            t.Vehicle.BrakeRaw           = GetSdkValue<float>(d, "BrakeRaw") ?? 0f;
            t.Vehicle.BrakeABSactive     = GetSdkValue<bool>(d, "BrakeABSactive") ?? false;
            t.Vehicle.BrakeABSCutPct     = GetSdkValue<float>(d, "BrakeABSCutPct") ?? 0f;
            t.Vehicle.HandBrake          = GetSdkValue<float>(d, "HandBrake") ?? 0f;
            t.Vehicle.HandBrakeRaw       = GetSdkValue<float>(d, "HandBrakeRaw") ?? 0f;
            t.Vehicle.SteeringWheelAngleMax = GetSdkValue<float>(d, "SteeringWheelAngleMax") ?? 0f;
            t.Vehicle.SteeringWheelLimiter  = GetSdkValue<int>(d, "SteeringWheelLimiter") ?? 0;
            t.Vehicle.SteeringWheelTorque   = GetSdkValue<float>(d, "SteeringWheelTorque") ?? 0f;
            t.Vehicle.SteeringWheelPeakForceNm = GetSdkValue<float>(d, "SteeringWheelPeakForceNm") ?? 0f;
            t.Vehicle.YawRate            = GetSdkValue<float>(d, "YawRate") ?? 0f;
            t.Vehicle.PitchRate          = GetSdkValue<float>(d, "PitchRate") ?? 0f;
            t.Vehicle.RollRate           = GetSdkValue<float>(d, "RollRate") ?? 0f;
            t.Vehicle.SteeringWheelPctDamper = GetSdkValue<float>(d, "SteeringWheelPctDamper") ?? 0f;
            t.Vehicle.SteeringWheelPctTorque = GetSdkValue<float>(d, "SteeringWheelPctTorque") ?? 0f;
            t.Vehicle.SteeringWheelPctTorqueSign = GetSdkValue<float>(d, "SteeringWheelPctTorqueSign") ?? 0f;
            t.Vehicle.SteeringWheelPctTorqueSignStops = GetSdkValue<float>(d, "SteeringWheelPctTorqueSignStops") ?? 0f;
            t.Powertrain.EnergyErsBattery        = GetSdkValue<float>(d, "EnergyERSBattery") ?? 0f;
            t.Powertrain.EnergyErsBatteryPct     = GetSdkValue<float>(d, "EnergyERSBatteryPct") ?? 0f;
            t.Powertrain.EnergyMguKLapDeployPct  = GetSdkValue<float>(d, "EnergyMGU_KLapDeployPct") ?? 0f;
            t.Powertrain.EnergyBatteryToMguKLap  = GetSdkValue<float>(d, "EnergyBatteryToMGU_KLap") ?? 0f;
            t.Powertrain.ManualBoost             = GetSdkValue<bool>(d, "ManualBoost") ?? false;
            t.Powertrain.ManualNoBoost           = GetSdkValue<bool>(d, "ManualNoBoost") ?? false;

            t.Vehicle.IsOnTrack  = GetSdkValue<bool>(d, "IsOnTrack") ?? false;
            t.Vehicle.IsInGarage = GetSdkValue<bool>(d, "IsInGarage") ?? false;
            t.Vehicle.VelocityX  = GetSdkValue<float>(d, "VelocityX") ?? 0f;
            t.Vehicle.VelocityY  = GetSdkValue<float>(d, "VelocityY") ?? 0f;
            t.Vehicle.VelocityZ  = GetSdkValue<float>(d, "VelocityZ") ?? 0f;
            t.Vehicle.YawNorth   = GetSdkValue<float>(d, "YawNorth") ?? 0f;
        }

        private void UpdateLapInfo(IRacingSdkData d, TelemetryModel t)
        {
            t.Lap                         = GetSdkValue<int>(d, "Lap") ?? 0;
            t.LapDistPct                  = GetSdkValue<float>(d, "LapDistPct")?? 0f;
            t.LapCurrentLapTime           = GetSdkValue<float>(d, "LapCurrentLapTime") ?? 0f;
            t.LapLastLapTime              = GetSdkValue<float>(d, "LapLastLapTime") ?? 0f;
            t.LapBestLapTime              = GetSdkValue<float>(d, "LapBestLapTime") ?? 0f;
            t.LapDeltaToSessionBestLap    = GetSdkValue<float>(d, "LapDeltaToSessionBestLap") ?? 0f;
            t.LapDeltaToSessionOptimalLap = GetSdkValue<float>(d, "LapDeltaToSessionOptimalLap") ?? 0f;
            t.LapDeltaToDriverBestLap     = GetSdkValue<float>(d, "LapDeltaToPlayerBestLap") ?? 0f;

            if (t.Lap != _lastLap)
            {
                if (_lastLap >= 0)
                {
                    _consumoUltimaVolta = _consumoVoltaAtual;
                    if (_consumoUltimaVolta > MIN_VALID_LAP_FUEL && !t.OnPitRoad)
                    {
                        _ultimoConsumoVoltas.Enqueue(_consumoUltimaVolta);
                        while (_ultimoConsumoVoltas.Count > 3)
                            _ultimoConsumoVoltas.Dequeue();
                    }
                }
                _lastLap = t.Lap;
                _fuelAtLapStart = t.Vehicle.FuelLevel;
                _consumoVoltaAtual = 0f;
            }

            t.FuelLevelLapStart = _fuelAtLapStart;

            float diffLap = _fuelAtLapStart - t.Vehicle.FuelLevel;
            if (diffLap > 0)
                _consumoVoltaAtual = diffLap;
        }

        private void ReadSectorTimes(IRacingSdkData d, TelemetryModel t)
        {
            float[] Arr(params string[] names)
            {
                foreach (var n in names)
                {
                    var raw = GetSdkArray<float>(d, n);
                    if (raw != null && raw.Length > 0 && raw.Any(v => v.HasValue))
                        return raw.Select(v => v ?? 0f).ToArray();
                }
                return Array.Empty<float>();
            }

            var sec = _cachedYamlData.Sec;
            t.LapAllSectorTimes = Arr("LapLastLapSectorTimes", "SectorTimeSessionLastLap");
            t.SessionBestSectorTimes = Arr("SessionBestSectorTimes", "SectorTimeSessionFastestLap");

            if (t.LapAllSectorTimes.Length == 0 && sec?.SectorTimes?.Length > 0)
                t.LapAllSectorTimes = sec.SectorTimes;
            if (t.SessionBestSectorTimes.Length == 0 && sec?.BestSectorTimes?.Length > 0)
                t.SessionBestSectorTimes = sec.BestSectorTimes;

            t.SectorCount = Math.Max(Math.Max(t.LapAllSectorTimes.Length, t.SessionBestSectorTimes.Length), sec?.SectorCount ?? 0);
            if (t.SectorCount <= 0) t.SectorCount = 3;

            TelemetryCalculations.UpdateSectorData(ref t);

            t.AreSectorsValid = t.LapAllSectorTimes.Length > 0 && t.LapAllSectorTimes.Any(s => s > 0);
            t.SectorTimesDebug = string.Join(",", t.LapAllSectorTimes.Select(v => v.ToString("F3")));

            var lapOpt = GetSdkValue<float>(d, "LapOptimalLapTime") ?? 0f;
            t.EstLapTime = lapOpt > 1e-4f ? lapOpt : t.LapBestLapTime;
        }

        private void ComputeForceFeedback(IRacingSdkData d, TelemetryModel t)
        {
            t.FfbPercent = GetSdkValue<float>(d, "ForceFeedbackPct") ?? 0f;
            t.FfbClip    = GetSdkValue<bool>(d, "ForceFeedbackClip") ?? false;
            if (t.FfbPercent <= 0f)
            {
                var torqueNm = GetSdkValue<float>(d, "SteeringWheelTorque") ?? 0f;
                var maxForceNm = GetSdkValue<float>(d, "SteeringWheelMaxForce")?? 6f;
                if (maxForceNm <= 0f) maxForceNm = 6f;
                t.FfbPercent = MathF.Min(MathF.Abs(torqueNm) / maxForceNm, 1f);
                t.FfbClip = t.FfbPercent >= 0.98f;
            }
        }

        private void ComputeRelativeDistances(IRacingSdkData d, TelemetryModel t)
        {
            var lapPctArr       = GetSdkArray<float>(d, "CarIdxLapDistPct")?.Select(v => v ?? 0f).ToArray() ?? Array.Empty<float>();
            var posArr          = GetSdkArray<int>(d, "CarIdxPosition")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var lapArr          = GetSdkArray<int>(d, "CarIdxLap")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var onPitArr        = GetSdkArray<bool>(d, "CarIdxOnPitRoad")?.Select(v => v ?? false).ToArray() ?? Array.Empty<bool>();
            var trackSurfaceArr = GetSdkArray<int>(d, "CarIdxTrackSurface")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var lastLapArr      = GetSdkArray<float>(d, "CarIdxLastLapTime")?.Select(v => v ?? 0f).ToArray() ?? Array.Empty<float>();
            var f2TimeArr       = GetSdkArray<float>(d, "CarIdxF2Time")?.Select(v => v ?? 0f).ToArray() ?? Array.Empty<float>();
            var bestLapArr      = GetSdkArray<float>(d, "CarIdxBestLapTime")?.Select(v => v ?? 0f).ToArray() ?? Array.Empty<float>();
            var gearArr         = GetSdkArray<int>(d, "CarIdxGear")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var rpmArr          = GetSdkArray<float>(d, "CarIdxRPM")?.Select(v => v ?? 0f).ToArray() ?? Array.Empty<float>();
            var paceFlagsArr    = GetSdkArray<int>(d, "CarIdxPaceFlags")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var paceLineArr     = GetSdkArray<int>(d, "CarIdxPaceLine")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var paceRowArr      = GetSdkArray<int>(d, "CarIdxPaceRow")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            var surfMatArr      = GetSdkArray<int>(d, "CarIdxTrackSurfaceMaterial")?.Select(v => v ?? 0).ToArray() ?? Array.Empty<int>();
            int myIdx           = GetSdkValue<int>(d, "PlayerCarIdx") ?? -1;

            if (myIdx >= 0 && myIdx < lapPctArr.Length && lapPctArr.Length == posArr.Length)
            {
                float myPct = lapPctArr[myIdx];
                float trackKm = GetSdkValue<float>(d, "TrackLength") ?? 1f;
                float bestA = 1f, bestB = 1f;
                for (int i = 0; i < lapPctArr.Length; i++)
                {
                    if (i == myIdx) continue;
                    float otherPct = lapPctArr[i];
                    if (otherPct <= 0f) continue;
                    float delta = otherPct - myPct;
                    if (delta < -0.5f) delta += 1f;
                    if (delta > 0.5f) delta -= 1f;
                    if (delta > 0f && delta < bestA) bestA = delta;
                    if (delta < 0f && -delta < bestB) bestB = -delta;
                }
                t.DistanceAhead  = float.IsInfinity(bestA)  ? -1f : bestA  * trackKm * 1000f;
                t.DistanceBehind = float.IsInfinity(bestB) ? -1f : bestB * trackKm * 1000f;
                t.CarIdxLapDistPct  = lapPctArr;
                t.CarIdxPosition    = posArr;
                t.CarIdxLap         = lapArr;
                t.CarIdxOnPitRoad   = onPitArr;
                t.CarIdxTrackSurface= trackSurfaceArr;
                t.CarIdxLastLapTime = lastLapArr;
                t.CarIdxBestLapTime = bestLapArr;
                t.CarIdxF2Time      = f2TimeArr;
                t.Radar.CarIdxGear        = gearArr;
                t.Radar.CarIdxRPM         = rpmArr;
                t.Radar.CarIdxPaceFlags   = paceFlagsArr;
                t.Radar.CarIdxPaceLine    = paceLineArr;
                t.Radar.CarIdxPaceRow     = paceRowArr;
                t.Radar.CarIdxTrackSurfaceMaterial = surfMatArr;
            }
            else
            {
                t.DistanceAhead    = -1f;
                t.DistanceBehind   = -1f;
                t.CarIdxLapDistPct = Array.Empty<float>();
                t.CarIdxPosition   = Array.Empty<int>();
                t.CarIdxLap        = Array.Empty<int>();
                t.CarIdxOnPitRoad  = Array.Empty<bool>();
                t.CarIdxTrackSurface= Array.Empty<int>();
                t.CarIdxLastLapTime= Array.Empty<float>();
                t.CarIdxBestLapTime= Array.Empty<float>();
                t.CarIdxF2Time     = Array.Empty<float>();
                t.Radar.CarIdxGear       = Array.Empty<int>();
                t.Radar.CarIdxRPM        = Array.Empty<float>();
                t.Radar.CarIdxPaceFlags  = Array.Empty<int>();
                t.Radar.CarIdxPaceLine   = Array.Empty<int>();
                t.Radar.CarIdxPaceRow    = Array.Empty<int>();
                t.Radar.CarIdxTrackSurfaceMaterial = Array.Empty<int>();
            }
        }

        private void PopulateSessionInfo(IRacingSdkData d, TelemetryModel t)
        {
            t.Session.SessionNum        = GetSdkValue<int>(d, "SessionNum") ?? 0;
            double rawSessionTime = GetSdkValue<double>(d, "SessionTime") ?? 0.0;
            if (rawSessionTime < 0)
            {
                _log.LogWarning($"Negative SessionTime received: {rawSessionTime}");
                rawSessionTime = 0.0;
            }
            rawSessionTime = Utilities.DataValidator.EnsurePositive(rawSessionTime);

            if (_log.IsEnabled(LogLevel.Debug))
                _log.LogDebug($"Raw SessionTime: {rawSessionTime}");

            double totalSessionTime = GetSdkValue<double>(d, "SessionTimeTotal") ?? 0.0;

            t.Session.SessionTime = rawSessionTime;

            const double MAX_VALID_REMAIN = 48 * 3600.0;
            double rawRemain = GetSdkValue<double>(d, "SessionTimeRemain") ?? -1.0;
            bool remainSuspect =
                rawRemain < 0.0 ||
                double.IsNaN(rawRemain) ||
                double.IsInfinity(rawRemain) ||
                rawRemain > MAX_VALID_REMAIN ||
                (totalSessionTime > 0 && rawRemain > totalSessionTime * 1.2);

            if (remainSuspect)
            {
                double recomputed = 0.0;
                if (totalSessionTime > 0)
                {
                    recomputed = totalSessionTime - rawSessionTime;
                    if (recomputed < 0) recomputed = 0.0;
                }
                if (_log.IsEnabled(LogLevel.Debug))
                    _log.LogDebug($"SessionTimeRemain inválido ({rawRemain}), recalculado.");
                rawRemain = recomputed;
            }

            t.Session.SessionTimeRemain = rawRemain;
            t.Session.SessionTimeRemainValid = !remainSuspect;

            if (t.SessionNum != _lastSessionNum)
            {
                _lastSessionNum = t.Session.SessionNum;
                _fuelAtLapStart = t.Vehicle.FuelLevel;
                _consumoVoltaAtual = 0f;
                _consumoUltimaVolta = 0f;
                _lastLap = t.Lap;
                _awaitingStoredData = true;
            }
            t.Session.SessionState      = GetSdkValue<int>(d, "SessionState") ?? 0;
            t.Session.PaceMode          = GetSdkValue<int>(d, "PaceMode") ?? 0;
            t.Session.SessionFlags      = GetSdkValue<int>(d, "SessionFlags") ?? 0;
            t.Session.PlayerCarIdx      = GetSdkValue<int>(d, "PlayerCarIdx") ?? -1;
            t.Session.TotalLaps         = GetSdkValue<int>(d, "CurrentSessionTotalLaps") ?? -1;
            t.Session.LapsRemainingRace = GetSdkValue<int>(d, "LapsRemainingRace") ?? 0;
            t.Session.SessionTimeTotal  = (float)totalSessionTime;
            t.Session.SessionLapsTotal  = GetSdkValue<int>(d, "SessionLapsTotal") ?? 0;
            t.Session.SessionLapsRemain = GetSdkValue<int>(d, "SessionLapsRemain") ?? 0;
            t.Session.RaceLaps         = GetSdkValue<int>(d, "RaceLaps") ?? 0;
            t.Session.PitsOpen         = GetSdkValue<bool>(d, "PitsOpen") ?? false;
            t.Session.SessionUniqueID  = GetSdkValue<long>(d, "SessionUniqueID") ?? 0;
            t.Session.SessionTick      = GetSdkValue<int>(d, "SessionTick") ?? 0;
            t.Session.SessionOnJokerLap = GetSdkValue<bool>(d, "SessionOnJokerLap") ?? false;
        }

        private void PopulateTyres(IRacingSdkData d, TelemetryModel t)
        {
            t.Tyres.LfTempCl = GetSdkValue<float>(d, "LFtempCL") ?? 0f;
            t.Tyres.LfTempCm = GetSdkValue<float>(d, "LFtempCM") ?? 0f;
            t.Tyres.LfTempCr = GetSdkValue<float>(d, "LFtempCR") ?? 0f;
            t.Tyres.RfTempCl = GetSdkValue<float>(d, "RFtempCL") ?? 0f;
            t.Tyres.RfTempCm = GetSdkValue<float>(d, "RFtempCM") ?? 0f;
            t.Tyres.RfTempCr = GetSdkValue<float>(d, "RFtempCR") ?? 0f;
            t.Tyres.LrTempCl = GetSdkValue<float>(d, "LRtempCL") ?? 0f;
            t.Tyres.LrTempCm = GetSdkValue<float>(d, "LRtempCM") ?? 0f;
            t.Tyres.LrTempCr = GetSdkValue<float>(d, "LRtempCR") ?? 0f;
            t.Tyres.RrTempCl = GetSdkValue<float>(d, "RRtempCL") ?? 0f;
            t.Tyres.RrTempCm = GetSdkValue<float>(d, "RRtempCM") ?? 0f;
            t.Tyres.RrTempCr = GetSdkValue<float>(d, "RRtempCR") ?? 0f;

            // Cold pressures from the car setup (kPa)

            float? lfColdKpa = GetSdkValue<float>(d, "LFcoldPressure");
            float? rfColdKpa = GetSdkValue<float>(d, "RFcoldPressure");
            float? lrColdKpa = GetSdkValue<float>(d, "LRcoldPressure");
            float? rrColdKpa = GetSdkValue<float>(d, "RRcoldPressure");

            // Current hot pressures reported by the SDK (kPa)
            float? lfHotKpa = GetSdkValue<float>(d, "LFhotPressure");
            float? rfHotKpa = GetSdkValue<float>(d, "RFhotPressure");
            float? lrHotKpa = GetSdkValue<float>(d, "LRhotPressure");
            float? rrHotKpa = GetSdkValue<float>(d, "RRhotPressure");

            // Current tire pressures from telemetry (kPa)
            float? lfKpa = GetSdkValue<float>(d, "LFpress");
            float? rfKpa = GetSdkValue<float>(d, "RFpress");
            float? lrKpa = GetSdkValue<float>(d, "LRpress");
            float? rrKpa = GetSdkValue<float>(d, "RRpress");

            bool onPitRoad = t.OnPitRoad;

            void ApplyPressures(
                float? cold, float? hot, float? live,
                ref float coldField, ref float hotField, ref float liveField,
                ref float lastHot)
            {
                if (cold.HasValue)
                    coldField = KPaToPsi(cold.Value);

                // Prefer hot values from the SDK, falling back to the last
                // recorded entry when missing (service started mid-run).
                hotField = hot.HasValue
                    ? KPaToPsi(hot.Value)
                    : (lastHot > 0f ? lastHot : 0f);

                // Use live pressure when available, otherwise fall back to the
                // known cold pressure so the UI always has a sensible value.
                liveField = live.HasValue
                    ? KPaToPsi(live.Value)
                    : (cold.HasValue ? coldField : liveField);
            }

            float lfColdPress  = t.Tyres.LfColdPress;
            float lfHotPress   = t.Tyres.LfHotPressure;
            float lfPress      = t.Tyres.LfPress;
            ApplyPressures(lfColdKpa, lfHotKpa, lfKpa,
                ref lfColdPress,
                ref lfHotPress,
                ref lfPress,
                ref _lfLastHotPress);
            t.Tyres.LfColdPress   = lfColdPress;
            t.Tyres.LfHotPressure = lfHotPress;
            t.Tyres.LfPress       = lfPress;

            float rfColdPress  = t.Tyres.RfColdPress;
            float rfHotPress   = t.Tyres.RfHotPressure;
            float rfPress      = t.Tyres.RfPress;
            ApplyPressures(rfColdKpa, rfHotKpa, rfKpa,
                ref rfColdPress,
                ref rfHotPress,
                ref rfPress,
                ref _rfLastHotPress);
            t.Tyres.RfColdPress   = rfColdPress;
            t.Tyres.RfHotPressure = rfHotPress;
            t.Tyres.RfPress       = rfPress;

            float lrColdPress  = t.Tyres.LrColdPress;
            float lrHotPress   = t.Tyres.LrHotPressure;
            float lrPress      = t.Tyres.LrPress;
            ApplyPressures(lrColdKpa, lrHotKpa, lrKpa,
                ref lrColdPress,
                ref lrHotPress,
                ref lrPress,
                ref _lrLastHotPress);
            t.Tyres.LrColdPress   = lrColdPress;
            t.Tyres.LrHotPressure = lrHotPress;
            t.Tyres.LrPress       = lrPress;

            float rrColdPress  = t.Tyres.RrColdPress;
            float rrHotPress   = t.Tyres.RrHotPressure;
            float rrPress      = t.Tyres.RrPress;
            ApplyPressures(rrColdKpa, rrHotKpa, rrKpa,
                ref rrColdPress,
                ref rrHotPress,
                ref rrPress,
                ref _rrLastHotPress);
            t.Tyres.RrColdPress   = rrColdPress;
            t.Tyres.RrHotPressure = rrHotPress;
            t.Tyres.RrPress       = rrPress;

            if (!lfColdKpa.HasValue)
            {
                if (_log.IsEnabled(LogLevel.Debug))
                    _log.LogDebug("Cold tire pressure data not available for this car (LFcoldPressure missing).");
            }


            if (onPitRoad)
            {
                t.Tyres.LfWear = new float?[] {
                    GetSdkValue<float>(d, "LFWearL"),
                    GetSdkValue<float>(d, "LFWearM"),
                    GetSdkValue<float>(d, "LFWearR")
                }.Select(v => v ?? 0f).ToArray();
                t.Tyres.RfWear = new float?[] {
                    GetSdkValue<float>(d, "RFWearL"),
                    GetSdkValue<float>(d, "RFWearM"),
                    GetSdkValue<float>(d, "RFWearR")
                }.Select(v => v ?? 0f).ToArray();
                t.Tyres.LrWear = new float?[] {
                    GetSdkValue<float>(d, "LRWearL"),
                    GetSdkValue<float>(d, "LRWearM"),
                    GetSdkValue<float>(d, "LRWearR")
                }.Select(v => v ?? 0f).ToArray();
                t.Tyres.RrWear = new float?[] {
                    GetSdkValue<float>(d, "RRWearL"),
                    GetSdkValue<float>(d, "RRWearM"),
                    GetSdkValue<float>(d, "RRWearR")
                }.Select(v => v ?? 0f).ToArray();

                Array.Copy(t.Tyres.LfWear, _lfLastWear, 3);
                Array.Copy(t.Tyres.RfWear, _rfLastWear, 3);
                Array.Copy(t.Tyres.LrWear, _lrLastWear, 3);
                Array.Copy(t.Tyres.RrWear, _rrLastWear, 3);
            }
            else
            {
                t.Tyres.LfWear = _lfLastWear.ToArray();
                t.Tyres.RfWear = _rfLastWear.ToArray();
                t.Tyres.LrWear = _lrLastWear.ToArray();
                t.Tyres.RrWear = _rrLastWear.ToArray();
            }

            t.Tyres.LfTreadRemainingParts = t.Tyres.LfWear;
            t.Tyres.RfTreadRemainingParts = t.Tyres.RfWear;
            t.Tyres.LrTreadRemainingParts = t.Tyres.LrWear;
            t.Tyres.RrTreadRemainingParts = t.Tyres.RrWear;

            if (onPitRoad)
            {
                t.Tyres.TreadRemainingFl = GetSdkValue<float>(d, "LFWearM") ?? 0f;
                t.Tyres.TreadRemainingFr = GetSdkValue<float>(d, "RFWearM") ?? 0f;
                t.Tyres.TreadRemainingRl = GetSdkValue<float>(d, "LRWearM") ?? 0f;
                t.Tyres.TreadRemainingRr = GetSdkValue<float>(d, "RRWearM") ?? 0f;

                _lfLastTread = t.Tyres.TreadRemainingFl;
                _rfLastTread = t.Tyres.TreadRemainingFr;
                _lrLastTread = t.Tyres.TreadRemainingRl;
                _rrLastTread = t.Tyres.TreadRemainingRr;
            }
            else
            {
                t.Tyres.TreadRemainingFl = _lfLastTread;
                t.Tyres.TreadRemainingFr = _rfLastTread;
                t.Tyres.TreadRemainingRl = _lrLastTread;
                t.Tyres.TreadRemainingRr = _rrLastTread;
            }

            t.Tyres.TreadLF = GetSdkValue<float>(d, "TireLF_TreadRemaining");
            t.Tyres.TreadRF = GetSdkValue<float>(d, "TireRF_TreadRemaining");
            t.Tyres.TreadLR = GetSdkValue<float>(d, "TireLR_TreadRemaining");
            t.Tyres.TreadRR = GetSdkValue<float>(d, "TireRR_TreadRemaining");

            _log.LogDebug(
                $"PopulateTyres raw - Press LF:{t.Tyres.LfPress} RF:{t.Tyres.RfPress} " +
                $"LR:{t.Tyres.LrPress} RR:{t.Tyres.RrPress}, " +
                $"Wear LF:[{string.Join(",", t.Tyres.LfWear)}] " +
                $"RF:[{string.Join(",", t.Tyres.RfWear)}] " +
                $"LR:[{string.Join(",", t.Tyres.LrWear)}] " +
                $"RR:[{string.Join(",", t.Tyres.RrWear)}]");

            t.BrakeTemp        = GetSdkArray<float>(d, "BrakeTemp").Select(v => v ?? 0f).ToArray();
            t.LfBrakeLinePress = GetSdkValue<float>(d, "LFbrakeLinePress") ?? 0f;
            t.RfBrakeLinePress = GetSdkValue<float>(d, "RFbrakeLinePress") ?? 0f;
            t.LrBrakeLinePress = GetSdkValue<float>(d, "LRbrakeLinePress") ?? 0f;
            t.RrBrakeLinePress = GetSdkValue<float>(d, "RRbrakeLinePress") ?? 0f;
            t.DcBrakeBias      = GetSdkValue<float>(d, "dcBrakeBias") ?? 0f;
            t.DcAbs            = GetSdkValue<int>(d, "dcABS") ?? 0;
            t.DcTractionControl= GetSdkValue<int>(d, "dcTractionControl") ?? 0;
            t.DcFrontWing      = GetSdkValue<int>(d, "dcFrontWing") ?? 0;
            t.DcRearWing       = GetSdkValue<int>(d, "dcRearWing") ?? 0;
            t.DcDiffEntry      = GetSdkValue<int>(d, "dcDiffEntry") ?? 0;
            t.DcDiffMiddle     = GetSdkValue<int>(d, "dcDiffMiddle") ?? 0;
            t.DcDiffExit       = GetSdkValue<int>(d, "dcDiffExit") ?? 0;

            t.LfSuspPos    = GetSdkValue<float>(d, "LFsuspPos") ?? 0f;
            t.RfSuspPos    = GetSdkValue<float>(d, "RFsuspPos") ?? 0f;
            t.LrSuspPos    = GetSdkValue<float>(d, "LRsuspPos") ?? 0f;
            t.RrSuspPos    = GetSdkValue<float>(d, "RRsuspPos") ?? 0f;
            t.LfSuspVel    = GetSdkValue<float>(d, "LFsuspVel") ?? 0f;
            t.RfSuspVel    = GetSdkValue<float>(d, "RFsuspVel") ?? 0f;
            t.LrSuspVel    = GetSdkValue<float>(d, "LRsuspVel") ?? 0f;
            t.RrSuspVel    = GetSdkValue<float>(d, "RRsuspVel") ?? 0f;
            t.LfRideHeight = GetSdkValue<float>(d, "LFrideHeight") ?? 0f;
            t.RfRideHeight = GetSdkValue<float>(d, "RFrideHeight") ?? 0f;
            t.LrRideHeight = GetSdkValue<float>(d, "LRrideHeight") ?? 0f;
            t.RrRideHeight = GetSdkValue<float>(d, "RRrideHeight") ?? 0f;
            t.LatAccel     = GetSdkValue<float>(d, "LatAccel") ?? 0f;
            t.LonAccel     = GetSdkValue<float>(d, "LonAccel") ?? 0f;
            t.VertAccel    = GetSdkValue<float>(d, "VertAccel") ?? 0f;
            t.Yaw          = GetSdkValue<float>(d, "Yaw") ?? 0f;
            t.Pitch        = GetSdkValue<float>(d, "Pitch") ?? 0f;
            t.Roll         = GetSdkValue<float>(d, "Roll") ?? 0f;
            t.DrsStatus      = GetSdkValue<int>(d, "DrsStatus") ?? 0;
            t.CarIdxP2PCount = GetSdkArray<int>(d, "CarIdxP2P_Count").Select(v => v ?? 0).ToArray();
            t.CarIdxP2PStatus= GetSdkArray<int>(d, "CarIdxP2P_Status").Select(v => v ?? 0).ToArray();
            t.DcEnginePower  = GetSdkValue<int>(d, "dcMGUKDeploymentMode") ?? 0;

            t.TrackSurfaceTemp = GetSdkValue<float>(d, "TrackTemp") ?? 0f;
            t.TrackTempCrew    = GetSdkValue<float>(d, "TrackTempCrew") ?? 0f;
            t.TempUnits        = (GetSdkValue<bool>(d, "DisplayUnits") ?? false) ? 1 : 0;
            t.SessionTimeOfDay = GetSdkValue<float>(d, "SessionTimeOfDay") ?? 0f;
            t.TrackSurfaceMaterial = GetSdkValue<int>(d, "TrackSurfaceMaterial") ?? 0;
            t.TrackGripStatus  = GetSdkString(d, "TrackGripStatus") ?? string.Empty;
            t.TrackWetnessPCA  = GetSdkValue<float>(d, "TrackWetness") ?? 0f;
            t.AirTemp         = GetSdkValue<float>(d, "AirTemp") ?? 0f;
            t.TrackAltitude   = GetSdkValue<float>(d, "Alt") ?? 0f;
            t.TrackLatitude   = GetSdkValue<float>(d, "Lat") ?? 0f;
            t.TrackLongitude  = GetSdkValue<float>(d, "Lon") ?? 0f;
            t.AirDensity       = GetSdkValue<float>(d, "AirDensity") ?? 0f;
            t.FogLevel         = GetSdkValue<float>(d, "FogLevel") ?? 0f;
            t.Precipitation    = GetSdkValue<float>(d, "Precipitation") ?? 0f;
            t.WeatherDeclaredWet = GetSdkValue<bool>(d, "WeatherDeclaredWet") ?? false;
            t.SolarAltitude    = GetSdkValue<float>(d, "SolarAltitude") ?? 0f;
            t.SolarAzimuth     = GetSdkValue<float>(d, "SolarAzimuth") ?? 0f;
            t.CarLeftRight     = GetSdkValue<int>(d, "CarLeftRight") ?? 0;
            t.TrackStatus      = string.Join(", ", EnumTranslations.TranslateSessionFlags(t.SessionFlags));

            t.FuelUsePerHour = GetSdkValue<float>(d, "FuelUsePerHour") ?? 0f;
            t.FuelUsePerLap  = GetSdkValue<float>(d, "FuelUsePerLap") ?? 0f;
            t.FuelPerLap     = t.FuelUsePerLap;
        }

        private void PopulateDamageData(IRacingSdkData d, TelemetryModel t)
        {
            t.Damage.LfDamage        = GetSdkValue<float>(d, "LFdamage") ?? 0f;
            t.Damage.RfDamage        = GetSdkValue<float>(d, "RFdamage") ?? 0f;
            t.Damage.LrDamage        = GetSdkValue<float>(d, "LRdamage") ?? 0f;
            t.Damage.RrDamage        = GetSdkValue<float>(d, "RRdamage") ?? 0f;
            t.Damage.FrontWingDamage = GetSdkValue<float>(d, "FrontWingDamage") ?? 0f;
            t.Damage.RearWingDamage  = GetSdkValue<float>(d, "RearWingDamage") ?? 0f;
            t.Damage.EngineDamage    = GetSdkValue<float>(d, "EngineDamagePct") ?? 0f;
            t.Damage.GearboxDamage   = GetSdkValue<float>(d, "GearBoxDamagePct") ?? 0f;
            t.Damage.SuspensionDamage = (
                (GetSdkValue<float>(d, "LFsuspDamPct") ?? 0f) +
                (GetSdkValue<float>(d, "RFsuspDamPct") ?? 0f) +
                (GetSdkValue<float>(d, "LRsuspDamPct") ?? 0f) +
                (GetSdkValue<float>(d, "RRsuspDamPct") ?? 0f)
            ) / 4f;
            t.Damage.ChassisDamage = t.Damage.SuspensionDamage;
        }


        private async Task ApplyYamlData(IRacingSdkData d, TelemetryModel t)
        {
            t.SessionInfoYaml = _reader.Data?.SessionInfoYaml ?? string.Empty;
            if (!string.IsNullOrEmpty(t.SessionInfoYaml) && t.SessionInfoYaml != _lastYaml)
            {
                if (_log.IsEnabled(LogLevel.Debug))
                    _log.LogDebug($"Atualizando cache do YAML. PlayerCarIdx: {t.PlayerCarIdx}, SessionNum: {t.SessionNum}");
                _cachedYamlData = _yamlParser.ParseSessionInfo(
                    t.SessionInfoYaml,
                    t.PlayerCarIdx,
                    t.SessionNum,
                    t.Session.SessionUniqueID
                );
                LogYamlDump(t.SessionInfoYaml);
                _lastYaml = t.SessionInfoYaml;
            }

            var (drv, wkd, ses, sec, drivers) = _cachedYamlData;
            t.YamlPlayerDriver = drv;
            t.YamlWeekendInfo  = wkd;
            t.YamlSessionInfo  = ses;
            t.YamlSectorInfo   = sec;
            t.YamlDrivers      = drivers;
            if (drv != null)
            {
                t.UserName           = drv.UserName;
                t.TeamName           = drv.TeamName;
                t.CarNumber          = drv.CarNumber;
                t.IRating            = drv.IRating;
                t.LicString          = drv.LicString;
                t.LicSafetyRating    = drv.LicLevel + drv.LicSubLevel / 1000f;
                t.PlayerCarClassID   = drv.CarClassID;
                t.TireCompound       = drv.TireCompound;
                t.Tyres.Compound     = drv.TireCompound;
            }

            if (t.Vehicle.PlayerCarPitStopCount > _lastPitCount && !string.IsNullOrEmpty(t.SessionInfoYaml))
            {
                _lastPitCount = t.Vehicle.PlayerCarPitStopCount;
                try
                {
                    using var reader = new StringReader(t.SessionInfoYaml);
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);
                    var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                    if (root.Children.TryGetValue(new YamlScalarNode("CarSetup"), out var csNode) && csNode is YamlMappingNode csMap)
                    {
                        YamlMappingNode? tiresNode = null;
                        if (csMap.Children.TryGetValue(new YamlScalarNode("TiresAero"), out var taNode) && taNode is YamlMappingNode tn)
                            tiresNode = tn;
                        else if (csMap.Children.TryGetValue(new YamlScalarNode("Tires"), out var tNode) && tNode is YamlMappingNode tn2)
                            tiresNode = tn2;
                        if (tiresNode != null)
                        {
                            if (tiresNode.Children.TryGetValue(new YamlScalarNode("Compound"), out var compNode) && compNode is YamlScalarNode compVal)
                                t.Tyres.Compound = compVal.Value ?? string.Empty;
                            else if (tiresNode.Children.TryGetValue(new YamlScalarNode("CompoundName"), out var compNode2) && compNode2 is YamlScalarNode compVal2)
                                t.Tyres.Compound = compVal2.Value ?? string.Empty;
                            string GetStr(YamlMappingNode n, string key)
                            {
                                if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlScalarNode s)
                                    return s.Value ?? string.Empty;
                                return string.Empty;
                            }
                            float ParsePressure(YamlMappingNode n, string field)
                            {
                                string val = GetStr(n, field);
                                if (string.IsNullOrEmpty(val)) return 0f;
                                string num = val.Replace("kPa", string.Empty).Replace("psi", string.Empty).Trim();
                                if (!float.TryParse(num, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                                    return 0f;
                                return val.Contains("kPa") ? KPaToPsi(v) : v;
                            }
                            float ParseWear(YamlMappingNode n, string field)
                            {
                                string val = GetStr(n, field);
                                if (string.IsNullOrEmpty(val)) return 0f;
                                val = val.Replace("%", string.Empty).Trim();
                                if (!float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                                    return 0f;
                                return v;
                            }
                            if (tiresNode.Children.TryGetValue(new YamlScalarNode("LeftFront"), out var lfNode) && lfNode is YamlMappingNode lfMap)
                            {
                                t.Tyres.LfLastHotPress = ParsePressure(lfMap, "LastHotPressure");
                                var pres = ParsePressure(lfMap, "Pressure");
                                if (pres > 0f) t.Tyres.LfSetupPressure = pres;
                                var cold = ParsePressure(lfMap, "ColdPressure");
                                if (cold > 0f) t.Tyres.LfColdPress = cold;
                                t.Tyres.LfWear = new float[]
                                {
                                    ParseWear(lfMap, "TreadRemainingL"),
                                    ParseWear(lfMap, "TreadRemainingM"),
                                    ParseWear(lfMap, "TreadRemainingR")
                                };
                            }
                            if (tiresNode.Children.TryGetValue(new YamlScalarNode("RightFront"), out var rfNode) && rfNode is YamlMappingNode rfMap)
                            {
                                t.Tyres.RfLastHotPress = ParsePressure(rfMap, "LastHotPressure");
                                var pres = ParsePressure(rfMap, "Pressure");
                                if (pres > 0f) t.Tyres.RfSetupPressure = pres;
                                var cold = ParsePressure(rfMap, "ColdPressure");
                                if (cold > 0f) t.Tyres.RfColdPress = cold;
                                t.Tyres.RfWear = new float[]
                                {
                                    ParseWear(rfMap, "TreadRemainingL"),
                                    ParseWear(rfMap, "TreadRemainingM"),
                                    ParseWear(rfMap, "TreadRemainingR")
                                };
                            }
                            if (tiresNode.Children.TryGetValue(new YamlScalarNode("LeftRear"), out var lrNode) && lrNode is YamlMappingNode lrMap)
                            {
                                t.Tyres.LrLastHotPress = ParsePressure(lrMap, "LastHotPressure");
                                var pres = ParsePressure(lrMap, "Pressure");
                                if (pres > 0f) t.Tyres.LrSetupPressure = pres;
                                var cold = ParsePressure(lrMap, "ColdPressure");
                                if (cold > 0f) t.Tyres.LrColdPress = cold;
                                t.Tyres.LrWear = new float[]
                                {
                                    ParseWear(lrMap, "TreadRemainingL"),
                                    ParseWear(lrMap, "TreadRemainingM"),
                                    ParseWear(lrMap, "TreadRemainingR")
                                };
                            }
                            if (tiresNode.Children.TryGetValue(new YamlScalarNode("RightRear"), out var rrNode) && rrNode is YamlMappingNode rrMap)
                            {
                                t.Tyres.RrLastHotPress = ParsePressure(rrMap, "LastHotPressure");
                                var pres = ParsePressure(rrMap, "Pressure");
                                if (pres > 0f) t.Tyres.RrSetupPressure = pres;
                                var cold = ParsePressure(rrMap, "ColdPressure");
                                if (cold > 0f) t.Tyres.RrColdPress = cold;
                                t.Tyres.RrWear = new float[]
                                {
                                    ParseWear(rrMap, "TreadRemainingL"),
                                    ParseWear(rrMap, "TreadRemainingM"),
                                    ParseWear(rrMap, "TreadRemainingR")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to parse LastHotPressure from session YAML.");
                }
                // Persist parsed hot pressures so they are available on subsequent ticks
                if (t.Tyres.LfLastHotPress > 0f) _lfLastHotPress = t.Tyres.LfLastHotPress;
                if (t.Tyres.RfLastHotPress > 0f) _rfLastHotPress = t.Tyres.RfLastHotPress;
                if (t.Tyres.LrLastHotPress > 0f) _lrLastHotPress = t.Tyres.LrLastHotPress;
                if (t.Tyres.RrLastHotPress > 0f) _rrLastHotPress = t.Tyres.RrLastHotPress;
                if (t.Tyres.LfLastHotPress == 0f && t.Tyres.RfLastHotPress == 0f && t.Tyres.LrLastHotPress == 0f)
                    _log.LogInformation("Last hot tire pressures not available (no data in YAML after pit).");
            }
            t.PlayerCarTeamIncidentCount = GetSdkValue<int>(d, "PlayerCarTeamIncidentCount") ?? 0;
            t.PlayerCarMyIncidentCount   = GetSdkValue<int>(d, "PlayerCarMyIncidentCount") ?? 0;

            PopulateDriverArrays(drivers.ToArray(), t);
            t.IsMultiClassSession = (wkd?.NumCarClasses ?? 0) > 1 ||
                                   t.CarIdxCarClassIds.Distinct().Count() > 1;


            if (wkd != null)
            {
                t.TrackDisplayName    = wkd.TrackDisplayName;
                t.TrackConfigName     = wkd.TrackConfigName;
                t.TrackLength         = wkd.TrackLengthKm;
                t.SessionTypeFromYaml = wkd.SessionType;
                t.Skies               = EnumTranslations.TranslateSkies(GetSdkValue<int>(d, "Skies") ?? 0);
                t.ForecastType        = wkd.ForecastType;
                t.TrackWindVel        = wkd.TrackWindVel;
                t.WindSpeed           = wkd.WindSpeed;
                t.WindDir             = wkd.WindDir;
                t.TrackAirTemp        = wkd.TrackAirTemp > 0 ?
                                         wkd.TrackAirTemp : t.AirTemp;
                t.TrackNumTurns       = wkd.TrackNumTurns;
                t.AirPressure         = wkd.AirPressure;
                t.RelativeHumidity    = wkd.RelativeHumidity;
                t.ChanceOfRain        = wkd.ChanceOfRain;
            }

            if (drv != null)
                _carPath = string.IsNullOrEmpty(drv.CarPath) ? _carPath : drv.CarPath;
            if (wkd != null)
                _trackName = string.IsNullOrEmpty(wkd.TrackDisplayName) ? _trackName : wkd.TrackDisplayName;

            if (_awaitingStoredData && !string.IsNullOrEmpty(_carPath) && !string.IsNullOrEmpty(_trackName))
            {
                var saved = await _store.GetAsync(_carPath, _trackName);
                _consumoUltimaVolta = saved.ConsumoUltimaVolta;
                t.ConsumoMedio = saved.ConsumoMedio;
                if (saved.FuelCapacity > 0)
                    t.FuelCapacity = saved.FuelCapacity;
                _awaitingStoredData = false;
            }

            if (ses != null)
            {
                t.IncidentLimit = ses.IncidentLimit;
                t.TotalLaps = ses.CurrentSessionTotalLaps > 0
                    ? ses.CurrentSessionTotalLaps
                    : ((ses.SessionType?.ToLower().Contains("race") ?? false) ? 0 : -1);
                if (string.IsNullOrEmpty(t.SessionTypeFromYaml))
                    t.SessionTypeFromYaml = ses.SessionType ?? string.Empty;
                var currentDetail = ses.AllSessionsFromYaml?.FirstOrDefault(sd => sd.SessionNum == t.SessionNum);
                t.Results = currentDetail?.ResultsPositions ?? new List<ResultPosition>();
            }
            else
            {
                t.TotalLaps = -1;
            }
        }

        private void LogYamlDump(string yaml)
        {
            try
            {
                Directory.CreateDirectory("logs");
                Directory.CreateDirectory("yamls");
                File.WriteAllText(Path.Combine("yamls", "input_current.yaml"), yaml);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var logFile = Path.Combine("logs", $"yaml_dump_{timestamp}.txt");
                File.WriteAllText(logFile, yaml);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao registrar YAML.");
            }
        }
    }
}
