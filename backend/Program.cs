// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using SuperBackendNR85IA.Services;

var builder = WebApplication.CreateBuilder(args);

// Porta única para HTTP + WebSocket, igual às overlays

// Restringe o acesso apenas ao localhost
builder.WebHost.UseUrls("http://localhost:5221");

// DI ------------------------------------------------------------------------
builder.Services.AddSingleton<TelemetryBroadcaster>();
builder.Services.AddSingleton<CarTrackDataStore>();
builder.Services.AddSingleton<SessionYamlParser>();

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
        var overlay = context.Request.Query["overlay"].ToString();
        await handler.AddClient(webSocket, overlay, context.RequestAborted); // passa o CancellationToken para permitir cancelamento gracioso
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Mapeia os controllers para REST futuro (se necessário)
app.MapControllers();

await app.RunAsync();
