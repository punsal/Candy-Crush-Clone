namespace Gameplay.Systems.MatchDetection.Type
{
    /// <summary>
    /// Defines the type of a matchable tile in the game. This enumeration is used
    /// to specify the color or category of a tile, which is utilized in match-detection
    /// systems to determine if tiles can form a valid match.
    /// </summary>
    public enum MatchType
    {
        Any,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple
    }
}