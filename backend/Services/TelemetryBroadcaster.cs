using System;
using System.Linq;
using System.Net.WebSockets;                 // Para WebSocket e WebSocketCloseStatus
using System.Collections.Concurrent;           // Para ConcurrentDictionary
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperBackendNR85IA.Models;              // Para TelemetryModel
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SuperBackendNR85IA.Services
{
    public class TelemetryBroadcaster
    {
        private class ClientInfo
        {
            public WebSocket Socket { get; init; } = default!;
            public string Overlay { get; init; } = string.Empty;
        }

        private readonly ConcurrentDictionary<Guid, ClientInfo> _clients = new();
        private readonly ILogger<TelemetryBroadcaster> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public TelemetryBroadcaster(ILogger<TelemetryBroadcaster> logger)
        {
            _logger = logger;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task AddClient(WebSocket webSocket, string overlay, CancellationToken cancellationToken)
        {
            var clientId = Guid.NewGuid();
            _clients.TryAdd(clientId, new ClientInfo { Socket = webSocket, Overlay = overlay });
            _logger.LogInformation($"Cliente WebSocket conectado: {clientId} (overlay: {overlay}). Total: {_clients.Count}");

            try
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                while (!result.CloseStatus.HasValue && !cancellationToken.IsCancellationRequested)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    await RemoveClient(clientId, webSocket, WebSocketCloseStatus.NormalClosure, "Operação cancelada");
                }
                else
                {
                    await RemoveClient(clientId, webSocket, result.CloseStatus!.Value, result.CloseStatusDescription);
                }
            }
            catch (OperationCanceledException)
            {
                await RemoveClient(clientId, webSocket, WebSocketCloseStatus.NormalClosure, "Operação cancelada");
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                                                ex.InnerException is System.Net.Sockets.SocketException)
            {
                _logger.LogWarning($"Cliente WebSocket {clientId} desconectado abruptamente.");
                await RemoveClient(clientId, webSocket, WebSocketCloseStatus.NormalClosure, "Conexão fechada prematuramente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro com cliente WebSocket {clientId}.");
                await RemoveClient(clientId, webSocket, WebSocketCloseStatus.InternalServerError, "Erro interno");
            }
        }

        private async Task RemoveClient(Guid clientId, WebSocket webSocket, WebSocketCloseStatus closeStatus, string? statusDescription)
        {
            if (_clients.TryRemove(clientId, out _))
            {
                _logger.LogInformation($"Cliente WebSocket desconectado: {clientId}. Status: {closeStatus}, Desc: {statusDescription}. Total: {_clients.Count}");
            }
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try
                {
                    await webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Exceção ao tentar fechar o WebSocket do cliente {clientId}.");
                }
            }
            webSocket.Dispose();
        }

        public async Task BroadcastTelemetry(object fullPayload, object inputsPayload)
        {
            if (!_clients.Any())
                return;

            var fullBytes = JsonSerializer.SerializeToUtf8Bytes(fullPayload, _jsonSerializerOptions);
            var inputsBytes = JsonSerializer.SerializeToUtf8Bytes(inputsPayload, _jsonSerializerOptions);

            static async Task SendAsync(Guid id, ClientInfo info, ArraySegment<byte> bytes,
                                        ILogger logger)
            {
                var socket = info.Socket;
                if (socket.State != WebSocketState.Open) return;
                try
                {
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    logger.LogError(ex, $"Erro ao enviar dados para o cliente WebSocket {id}. Removendo cliente.");
                    await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Erro durante envio", CancellationToken.None);
                }
                catch (ObjectDisposedException)
                {
                    logger.LogWarning($"Tentativa de envio para cliente {id} com socket disposed.");
                }
            }

            var tasks = new List<Task>(_clients.Count);
            foreach (var (clientId, info) in _clients)
            {
                var bytes = info.Overlay == "inputs" ? inputsBytes : fullBytes;
                tasks.Add(SendAsync(clientId, info, new ArraySegment<byte>(bytes), _logger));
            }

            await Task.WhenAll(tasks);
        }
    }
}
