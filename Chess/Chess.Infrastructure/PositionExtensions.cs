namespace Events;

public static class PositionExtensions
{
    public static Position ToPosition(this string positionString)
    {
        if (positionString.Length != 2)
            throw new Exception("Unable to cast string to position");
        
        return new Position(positionString[0], int.Parse(positionString[1].ToString()));
    }
}