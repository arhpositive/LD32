/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * TutorialItem.cs
 * Properties of an item in the tutorial, such as timer before popup, timer before initialization, etc.
 */

public class TutorialItem
{
	public SpawnType SpawnedItemType { get; private set; }
	public float TimeBeforeInit { get; private set; }
	public float TimeBeforePopupAndPause { get; private set; }

	public TutorialItem(SpawnType spawnedItemType, float timeBeforeInit, float timeBeforePopupAndPause)
	{
		SpawnedItemType = spawnedItemType;
		TimeBeforeInit = timeBeforeInit;
		TimeBeforePopupAndPause = timeBeforePopupAndPause;
	}
}

public class TutorialWaveItem : TutorialItem
{
	public int EnemyCountInWave { get; private set; }
	public int EnemyTypeIndex { get; private set; }

	public TutorialWaveItem(SpawnType spawnedItemType, float timeBeforeInit, float timeBeforePopupAndPause, int enemyCountInWave, int enemyTypeIndex) : base(spawnedItemType, timeBeforeInit, timeBeforePopupAndPause)
	{
		EnemyCountInWave = enemyCountInWave;
		EnemyTypeIndex = enemyTypeIndex;
	}
}

public class TutorialPowerupItem : TutorialItem
{
	public PowerupType TypeOfPowerup { get; private set; }

	public TutorialPowerupItem(SpawnType spawnedItemType, float timeBeforeInit, float timeBeforePopupAndPause, PowerupType typeOfPowerup) : base(spawnedItemType, timeBeforeInit, timeBeforePopupAndPause)
	{
		TypeOfPowerup = typeOfPowerup;
	}
}
