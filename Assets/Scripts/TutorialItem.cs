/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * TutorialItem.cs
 * Properties of an item in the tutorial, such as timer before popup, timer before initialization, etc.
 */

using System;
using UnityEngine.Assertions;

public enum TutorialType
{
    TtNone,
    TtWave,
    TtPowerup,
    TtHugeEnemy,
    TtActivateGun,
    TtActivateMovement,
    TtCount
}

public class TutorialItem
{
	public TutorialType TutorialType { get; private set; }
	public float TimeToWaitAfterEnd { get; private set; }
	public string StandardPopupText { get; private set; }
    public string AlternativePopupText { get; private set; }
    public float TimeBeforePopupAndPause { get; private set; }
    public string TriggerPopupText { get; private set; }
    public bool EventTriggered { get; private set; }

    private Func<bool> TriggerConditionFunc;

    public TutorialItem(TutorialType tutorialType, float timeToWaitAfterEnd, string standardPopupText = "", 
        float timeBeforePopupAndPause = 0.0f, string triggerPopupText = "", Func<bool> triggerConditionFunc = null, string alternativePopupText = "")
	{
		TutorialType = tutorialType;
		TimeToWaitAfterEnd = timeToWaitAfterEnd;
        StandardPopupText = standardPopupText;
		TimeBeforePopupAndPause = timeBeforePopupAndPause;
	    TriggerPopupText = triggerPopupText;
	    TriggerConditionFunc = triggerConditionFunc;
	    EventTriggered = TriggerConditionFunc == null || TriggerConditionFunc();
	    AlternativePopupText = alternativePopupText;
    }

    public bool CheckTrigger()
    {
        Assert.IsFalse(EventTriggered);
        if (TriggerConditionFunc == null || TriggerConditionFunc())
        {
            EventTriggered = true;
        }
        return EventTriggered;
    }
}

public class TutorialWaveItem : TutorialItem
{
	public int EnemyCountInWave { get; private set; }
	public int EnemyTypeIndex { get; private set; }

	public TutorialWaveItem(int enemyCountInWave, int enemyTypeIndex, float timeToWaitAfterEnd, string popupInfoText = "",
        float timeBeforeStandardPopupAndPause = 0.0f, string triggerPopupText = "", Func<bool> triggerConditionFunc = null, string popupInfoAltText = "") 
        : base(TutorialType.TtWave, timeToWaitAfterEnd, popupInfoText, timeBeforeStandardPopupAndPause, triggerPopupText, triggerConditionFunc, popupInfoAltText)
	{
		EnemyCountInWave = enemyCountInWave;
		EnemyTypeIndex = enemyTypeIndex;
	}
}

public class TutorialPowerupItem : TutorialItem
{
	public PowerupType TypeOfPowerup { get; private set; }

	public TutorialPowerupItem(PowerupType typeOfPowerup, float timeToWaitAfterEnd, string popupInfoText = "",
        float timeBeforeStandardPopupAndPause = 0.0f, string triggerPopupText = "", Func<bool> triggerConditionFunc = null, string popupInfoAltText = "") 
		: base(TutorialType.TtPowerup, timeToWaitAfterEnd, popupInfoText, timeBeforeStandardPopupAndPause, triggerPopupText, triggerConditionFunc, popupInfoAltText)
	{
		TypeOfPowerup = typeOfPowerup;
	}
}

public class TutorialActivateGunItem : TutorialItem
{
	public GunType TypeOfGun { get; private set; }

	public TutorialActivateGunItem(GunType typeOfGun, float timeToWaitAfterEnd, string popupInfoText = "",
        float timeBeforeStandardPopupAndPause = 0.0f, string triggerPopupText = "", Func<bool> triggerConditionFunc = null, string popupInfoAltText = "") 
        : base(TutorialType.TtActivateGun, timeToWaitAfterEnd, popupInfoText, timeBeforeStandardPopupAndPause, triggerPopupText, triggerConditionFunc, popupInfoAltText)
	{
		TypeOfGun = typeOfGun;
	}
}
