/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * TutorialItem.cs
 * Properties of an item in the tutorial, such as timer before popup, timer before initialization, etc.
 */

public class TutorialItem
{
	public TutorialType TutorialType { get; private set; }
	public float TimeToWaitAfterEnd { get; private set; }
	public float TimeBeforePopupAndPause { get; private set; }
	public string PopupInfoText { get; private set; }

	public TutorialItem(TutorialType tutorialType, float timeToWaitAfterEnd, string popupInfoText = "", float timeBeforePopupAndPause = 0.0f)
	{
		TutorialType = tutorialType;
		TimeToWaitAfterEnd = timeToWaitAfterEnd;
		PopupInfoText = popupInfoText;
		TimeBeforePopupAndPause = timeBeforePopupAndPause;
	}
}

public class TutorialWaveItem : TutorialItem
{
	public int EnemyCountInWave { get; private set; }
	public int EnemyTypeIndex { get; private set; }

	public TutorialWaveItem(int enemyCountInWave, int enemyTypeIndex, float timeToWaitAfterEnd, string popupInfoText = "", float timeBeforePopupAndPause = 0.0f) 
		: base(TutorialType.TtWave, timeToWaitAfterEnd, popupInfoText, timeBeforePopupAndPause)
	{
		EnemyCountInWave = enemyCountInWave;
		EnemyTypeIndex = enemyTypeIndex;
	}
}

public class TutorialPowerupItem : TutorialItem
{
	public PowerupType TypeOfPowerup { get; private set; }

	public TutorialPowerupItem(PowerupType typeOfPowerup, float timeToWaitAfterEnd, string popupInfoText = "", float timeBeforePopupAndPause = 0.0f) 
		: base(TutorialType.TtPowerup, timeToWaitAfterEnd, popupInfoText, timeBeforePopupAndPause)
	{
		TypeOfPowerup = typeOfPowerup;
	}
}

public class TutorialActivateGunItem : TutorialItem
{
	public GunType TypeOfGun { get; private set; }

	public TutorialActivateGunItem(GunType typeOfGun, float timeToWaitAfterEnd, string popupInfoText = "", float timeBeforePopupAndPause = 0.0f) : 
		base(TutorialType.TtActivateGun, timeToWaitAfterEnd, popupInfoText, timeBeforePopupAndPause)
	{
		TypeOfGun = typeOfGun;
	}
}
