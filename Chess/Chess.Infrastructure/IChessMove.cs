using System.Runtime.CompilerServices;

namespace Events;

public interface IChessMove
{
    Position From { get; init; }
    Position To { get; init; }
}

public record struct Position
{
    public readonly char File;
    public readonly int Rank;

    public Position(char file, int rank)
    {
        File = file;
        Rank = rank;
    }

    public Position(int fileIndex, int rankIndex)
    {
        File = (char)(fileIndex+96);
        Rank = rankIndex;
    }

    public int FileToIndex()
    {
        var file = File switch
        {
            'a' => 0,
            'b' => 1,
            'c' => 2,
            'd' => 3,
            'e' => 4,
            'f' => 5,
            'g' => 6,
            'h' => 7,
            _ => throw new Exception("Invalid file format")
        };
        return file + 1;
    }

    public static bool IsOutOfBounds(Position position)
    {
        try
        {
            return IsOutOfBounds(position.FileToIndex(), position.Rank);
        }
        catch
        {
            return true;
        }


    }

    public static bool IsOutOfBounds(int resultingFile, int resultingRank)
    {
        return resultingRank < 1 || resultingRank > 8 || resultingFile < 1 || resultingFile > 8;
    }
    
    public static void AddIfInBounds(int rankAsIndex, int fileAsIndex, HashSet<Position> hashSet)
    {
        if (!IsOutOfBounds(fileAsIndex, rankAsIndex))
        {
            hashSet.Add(new Position(fileAsIndex, rankAsIndex));
        }
    }
    
    public static (int, int) operator -(Position first, Position second)
    {
        return (first.FileToIndex() - second.FileToIndex(), first.Rank - second.Rank);
    }

    public static Position operator +(Position pos, (int, int) direction)
    {
        return new Position(pos.FileToIndex() + direction.Item1, pos.Rank + direction.Item2);
    }

    public override string ToString()
    {
        return File.ToString() + Rank.ToString(); 
    }
}