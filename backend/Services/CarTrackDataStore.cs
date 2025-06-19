using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SuperBackendNR85IA.Repositories;

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

    public class CarTrackDataStore : ICarTrackRepository
    {
        // Caminho para armazenamento do JSON contendo dados por carro/pista
        private readonly string _filePath;
        private readonly object _lock = new();
        private readonly ILogger<CarTrackDataStore> _log;
        private Dictionary<string, CarTrackData> _data = new();

        public CarTrackDataStore(IConfiguration configuration, ILogger<CarTrackDataStore> logger)
        {
            _log = logger;
            var configured = configuration["CarTrackStorePath"];
            _filePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "carTrackData.json")
                : configured;

            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _data = JsonSerializer.Deserialize<Dictionary<string, CarTrackData>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to load car/track data store");
                _data = new();
            }
        }

        private string Key(string carPath, string trackName) => $"{carPath}::{trackName}";

        private CarTrackData GetOrCreate(string carPath, string trackName)
        {
            var key = Key(carPath, trackName);
            if (_data.TryGetValue(key, out var d))
                return d;
            d = new CarTrackData { CarPath = carPath, TrackName = trackName };
            _data[key] = d;
            return d;
        }

        public CarTrackData Get(string carPath, string trackName)
        {
            lock (_lock)
            {
                return GetOrCreate(carPath, trackName);
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
                    File.WriteAllText(_filePath, json);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to persist car/track data");
                }
            }
        }

        public Task<CarTrackData> GetAsync(string carPath, string trackName)
        {
            lock (_lock)
            {
                return Task.FromResult(GetOrCreate(carPath, trackName));
            }
        }

        public async Task UpdateAsync(CarTrackData d)
        {
            Dictionary<string, CarTrackData> snapshot;
            lock (_lock)
            {
                var key = Key(d.CarPath, d.TrackName);
                _data[key] = d;
                snapshot = new Dictionary<string, CarTrackData>(_data);
            }

            try
            {
                var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to persist car/track data asynchronously");
            }
        }
    }
}