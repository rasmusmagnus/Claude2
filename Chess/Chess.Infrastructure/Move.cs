namespace Events;

/// <summary>
/// A concrete <see cref="IChessMove"/> expressed as an origin and destination
/// square in coordinate notation (e.g. "e2" -&gt; "e4"). This is what the UI
/// produces from a drag-and-drop gesture. The core domain is free to introduce
/// richer move types later; consumers only need <see cref="IChessMove"/>.
/// </summary>
public sealed record Move(string From, string To) : IChessMove
{
}
