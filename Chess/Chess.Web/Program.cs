using System.Diagnostics;
using Chess.Core;
using Chess.Web.Components;
using Chess.Web.Services;
using Events;
using Events.Commands;
using Events.Events;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Event system -------------------------------------------------------
// Two independent buses give a clean, one-directional flow:
//   UI    --ICommand-->    core domain   (the command bus)
//   core  --IGameEvent-->  UI            (the event bus)
// The UI ONLY produces commands (moves) and consumes events (to render/export).
// Applying a move and emitting BoardUpdateEvent is the core domain's job.
// Each EventDistributor is a single object exposed through both its producer
// and consumer faces, exactly as the existing console host wires things up.
builder.Services.AddSingleton<EventDistributor<ICommand>>();
builder.Services.AddSingleton<IEventProducer<ICommand>>(
    sp => sp.GetRequiredService<EventDistributor<ICommand>>());
builder.Services.AddSingleton<IEventConsumer<ICommand>>(
    sp => sp.GetRequiredService<EventDistributor<ICommand>>());

builder.Services.AddSingleton<EventDistributor<IGameEvent>>();
builder.Services.AddSingleton<IEventProducer<IGameEvent>>(
    sp => sp.GetRequiredService<EventDistributor<IGameEvent>>());
builder.Services.AddSingleton<IEventConsumer<IGameEvent>>(
    sp => sp.GetRequiredService<EventDistributor<IGameEvent>>());

// The core domain engine. It owns all game logic: it consumes the commands the
// UI submits, applies moves, and emits BoardUpdateEvents. Move legality is the
// validator's job (today AllOkMoveValidator accepts everything).
builder.Services.AddSingleton<IMoveValidator, AllOkMoveValidator>();
builder.Services.AddSingleton<Board>();
builder.Services.AddHostedService<BoardEngineService>();

// The UI view-model: consumes events, tracks state for rendering/export,
// multicasts changes to circuits. It never mutates the board.
builder.Services.AddSingleton<BoardStateStore>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BoardStateStore>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// In development, pop the board open in the user's default browser as soon as
// the server is listening — so "just run the app" shows the board with no extra
// step, regardless of whether it was started via `dotnet run`, `dotnet watch`,
// or an IDE. Set CHESS_NO_BROWSER=1 to opt out.
if (app.Environment.IsDevelopment() &&
    Environment.GetEnvironmentVariable("CHESS_NO_BROWSER") != "1")
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();

        if (!string.IsNullOrEmpty(url))
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { /* best-effort: ignore if no browser is available */ }
        }
    });
}

app.Run();
