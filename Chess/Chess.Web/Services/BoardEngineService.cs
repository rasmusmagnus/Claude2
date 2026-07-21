using Chess.Core;
using Microsoft.Extensions.Hosting;

namespace Chess.Web.Services;

/// <summary>
/// Hosts the core <see cref="Board"/> engine inside the web process. It simply
/// runs the domain's command loop for the lifetime of the app: the board
/// consumes <c>ICommand</c>s (the moves the UI submits) and emits
/// <c>BoardUpdateEvent</c>s that the UI renders. All the actual game logic lives
/// in the core domain — this class only keeps it running.
/// </summary>
public sealed class BoardEngineService : BackgroundService
{
    private readonly Board _board;

    public BoardEngineService(Board board)
    {
        _board = board;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _board.RunAsync(stoppingToken);
}
