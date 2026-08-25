namespace Events;

public interface IChessMove
{
    Position From { get; init; }
    Position To { get; init; }
}

public readonly record struct Position
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

    public static bool IsOutOfBounds(int resultingRank, int resultingFile)
    {
        return resultingRank < 1 || resultingRank > 8 || resultingFile < 1 || resultingFile > 8;
    }
    
    public static void AddIfInBounds(int rankAsIndex, int fileAsIndex, HashSet<Position> hashSet)
    {
        if (!IsOutOfBounds(rankAsIndex, fileAsIndex))
        {
            hashSet.Add(new Position(fileAsIndex, rankAsIndex));
        }
    }
    
}