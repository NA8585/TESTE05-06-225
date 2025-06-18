using System;
using System.Linq;
using IRSDKSharper;
using SuperBackendNR85IA.Models;
using SuperBackendNR85IA.Calculations;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private void RunCustomCalculations(IRacingSdkData d, TelemetryModel t)
        {
            t.LapDistTotal  = GetSdkValue<float>(d, "LapDist") ?? 0f;
            t.FuelUsedTotal = GetSdkValue<float>(d, "SessionFuelUsed") ?? 0f;
            try
            {
                t.FuelUsePerLapCalc = t.FuelUsePerLap;
                t.EstLapTimeCalc    = t.EstLapTime;
                t.ConsumoVoltaAtual = _consumoVoltaAtual;
                if (t.ConsumoVoltaAtual <= 0)
                {
                    float[] opts = { t.FuelUsePerLap, t.FuelPerLap, t.FuelUsePerLapCalc };
                    foreach (var opt in opts)
                    {
                        if (opt > 0)
                        {
                            t.ConsumoVoltaAtual = opt;
                            break;
                        }
                    }
                }
                double lapsLeftWithCurrentFuel = TelemetryCalculations.GetFuelLapsLeft(
                    t.FuelLevel,
                    t.ConsumoVoltaAtual
                );
                t.LapsRemaining = (int)Math.Floor(lapsLeftWithCurrentFuel);
                t.ConsumoUltimaVolta = _consumoUltimaVolta;
                t.VoltasRestantesUltimaVolta = _consumoUltimaVolta > 0 ?
                    t.FuelLevel / _consumoUltimaVolta : 0f;
                float lapsEfetivos = t.Lap + t.LapDistPct;
                float novoConsumoMedio = _ultimoConsumoVoltas.Count > 0
                    ? _ultimoConsumoVoltas.Average()
                    : (lapsEfetivos > 0.5f && t.FuelUsedTotal > 0
                        ? t.FuelUsedTotal / lapsEfetivos
                        : 0f);
                if (novoConsumoMedio > 0 && !t.OnPitRoad)
                    t.ConsumoMedio = (float)Math.Round(novoConsumoMedio, 3);
                t.VoltasRestantesMedio = t.ConsumoMedio > 0
                    ? (t.FuelLevel / t.ConsumoMedio)
                    : 0f;
                if (t.TotalLaps > 0)
                {
                    t.LapsRemainingRace = Utilities.DataValidator.EnsureNonNegative(t.TotalLaps - t.Lap);
                }
                else
                {
                    t.LapsRemainingRace = (t.SessionTimeRemain > 0 && t.EstLapTime > 0 && t.EstLapTime < (60 * 20))
                        ? (int)Math.Floor(t.SessionTimeRemain / t.EstLapTime)
                        : 0;
                }
                float fuelNeededForRaceLaps = (t.LapsRemainingRace > 0 && t.ConsumoMedio > 0)
                    ? (t.LapsRemainingRace * t.ConsumoMedio) : 0;
                t.NecessarioFim = fuelNeededForRaceLaps;
                float faltante = fuelNeededForRaceLaps - t.FuelLevel;
                t.RecomendacaoAbastecimento = faltante;
                t.FuelRemaining = t.FuelLevel;
                t.FuelEta       = t.LapsRemaining * t.EstLapTime;
                if (t.FuelLevel <= 0)
                    t.FuelStatus = new FuelStatus { Text = "VAZIO", Class = "status-danger" };
                else if (t.LapsRemaining <= 1)
                    t.FuelStatus = new FuelStatus { Text = "PERIGO", Class = "status-danger" };
                else if (t.LapsRemaining < 5)
                    t.FuelStatus = new FuelStatus { Text = "ALERTA", Class = "status-warning" };
                else
                    t.FuelStatus = new FuelStatus { Text = "OK", Class = "status-ok" };
            }
            catch (Exception calcEx)
            {
                _log.LogWarning(calcEx, "Falha ao executar cálculos em TelemetryCalculations.");
                t.LapsRemaining = 0;
                t.ConsumoMedio = 0;
                t.VoltasRestantesMedio = 0;
                t.LapsRemainingRace = 0;
                t.NecessarioFim = 0;
                t.RecomendacaoAbastecimento = 0;
                t.FuelStatus = new FuelStatus { Text = "ERRO", Class = "status-danger" };
            }
        }
    }
}
