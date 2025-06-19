using System.Threading.Tasks;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private async Task PersistCarTrackData(TelemetryModel t)
        {
            if (string.IsNullOrEmpty(_carPath) || string.IsNullOrEmpty(_trackName))
                return;

            var data = new CarTrackData
            {
                CarPath = _carPath,
                TrackName = _trackName,
                ConsumoMedio = Utilities.DataValidator.EnsurePositive(t.ConsumoMedio),
                ConsumoUltimaVolta = Utilities.DataValidator.EnsurePositive(_consumoUltimaVolta),
                FuelCapacity = Utilities.DataValidator.EnsurePositive(t.FuelCapacity)
            };

            await _store.UpdateAsync(data);
        }
    }
}
