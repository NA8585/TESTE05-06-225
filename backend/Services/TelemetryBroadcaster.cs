using System;
using System.Linq;
using System.Net.WebSockets;                 // Para WebSocket e WebSocketCloseStatus
using System.Collections.Concurrent;           // Para ConcurrentDictionary
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

            foreach (var (clientId, info) in _clients)
            {
                var clientSocket = info.Socket;
                if (clientSocket.State == WebSocketState.Open)
                {
                    var bytes = info.Overlay == "inputs" ? inputsBytes : fullBytes;
                    var segment = new ArraySegment<byte>(bytes);
                    try
                    {
                        await clientSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogError(ex, $"Erro ao enviar dados para o cliente WebSocket {clientId}. Removendo cliente.");
                        await RemoveClient(clientId, clientSocket, WebSocketCloseStatus.EndpointUnavailable, "Erro durante envio");
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogWarning($"Tentativa de envio para cliente {clientId} com socket já disposed. Removendo.");
                        await RemoveClient(clientId, clientSocket, WebSocketCloseStatus.EndpointUnavailable, "Socket disposed");
                    }
                }
            }
        }

        public async Task BroadcastTireSnapshots(IEnumerable<Collectors.TelemetrySnapshot> snapshots)
        {
            if (!_clients.Any())
                return;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(snapshots, _jsonSerializerOptions);

            foreach (var (clientId, info) in _clients)
            {
                if (info.Overlay != "tire-snapshots")
                    continue;

                var clientSocket = info.Socket;
                if (clientSocket.State == WebSocketState.Open)
                {
                    var segment = new ArraySegment<byte>(bytes);
                    try
                    {
                        await clientSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogError(ex, $"Erro ao enviar dados para o cliente WebSocket {clientId}. Removendo cliente.");
                        await RemoveClient(clientId, clientSocket, WebSocketCloseStatus.EndpointUnavailable, "Erro durante envio");
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogWarning($"Tentativa de envio para cliente {clientId} com socket já disposed. Removendo.");
                        await RemoveClient(clientId, clientSocket, WebSocketCloseStatus.EndpointUnavailable, "Socket disposed");
                    }
                }
            }
        }
    }
}
