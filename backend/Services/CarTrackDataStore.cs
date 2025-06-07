using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SuperBackendNR85IA.Models;

namespace SuperBackendNR85IA.Services
{
    public class CarTrackData
    {
        public string CarPath { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string TrackConfig { get; set; } = string.Empty;
        public float ConsumoMedio { get; set; }
        public float ConsumoUltimaVolta { get; set; }
    }

    public class CarTrackDataStore
    {
        private readonly string _filePath;
        private readonly ILogger<CarTrackDataStore> _log;
        private readonly Dictionary<string, CarTrackData> _data = new();

        public CarTrackDataStore(ILogger<CarTrackDataStore> log)
        {
            _log = log;
            _filePath = Path.Combine(AppContext.BaseDirectory, "carTrackData.json");
            Load();
        }

        private string Key(string car, string track, string config) => $"{car}|{track}|{config}";

        private void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var list = JsonSerializer.Deserialize<List<CarTrackData>>(json) ?? new List<CarTrackData>();
                    foreach (var item in list)
                    {
                        _data[Key(item.CarPath, item.TrackName, item.TrackConfig)] = item;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao carregar dados de carro/pista.");
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_data.Values.ToList(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao salvar dados de carro/pista.");
            }
        }

        public CarTrackData Get(string car, string track, string config)
        {
            _data.TryGetValue(Key(car, track, config), out var d);
            return d ?? new CarTrackData { CarPath = car, TrackName = track, TrackConfig = config };
        }

        public void Update(CarTrackData data)
        {
            _data[Key(data.CarPath, data.TrackName, data.TrackConfig)] = data;
            Save();
        }
    }
}

