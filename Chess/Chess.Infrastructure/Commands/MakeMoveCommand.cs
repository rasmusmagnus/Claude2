namespace Events.Commands;

/// <summary>
/// A request to play a move. The command layer is intentionally dumb: it states
/// the intent only. Whether the move is legal is decided downstream by the engine
/// (and, in the future, the core domain rules), which either produces a
/// <see cref="Events.BoardUpdateEvent"/> or rejects it.
/// </summary>
public readonly record struct MakeMoveCommand(IChessMove Move) : ICommand;
