namespace Events.Commands;

public readonly record struct GetViablePieceMovesCommand(Position PiecePosition) : ICommand
{
    
}