using Chess.Core.Pieces;

namespace Events.Events;

public readonly record struct PromotingEvent(Colour Colour, Position Pos) : IGameEvent;