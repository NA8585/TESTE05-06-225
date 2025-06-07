using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
        private const string FilePath = "carTrackData.json";
        private readonly object _lock = new();
        private Dictionary<string, CarTrackData> _data = new();

        public CarTrackDataStore()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _data = JsonSerializer.Deserialize<Dictionary<string, CarTrackData>>(json) ?? new();
                }
            }
            catch { _data = new(); }
        }

        private string Key(string carPath, string trackName) => $"{carPath}::{trackName}";

        public CarTrackData Get(string carPath, string trackName)
        {
            lock (_lock)
            {
                var key = Key(carPath, trackName);
                if (_data.TryGetValue(key, out var d))
                    return d;
                d = new CarTrackData { CarPath = carPath, TrackName = trackName };
                _data[key] = d;
                return d;
            }
        }

        public void Update(CarTrackData d)
        {
            lock (_lock)
            {
                var key = Key(d.CarPath, d.TrackName);
                _data[key] = d;
                try
                {
                    var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(FilePath, json);
                }
                catch { }
            }
        }
    }
}
