using System.Threading.Tasks;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public sealed partial class IRacingTelemetryService
    {
        private async Task PersistCarTrackData(TelemetryModel t)
        {
            if (!string.IsNullOrEmpty(_carPath) && !string.IsNullOrEmpty(_trackName))
            {
                await _store.UpdateAsync(new CarTrackData
                {
                    CarPath = _carPath,
                    TrackName = _trackName,
                    ConsumoMedio = t.ConsumoMedio,
                    ConsumoUltimaVolta = _consumoUltimaVolta,
                    FuelCapacity = t.FuelCapacity
                });
            }
        }
    }
}
