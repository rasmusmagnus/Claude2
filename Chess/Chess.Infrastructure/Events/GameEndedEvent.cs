namespace Events.Events;

public struct GameEndedEvent(GameResult result) : IGameEvent
{
}