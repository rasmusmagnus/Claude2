namespace Events.Events;

public readonly record struct ViablePieceMovesEvent(Position[]? Positions) : IGameEvent
{
    
}