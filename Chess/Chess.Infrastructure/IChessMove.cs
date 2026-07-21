
namespace Events;

public interface IChessMove
{
    string From { get; init; }
    string To { get; init; }
}