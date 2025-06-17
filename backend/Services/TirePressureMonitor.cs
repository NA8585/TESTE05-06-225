using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IRSDKSharper;

namespace SuperBackendNR85IA.Services
{
    /// <summary>
    /// Simple monitor that reads tire pressures from IRSDKSharper and
    /// writes them to a log file. This is a minimal starting point for
    /// rewriting tire pressure handling.
    /// </summary>
    public sealed class TirePressureMonitor : IDisposable
    {
        private readonly IRacingSdk _sdk = new();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private readonly string _logFile;

        public TirePressureMonitor()
        {
            Directory.CreateDirectory("TirePressureLogs");
            _logFile = Path.Combine("TirePressureLogs", "tire_pressures.log");
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            StartSdk();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _loopTask?.Wait();
                _cts.Dispose();
                _cts = null;
            }
            _sdk.Stop();
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_sdk.IsConnected && _sdk.Data != null)
                {
                    WritePressures(_sdk.Data);
                }
                await Task.Delay(16, ct); // ~60 Hz
            }
        }

        private void StartSdk()
        {
            try
            {
                var flagsType = Type.GetType("IRSDKSharper.DefinitionFlags, IRSDKSharper");
                if (flagsType != null)
                {
                    var allValue = Enum.Parse(flagsType, "All");
                    var startMethod = _sdk.GetType().GetMethod("Start", new[] { flagsType });
                    if (startMethod != null)
                    {
                        startMethod.Invoke(_sdk, new[] { allValue });
                        return;
                    }
                }
            }
            catch
            {
                // Fallback to simple Start()
            }
            _sdk.Start();
        }

        private void WritePressures(IRacingSdkData d)
        {
            var payload = new
            {
                Timestamp = DateTime.UtcNow,
                FrontLeftCurrent = d.GetFloat("LFpress"),
                FrontRightCurrent = d.GetFloat("RFpress"),
                RearLeftCurrent = d.GetFloat("LRpress"),
                RearRightCurrent = d.GetFloat("RRpress"),
                FrontLeftLastHot = d.GetFloat("LFhotPressure"),
                FrontRightLastHot = d.GetFloat("RFhotPressure"),
                RearLeftLastHot = d.GetFloat("LRhotPressure"),
                RearRightLastHot = d.GetFloat("RRhotPressure")
            };

            var json = JsonSerializer.Serialize(payload);
            File.AppendAllText(_logFile, json + Environment.NewLine);
        }

        public void Dispose()
        {
            Stop();
            _sdk.Dispose();
        }
    }
}
