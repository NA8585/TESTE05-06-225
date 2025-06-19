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
                static float Avg(float a, float b, float c) => (a + b + c) / 3f;

                return new TireData
                {
                    CurrentPressure = Utilities.DataValidator.EnsurePositive(currentPress),
                    LastHotPressure = Utilities.DataValidator.EnsurePositive(lastHotPress),
                    ColdPressure = Utilities.DataValidator.EnsurePositive(coldPress),
                    CurrentTempInternal = tempL,
                    CurrentTempMiddle = tempM,
                    CurrentTempExternal = tempR,
                    LastHotTempInternal = lastHotL,
                    LastHotTempMiddle = lastHotM,
                    LastHotTempExternal = lastHotR,
                    ColdTempInternal = coldL,
                    ColdTempMiddle = coldM,
                    ColdTempExternal = coldR,
                    CoreTemp = Avg(tempL, tempM, tempR),
                    LastHotTemp = Avg(lastHotL, lastHotM, lastHotR),
                    ColdTemp = Avg(coldL, coldM, coldR),
                    Wear = startTread > 0f ? 1f - tread / startTread : 0f,
                    TreadRemaining = Utilities.DataValidator.EnsurePositive(tread),
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

            var fl = Map(t.LfPress, t.LfLastHotPress, t.LfColdPress,
                t.LfTempCl, t.LfTempCm, t.LfTempCr,
                t.LfLastTempCl, t.LfLastTempCm, t.LfLastTempCr,
                t.LfColdTempCl, t.LfColdTempCm, t.LfColdTempCr,
                t.TreadRemainingFl, t.StartTreadFl);

            var fr = Map(t.RfPress, t.RfLastHotPress, t.RfColdPress,
                t.RfTempCl, t.RfTempCm, t.RfTempCr,
                t.RfLastTempCl, t.RfLastTempCm, t.RfLastTempCr,
                t.RfColdTempCl, t.RfColdTempCm, t.RfColdTempCr,
                t.TreadRemainingFr, t.StartTreadFr);

            var rl = Map(t.LrPress, t.LrLastHotPress, t.LrColdPress,
                t.LrTempCl, t.LrTempCm, t.LrTempCr,
                t.LrLastTempCl, t.LrLastTempCm, t.LrLastTempCr,
                t.LrColdTempCl, t.LrColdTempCm, t.LrColdTempCr,
                t.TreadRemainingRl, t.StartTreadRl);

            var rr = Map(t.RrPress, t.RrLastHotPress, t.RrColdPress,
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
                Speed = Utilities.DataValidator.EnsurePositive(t.Speed),
                Rpm = Utilities.DataValidator.EnsurePositive(t.Rpm),
                VerticalAcceleration = t.VertAccel,
                LateralAcceleration = t.LatAccel,
                LongitudinalAcceleration = t.LonAccel,
                TireCompound = t.TireCompound
            };
        }
    }
}
