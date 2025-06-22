using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IRSDKSharper;

namespace SuperBackendNR85IA.Services
{
    public class TelemetryReader
    {
        private readonly IRacingSdk _sdk = new();
        private readonly ILogger<TelemetryReader> _log;
        private readonly int _updateInterval;

        public TelemetryReader(IConfiguration configuration, ILogger<TelemetryReader> logger)
        {
            _log = logger;
            _updateInterval = configuration.GetValue<int>("TelemetryManager:TelemetryUpdateIntervalMs", 16);
            _sdk.OnConnected += () => _log.LogInformation("SDK conectado ao iRacing.");
            _sdk.OnDisconnected += () => _log.LogInformation("SDK desconectado do iRacing.");
            _sdk.OnException += ex => _log.LogError(ex, "Exceção no IRSDKSharper.");
        }

        public IRacingSdkData? Data => _sdk.Data;
        public bool IsConnected => _sdk.IsConnected;
        public bool IsStarted => _sdk.IsStarted;

        public void Start()
        {
            try
            {
                _sdk.UpdateInterval = Math.Max(1, _updateInterval / 16);
                StartWithFlags();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Erro ao iniciar o IRacingSdk");
                throw;
            }
        }

        public void Stop() => _sdk.Stop();

        private void StartWithFlags()
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
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao usar DefinitionFlags. Iniciando com Start() padrão.");
            }
            _sdk.Start();
        }
    }
}
