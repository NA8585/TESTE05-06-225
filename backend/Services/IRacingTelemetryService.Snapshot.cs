using System;
using SuperBackendNR85IA.Models;
using SuperBackendNR85IA.Snapshots;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private TelemetrySnapshot BuildTelemetrySnapshot(TelemetryModel t)
        {
            TireData Map(
                float currentPress, float lastHotPress, float coldPress,
                float tempL, float tempM, float tempR,
                float lastHotL, float lastHotM, float lastHotR,
                float coldL, float coldM, float coldR,
                float tread, float startTread)
            {
                return new TireData
                {
                    CurrentPressure = currentPress,
                    LastHotPressure = lastHotPress,
                    ColdPressure = coldPress,
                    CurrentTempInternal = tempL,
                    CurrentTempMiddle = tempM,
                    CurrentTempExternal = tempR,
                    CoreTemp = (tempL + tempM + tempR) / 3f,
                    LastHotTemp = (lastHotL + lastHotM + lastHotR) / 3f,
                    ColdTemp = (coldL + coldM + coldR) / 3f,
                    Wear = startTread > 0f ? 1f - tread / startTread : 0f,
                    TreadRemaining = tread,
                    SlipAngle = 0f,
                    SlipRatio = 0f,
                    Load = 0f,
                    Deflection = 0f,
                    RollVelocity = 0f,
                    GroundVelocity = 0f,
                    LateralForce = 0f,
                    LongitudinalForce = 0f
                };
            }

            var fl = Map(t.LfHotPressure, t.LfLastHotPress, t.LfColdPress,
                t.LfTempCl, t.LfTempCm, t.LfTempCr,
                t.LfLastTempCl, t.LfLastTempCm, t.LfLastTempCr,
                t.LfColdTempCl, t.LfColdTempCm, t.LfColdTempCr,
                t.TreadRemainingFl, t.StartTreadFl);

            var fr = Map(t.RfHotPressure, t.RfLastHotPress, t.RfColdPress,
                t.RfTempCl, t.RfTempCm, t.RfTempCr,
                t.RfLastTempCl, t.RfLastTempCm, t.RfLastTempCr,
                t.RfColdTempCl, t.RfColdTempCm, t.RfColdTempCr,
                t.TreadRemainingFr, t.StartTreadFr);

            var rl = Map(t.LrHotPressure, t.LrLastHotPress, t.LrColdPress,
                t.LrTempCl, t.LrTempCm, t.LrTempCr,
                t.LrLastTempCl, t.LrLastTempCm, t.LrLastTempCr,
                t.LrColdTempCl, t.LrColdTempCm, t.LrColdTempCr,
                t.TreadRemainingRl, t.StartTreadRl);

            var rr = Map(t.RrHotPressure, t.RrLastHotPress, t.RrColdPress,
                t.RrTempCl, t.RrTempCm, t.RrTempCr,
                t.RrLastTempCl, t.RrLastTempCm, t.RrLastTempCr,
                t.RrColdTempCl, t.RrColdTempCm, t.RrColdTempCr,
                t.TreadRemainingRr, t.StartTreadRr);

            return new TelemetrySnapshot
            {
                Timestamp = DateTime.UtcNow,
                LapNumber = t.Lap,
                LapDistance = t.LapDistPct,
                FrontLeftTire = fl,
                FrontRightTire = fr,
                RearLeftTire = rl,
                RearRightTire = rr,
                Speed = t.Speed,
                Rpm = t.Rpm,
                VerticalAcceleration = t.VertAccel,
                LateralAcceleration = t.LatAccel,
                LongitudinalAcceleration = t.LonAccel,
                TireCompound = t.TireCompound
            };
        }
    }
}
