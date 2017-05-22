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
