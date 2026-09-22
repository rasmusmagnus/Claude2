namespace Events.Events;

public readonly record struct NewGameEvent(string Fen) : IGameEvent
{
    
}