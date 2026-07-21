using System.Text;
using Events;
using Events.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chess.Web.Services;

/// <summary>
/// The UI-facing view of the game. It does no chess logic: it only listens to
/// <see cref="BoardUpdateEvent"/>s that the core domain emits and remembers the
/// positions it has seen so the board can render and so the state can be
/// exported ("flushed") for building unit tests.
///
/// A single background reader drains the event bus (a channel hands each message
/// to exactly one reader, so this is the one place that reads it) and fans the
/// result out to every connected Blazor circuit via <see cref="Changed"/>.
/// </summary>
public sealed class BoardStateStore : BackgroundService
{
    private readonly IEventConsumer<IGameEvent> _events;
    private readonly ILogger<BoardStateStore> _logger;
    private readonly object _gate = new();
    private readonly List<string> _history = new();

    public BoardStateStore(IEventConsumer<IGameEvent> events, ILogger<BoardStateStore> logger)
    {
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// The current position as FEN. Defaults to the start position so the board
    /// renders something before the core has emitted its first update.
    /// </summary>
    public string CurrentFen { get; private set; } = Fen.StartPosition;

    /// <summary>Snapshot of the FENs observed so far (for the debug/export panel).</summary>
    public IReadOnlyList<string> History
    {
        get { lock (_gate) return _history.ToArray(); }
    }

    /// <summary>Raised (off the UI thread) whenever the observed position changes.</summary>
    public event Action? Changed;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _events.GetReader();
        await foreach (var evt in reader.ReadAllAsync(stoppingToken))
        {
            switch (evt)
            {
                case BoardUpdateEvent update:
                    CurrentFen = update.BoardFenNotation;
                    lock (_gate) _history.Add(update.BoardFenNotation);
                    Changed?.Invoke();
                    break;
            }
        }
    }

    /// <summary>
    /// "Flushes" the captured board history to a copyable form: it writes the
    /// export to the server console and returns the same text for display in the
    /// UI. The output is formatted as xUnit <c>[InlineData]</c> rows so a
    /// position sequence can be pasted straight into a test.
    /// </summary>
    public string Flush()
    {
        var fens = History;
        if (fens.Count == 0)
            fens = new[] { CurrentFen };

        var sb = new StringBuilder();
        sb.AppendLine($"// {fens.Count} board state(s) captured");
        foreach (var fen in fens)
            sb.AppendLine($"[InlineData(\"{fen}\")]");

        var export = sb.ToString().TrimEnd();

        // "writing it in some console" — the server terminal.
        _logger.LogInformation("Board state flush:\n{Export}", export);

        return export;
    }
}
