// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using SuperBackendNR85IA.Services;

var builder = WebApplication.CreateBuilder(args);

// Porta única para HTTP + WebSocket, igual às overlays
// Permite definir a URL de binding via variável de ambiente BACKEND_BIND_URL
var bindUrl = Environment.GetEnvironmentVariable("BACKEND_BIND_URL")
               ?? "http://0.0.0.0:5221"; // aceita conexões de outras máquinas
builder.WebHost.UseUrls(bindUrl);

// DI ------------------------------------------------------------------------
builder.Services.AddSingleton<TelemetryBroadcaster>();
builder.Services.AddSingleton<CarTrackDataStore>();
builder.Services.AddSingleton<SessionYamlParser>();
builder.Services.AddHostedService<IRacingTelemetryService>();

// Serializa todas as propriedades em camelCase
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Arquivos estáticos (wwwroot) – se tiver páginas de diagnóstico
app.UseDefaultFiles();
app.UseStaticFiles();

// WebSockets ----------------------------------------------------------------
app.UseWebSockets();
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = context.RequestServices.GetRequiredService<TelemetryBroadcaster>();
        await handler.AddClient(webSocket, context.RequestAborted); // passa o CancellationToken para permitir cancelamento gracioso
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Mapeia os controllers para REST futuro (se necessário)
app.MapControllers();

await app.RunAsync();
