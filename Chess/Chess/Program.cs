using Chess.Core;
using Events;
using Events.Commands;
using Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register the event distributor as a singleton and expose it through both
// the producer and consumer interfaces (it implements both).
builder.Services.AddSingleton<EventDistributor<IGameEvent>>();
builder.Services.AddSingleton<EventDistributor<ICommand>>();
builder.Services.AddSingleton<IEventProducer<IGameEvent>>(
    sp => sp.GetRequiredService<EventDistributor<IGameEvent>>());
builder.Services.AddSingleton<IEventConsumer<ICommand>>(
    sp => sp.GetRequiredService<EventDistributor<ICommand>>());

builder.Services.AddSingleton<IMoveValidator, AllOkMoveValidator>();

// Register the board, which receives its dependencies via constructor injection.
builder.Services.AddSingleton<Board>();

using var host = builder.Build();

await host.StartAsync();

// Resolve the board from the container and run it until shutdown is requested.
var board = host.Services.GetRequiredService<Board>();
var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

await board.RunAsync(lifetime.ApplicationStopping);

await host.StopAsync();
