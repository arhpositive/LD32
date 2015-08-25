/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * GameConstants.cs
 * Holds constant values
 */

namespace Assets.Scripts
{
    public static class GameConstants
    {
        public const float HorizontalMaxCoord = 9.0f;
        public const float HorizontalMinCoord = -2.0f;
        public const float VerticalMaxCoord = 7.0f;
        public const float VerticalMinCoord = -2.0f;
        public const float MinHorizontalMovementLimit = -0.15f;
        public const float MaxHorizontalMovementLimit = 3.25f;
        public const float MinVerticalMovementLimit = 0.45f;
        public const float MaxVerticalMovementLimit = 5.15f;
	    public const int PlayerInitialHealth = 3;
        public const float WaveSpawnBaseInterval = 5.0f;
        public const float PowerupSpawnBaseInterval = 3.0f;
        public const float MinDifficultyMultiplier = 0.1f;
        public const float MaxDifficultyMultiplier = 2.0f;
    }
}
