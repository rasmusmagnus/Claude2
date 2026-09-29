namespace Events.Events;

public readonly record struct GameEndedEvent(GameResult Result) : IGameEvent
{
}