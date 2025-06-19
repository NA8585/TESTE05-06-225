using System;
using System.Threading;
using System.Threading.Tasks;
using IRSDKSharper;

namespace SuperBackendNR85IA.Collectors
{
    /// <summary>
    /// Simplified wrapper around IRSDKSharper.IRacingSdk to mimic the events
    /// from iRacingSdkWrapper used by older code.
    /// </summary>
    public class SdkWrapper
    {
        private readonly IRacingSdk _sdk = new();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;

        public int TelemetryUpdateInterval { get; set; } = 16;

        public bool IsConnected => _sdk.IsConnected;

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<TelemetryUpdateEventArgs>? TelemetryUpdated;

        public void Start()
        {
            if (_loopTask != null)
                return;

            _cts = new CancellationTokenSource();

            _sdk.OnConnected += () => Connected?.Invoke(this, EventArgs.Empty);
            _sdk.OnDisconnected += () => Disconnected?.Invoke(this, EventArgs.Empty);

            StartSdk();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _loopTask?.Wait();
            _loopTask = null;
            _sdk.Stop();
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
                // fallback below
            }
            _sdk.Start();
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_sdk.IsConnected && _sdk.IsStarted && _sdk.Data != null)
                {
                    TelemetryUpdated?.Invoke(this, new TelemetryUpdateEventArgs(_sdk.Data));
                }
                await Task.Delay(TelemetryUpdateInterval, ct);
            }
        }
    }

    public class TelemetryUpdateEventArgs : EventArgs
    {
        public IRacingSdkData Telemetry { get; }
        public TelemetryUpdateEventArgs(IRacingSdkData data) => Telemetry = data;
    }
}
