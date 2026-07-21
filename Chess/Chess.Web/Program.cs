using Chess.Web.Components;
using Chess.Web.Services;
using Events;
using Events.Commands;
using Events.Events;

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

app.Run();
