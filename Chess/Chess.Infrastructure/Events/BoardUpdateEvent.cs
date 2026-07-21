namespace Events.Events;

/// <summary>
/// Emitted whenever the authoritative board position changes. Carries the full
/// FEN so any consumer (the UI, a logger, future tests) can render or record the
/// position without holding a reference to the engine's internal state.
/// </summary>
public readonly record struct BoardUpdateEvent(string BoardFenNotation) : IGameEvent;
