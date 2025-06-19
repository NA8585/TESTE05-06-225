using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SuperBackendNR85IA.Utilities
{
    public sealed class PerformanceMonitor : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operation;
        private readonly Stopwatch _sw = Stopwatch.StartNew();

        public PerformanceMonitor(ILogger logger, string operation)
        {
            _logger = logger;
            _operation = operation;
        }

        public void Dispose()
        {
            _sw.Stop();
            _logger.LogDebug($"{_operation} took {_sw.ElapsedMilliseconds} ms");
        }
    }
}
