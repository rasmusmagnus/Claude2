using Events.Commands;

namespace Chess.Core;

public interface IMoveValidator
{
    bool Validate(MakeMoveCommand command);
}