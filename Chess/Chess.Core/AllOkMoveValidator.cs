using Events.Commands;

namespace Chess.Core;

public class AllOkMoveValidator : IMoveValidator
{
    public bool Validate(MakeMoveCommand command)
    {
        return true;
    }
}