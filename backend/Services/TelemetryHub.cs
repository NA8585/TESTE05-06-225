using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperBackendNR85IA.Models;
using SuperBackendNR85IA.Services;

namespace SuperBackendNR85IA.Services
{
	
	public interface IIRacingTelemetryService
    {
        TelemetryModel? BuildTelemetryModel();
    }
	
    public class WebSocketManager
    {
        private readonly List<WebSocket> _sockets = new();
        private readonly object _lock = new();

        public void Broadcast(string json)
        {
            lock (_lock)
            {
                for (int i = _sockets.Count - 1; i >= 0; i--)
                {
                    var socket = _sockets[i];
                    if (socket.State == WebSocketState.Open)
                    {
                        var buffer = Encoding.UTF8.GetBytes(json);
                        socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        _sockets.RemoveAt(i);
                    }
                }
            }
        }

        public async Task HandleConnectionAsync(WebSocket socket)
        {
            lock (_lock) _sockets.Add(socket);
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    lock (_lock) _sockets.Remove(socket);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
                }
            }
        }
    }

    public class TelemetryService : IHostedService
    {
        private Timer? _timer;
        private readonly WebSocketManager _manager;
        private readonly IIRacingTelemetryService _telemetryService;
        private readonly JsonSerializerOptions _jsonOptions;

        public TelemetryService(WebSocketManager manager, IIRacingTelemetryService telemetryService)
        {
            _manager = manager;
            _telemetryService = telemetryService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ReadAndBroadcast, null, 0, 50);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void ReadAndBroadcast(object? state)
        {
            try
            {
                var telemetry = _telemetryService.BuildTelemetryModel();
                if (telemetry != null)
                {
                    string json = JsonSerializer.Serialize(telemetry, _jsonOptions);
                    _manager.Broadcast(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro na leitura de telemetria: " + ex.Message);
            }
        }
    }
}