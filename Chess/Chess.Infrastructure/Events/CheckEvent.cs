using Chess.Core.Pieces;

namespace Events.Events;

public readonly record struct CheckEvent(Colour ColourInCheck) : IGameEvent
{
    
}