namespace Events.Commands;

/// <summary>Starts a new game from a complete Forsyth-Edwards Notation position.</summary>
public readonly record struct StartFromFenCommand(string Fen) : ICommand;

