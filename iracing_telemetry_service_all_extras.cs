// IRacingTelemetryService.AllExtras.cs
// Partial class reunindo todos os métodos de coleta dos dados de telemetria adicionais
// (Controles avançados, Powertrain/ERS, Pit & estratégia, Sessão & ambiente, Radar/Relative extra,
//  Sistema & desempenho, Danos extras).  
//
// IMPORTANTE:
//  • As propriedades comentadas com "// TODO: adicionar em TelemetryModel" ainda não existem.
//    Quando você me enviar a pasta Models, vamos incluir essas propriedades nas classes corretas
//    (VehicleData, PowertrainData, PitData, etc.).  
//  • Os novos métodos são chamados de dentro de BuildTelemetryModelAsync no ponto indicado.
//  • Para manter clareza, cada grupo de variáveis está em um método separado mas todos são invocados
//    pelo wrapper PopulateAllExtraData.
//  • Qualquer variável inexistente no carro ou build atual do iRacing simplesmente retorna 0/false.

using System;
using IRSDKSharper;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        // -------- WRAPPER PRINCIPAL --------
        private void PopulateAllExtraData(IRacingSdkData d, TelemetryModel t)
        {
            PopulateAdvancedVehicleData(d, t);
            PopulatePowertrainData(d, t);
            PopulatePitStrategyData(d, t);
            PopulateSessionEnvironmentData(d, t);
            PopulateRadarExtraData(d, t);
            PopulateSystemPerformanceData(d, t);
            PopulateDamageExtraData(d, t);
        }

        #region 1. Controles Avançados (pedais & volante brutos)
        private void PopulateAdvancedVehicleData(IRacingSdkData d, TelemetryModel t)
        {
            // TODO: adicionar propriedades abaixo em VehicleData
            t.Vehicle.ThrottleRaw              = GetSdkValue<float>(d, "ThrottleRaw")              ?? 0f;
            t.Vehicle.BrakeRaw                 = GetSdkValue<float>(d, "BrakeRaw")                 ?? 0f;
            t.Vehicle.HandBrake                = GetSdkValue<float>(d, "HandBrake")                ?? 0f;
            t.Vehicle.HandBrakeRaw             = GetSdkValue<float>(d, "HandBrakeRaw")             ?? 0f;
            t.Vehicle.SteeringWheelPctTorque   = GetSdkValue<float>(d, "SteeringWheelPctTorque")   ?? 0f;
            t.Vehicle.SteeringWheelLimiter     = GetSdkValue<int>(  d, "SteeringWheelLimiter")     ?? 0;
            t.Vehicle.SteeringWheelPeakForceNm = GetSdkValue<float>(d, "SteeringWheelPeakForceNm") ?? 0f;
        }
        #endregion

        #region 2. Powertrain / ERS / Mudança de marcha / P2P
        private void PopulatePowertrainData(IRacingSdkData d, TelemetryModel t)
        {
            // TODO: adicionar propriedades em PowertrainData (ou VehicleData, se preferir)
            t.ShiftIndicatorPct           = GetSdkValue<float>(d, "ShiftIndicatorPct")           ?? 0f;
            t.ShiftPowerPct               = GetSdkValue<float>(d, "ShiftPowerPct")               ?? 0f;
            t.ShiftGrindRpm               = GetSdkValue<float>(d, "ShiftGrindRPM")               ?? 0f;
            t.ManualBoost                 = GetSdkValue<bool>( d, "ManualBoost")                 ?? false;
            t.ManualNoBoost               = GetSdkValue<bool>( d, "ManualNoBoost")               ?? false;
            t.PushToPass                  = GetSdkValue<bool>( d, "PushToPass")                  ?? false;
            t.P2PCount                    = GetSdkValue<int>(  d, "P2P_Count")                   ?? 0;
            t.P2PStatus                   = GetSdkValue<int>(  d, "P2P_Status")                  ?? 0;
            t.EnergyErsBattery            = GetSdkValue<float>(d, "EnergyERSBattery")            ?? 0f;
            t.EnergyErsBatteryPct         = GetSdkValue<float>(d, "EnergyERSBatteryPct")         ?? 0f;
            t.EnergyBatteryToMguKLap      = GetSdkValue<float>(d, "EnergyBatteryToMGU_KLap")     ?? 0f;
            t.EnergyMguKLapDeployPct      = GetSdkValue<float>(d, "EnergyMGU_KLapDeployPct")     ?? 0f;
        }
        #endregion

        #region 3. Pit‑Stop & Estratégia
        private void PopulatePitStrategyData(IRacingSdkData d, TelemetryModel t)
        {
            // TODO: criar PitData dentro de TelemetryModel
            t.Pit.PitSvFuel          = GetSdkValue<float>(d, "PitSvFuel")          ?? 0f;
            t.Pit.PitSvFlags         = GetSdkValue<int>(  d, "PitSvFlags")         ?? 0;
            t.Pit.PitSvTireCompound  = GetSdkValue<int>(  d, "PitSvTireCompound")  ?? 0;
            t.Pit.PitSvLFP           = GetSdkValue<float>(d, "PitSvLFP")           ?? 0f;
            t.Pit.PitSvLRP           = GetSdkValue<float>(d, "PitSvLRP")           ?? 0f;
            t.Pit.PitSvRFP           = GetSdkValue<float>(d, "PitSvRFP")           ?? 0f;
            t.Pit.PitSvRRP           = GetSdkValue<float>(d, "PitSvRRP")           ?? 0f;
            t.Pit.FastRepairAvailable= GetSdkValue<int>(  d, "FastRepairAvailable")?? 0;
            t.Pit.FastRepairUsed     = GetSdkValue<int>(  d, "FastRepairUsed")     ?? 0;
            t.Pit.PlayerCarInPitStall= GetSdkValue<bool>( d, "PlayerCarInPitStall")?? false;
        }
        #endregion

        #region 4. Sessão & Ambiente Extras
        private void PopulateSessionEnvironmentData(IRacingSdkData d, TelemetryModel t)
        {
            // TODO: adicionar em SessionData ou EnvironmentData
            t.Session.SessionUniqueID  = GetSdkValue<int>(  d, "SessionUniqueID")  ?? 0;
            t.Session.SessionTick      = GetSdkValue<int>(  d, "SessionTick")      ?? 0;
            t.Session.SessionTimeTotal = GetSdkValue<float>(d, "SessionTimeTotal") ?? 0f;
            t.Environment.WeatherDeclaredWet = GetSdkValue<bool>(d, "WeatherDeclaredWet") ?? false;
            t.Environment.SolarAltitude      = GetSdkValue<float>(d, "SolarAltitude")      ?? 0f;
            t.Environment.SolarAzimuth       = GetSdkValue<float>(d, "SolarAzimuth")       ?? 0f;
        }
        #endregion

        #region 5. Radar / Relative Extras
        private void PopulateRadarExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.CarIdxGear                = GetSdkArray<int>(   d, "CarIdxGear").Select(v => v ?? 0).ToArray();
            t.CarIdxRPM                 = GetSdkArray<float>( d, "CarIdxRPM").Select(v => v ?? 0f).ToArray();
            t.CarIdxSteer               = GetSdkArray<float>( d, "CarIdxSteer").Select(v => v ?? 0f).ToArray();
            t.CarIdxTrackSurfaceMaterial= GetSdkArray<int>(   d, "CarIdxTrackSurfaceMaterial").Select(v => v ?? 0).ToArray();
            t.CarIdxEstTime             = GetSdkArray<float>( d, "CarIdxEstTime").Select(v => v ?? 0f).ToArray();
            t.CarIdxPaceFlags           = GetSdkArray<int>(   d, "CarIdxPaceFlags").Select(v => v ?? 0).ToArray();
            t.CarIdxPaceLine            = GetSdkArray<int>(   d, "CarIdxPaceLine").Select(v => v ?? 0).ToArray();
            t.CarIdxPaceRow             = GetSdkArray<int>(   d, "CarIdxPaceRow").Select(v => v ?? 0).ToArray();
            // Spotter
            t.CarLeftRight              = GetSdkValue<int>(   d, "CarLeftRight") ?? 0;
        }
        #endregion

        #region 6. Sistema & Desempenho
        private void PopulateSystemPerformanceData(IRacingSdkData d, TelemetryModel t)
        {
            t.System.FrameRate          = GetSdkValue<float>(d, "FrameRate")   ?? 0f;
            t.System.CpuUsageFg         = GetSdkValue<float>(d, "CpuUsageFG")  ?? 0f;
            t.System.CpuUsageBg         = GetSdkValue<float>(d, "CpuUsageBG")  ?? 0f;
            t.System.GpuUsage           = GetSdkValue<float>(d, "GpuUsage")    ?? 0f;
            t.System.ChanLatency        = GetSdkValue<float>(d, "ChanLatency") ?? 0f;
            t.System.ChanQuality        = GetSdkValue<float>(d, "ChanQuality") ?? 0f;
            t.System.ChanAvgLatency     = GetSdkValue<float>(d, "ChanAvgLatency") ?? 0f;
            t.System.ChanClockSkew      = GetSdkValue<float>(d, "ChanClockSkew")  ?? 0f;
        }
        #endregion

        #region 7. Danos Extras
        private void PopulateDamageExtraData(IRacingSdkData d, TelemetryModel t)
        {
            t.Damage.PlayerCarWeightPenalty = GetSdkValue<float>(d, "PlayerCarWeightPenalty") ?? 0f;
            t.Damage.PlayerCarPowerAdjust   = GetSdkValue<float>(d, "PlayerCarPowerAdjust")   ?? 0f;
            t.Damage.PlayerCarTowTime       = GetSdkValue<float>(d, "PlayerCarTowTime")       ?? 0f;
        }
        #endregion
    }
}
