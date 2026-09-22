using System.Text;
using Chess.Core.Pieces;
using Events;
using Events.Events;

namespace Chess.Web.Services;

/// <summary>
/// The UI-facing view of the game. It does no chess logic: it only listens to
/// game events that the core domain emits, remembers the positions it has seen,
/// and exposes transient UI state such as which king is in check.
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
    private string _currentFen = Fen.StartPosition;
    private Colour? _colourInCheck;
    private bool _isMate;
    private PromotingEvent? _promotion;

    public BoardStateStore(IEventConsumer<IGameEvent> events, ILogger<BoardStateStore> logger)
    {
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// The current position as FEN. Defaults to the start position so the board
    /// renders something before the core has emitted its first update.
    /// </summary>
    public string CurrentFen
    {
        get { lock (_gate) return _currentFen; }
    }

    /// <summary>The colour reported by the latest check event, if any.</summary>
    public Colour? ColourInCheck
    {
        get { lock (_gate) return _colourInCheck; }
    }

    /// <summary>Atomically captures the board and its associated transient state.</summary>
    public (string Fen, Colour? ColourInCheck, bool IsMate, PromotingEvent? Promotion) Snapshot
    {
        get { lock (_gate) return (_currentFen, _colourInCheck, _isMate, _promotion); }
    }

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
                case NewGameEvent newGame:
                    lock (_gate)
                    {
                        _currentFen = newGame.Fen;
                        _colourInCheck = null;
                        _isMate = false;
                        _promotion = null;
                        _history.Clear();
                    }
                    Changed?.Invoke();
                    break;
                case BoardUpdateEvent update:
                    // The engine emits an empty update on startup as a "ready"
                    // ping; ignore FEN-less updates so rendering keeps its default.
                    if (string.IsNullOrWhiteSpace(update.BoardFenNotation))
                        break;
                    lock (_gate)
                    {
                        _currentFen = update.BoardFenNotation;
                        _colourInCheck = null;
                        _isMate = false;
                        _promotion = null;
                        _history.Add(update.BoardFenNotation);
                    }
                    Changed?.Invoke();
                    break;
                case CheckEvent check:
                    lock (_gate)
                    {
                        _colourInCheck = check.ColourInCheck;
                        _isMate = check.IsMate;
                    }
                    Changed?.Invoke();
                    break;
                case PromotingEvent promotion:
                    lock (_gate)
                    {
                        _promotion = promotion;
                    }
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
