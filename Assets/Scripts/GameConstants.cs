/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * GameConstants.cs
 * Holds constant values
 */

public static class GameConstants
{
	public const float DifficultyCoef = (2.0f/3.0f);
	public const int DifficultyStep = 1;
	public const int MinDifficulty = 1;
	public const int MaxDifficulty = 5;
	public const int MidDifficulty = (int)((MaxDifficulty + MinDifficulty) * 0.5f);
	public const int StartDifficulty = MidDifficulty;
	public const int DifficultyStepCount = (MaxDifficulty - MinDifficulty + DifficultyStep) / DifficultyStep;
	public const float JoystickDeadZoneCoef = 0.19f;
	public const int BaseScoreMultiplier = 10;
	public const float ScoreTextMinClamp = -330.0f; //TODO LATER big magic number here!
	public const float ScoreTextMaxClamp = 500.0f;
}