using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperBackendNR85IA.Services
{
    public class CarTrackData
    {
        public string CarPath { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public float ConsumoMedio { get; set; }
        public float ConsumoUltimaVolta { get; set; }
        public float FuelCapacity { get; set; }
    }

    public class CarTrackDataStore
    {
        // Salva o arquivo no mesmo diretório do executável para evitar
        // depender do diretório de trabalho corrente da aplicação
        private static readonly string FilePath = Path.Combine(
            AppContext.BaseDirectory,
            "carTrackData.json");
        private const int SAVE_INTERVAL_SECONDS = 5;

        private readonly object _lock = new();
        private Dictionary<string, CarTrackData> _data = new();
        private DateTime _lastPersist = DateTime.UtcNow;
        private bool _dirty = false;

        public CarTrackDataStore()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllTextAsync(FilePath).GetAwaiter().GetResult();
                    _data = JsonSerializer.Deserialize<Dictionary<string, CarTrackData>>(json) ?? new();
                }
            }
            catch { _data = new(); }
        }

        private string Key(string carPath, string trackName) => $"{carPath}::{trackName}";

        public Task<CarTrackData> GetAsync(string carPath, string trackName)
        {
            lock (_lock)
            {
                var key = Key(carPath, trackName);
                if (_data.TryGetValue(key, out var d))
                    return Task.FromResult(d);
                d = new CarTrackData { CarPath = carPath, TrackName = trackName };
                _data[key] = d;
                return Task.FromResult(d);
            }
        }

        public async Task UpdateAsync(CarTrackData d)
        {
            bool shouldPersist = false;
            lock (_lock)
            {
                var key = Key(d.CarPath, d.TrackName);
                _data[key] = d;
                _dirty = true;
                if ((DateTime.UtcNow - _lastPersist).TotalSeconds >= SAVE_INTERVAL_SECONDS)
                {
                    shouldPersist = true;
                }
            }

            if (shouldPersist)
            {
                await PersistAsync();
            }
        }

        public Task SaveAsync() => PersistAsync();

        private async Task PersistAsync()
        {
            Dictionary<string, CarTrackData> snapshot;
            lock (_lock)
            {
                if (!_dirty)
                    return;
                snapshot = new Dictionary<string, CarTrackData>(_data);
                _dirty = false;
                _lastPersist = DateTime.UtcNow;
            }
            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(FilePath, json);
            }
            catch { }
        }
    }
}
