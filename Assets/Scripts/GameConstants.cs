/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * GameConstants.cs
 * Holds constant values
 */

public static class GameConstants
{
    //TODO LATER you might want to divide these constants to related classes to achieve better readable code
    public const float HorizontalMaxCoord = 9.0f;
    public const float HorizontalMinCoord = -2.0f;
    public const float HorizontalEarlyMaxCoord = 7.55f;
    public const float VerticalMaxCoord = 7.0f;
    public const float VerticalMinCoord = -2.0f;
    public const float MinHorizontalMovementLimit = -0.15f;
    public const float MaxHorizontalMovementLimit = 7.15f;
    public const float MinVerticalMovementLimit = 0.45f;
    public const float MaxVerticalMovementLimit = 5.15f;
    public const float MinDifficultyMultiplier = 0.33f;
    public const float MaxDifficultyMultiplier = 3.0f;
    public const float JoystickDeadZoneCoef = 0.19f;
    public const int StarToMeteorRatio = 10;
}