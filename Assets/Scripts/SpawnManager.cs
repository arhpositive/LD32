/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using System;
using System.Collections.Generic;
using ui;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public struct WaveEntity
{
	public Vector2 Position;
	public Vector2 MoveDir;

	public WaveEntity(Vector2 position, Vector2 moveDir)
	{
		Position = position;
		MoveDir = moveDir;
	}
};

public class Formation
{
	public List<WaveEntity> WaveEntities { get; private set; }
	public int HorizontalShipSpan { get; private set; }

	public Formation(List<WaveEntity> waveEntities, int horizontalShipSpan)
	{
		WaveEntities = waveEntities;
		HorizontalShipSpan = horizontalShipSpan;
	}
}

public class SpawnManager : MonoBehaviour
{
    //debug bounds
	public GameObject UpBound;
	public GameObject DownBound;

	public GameObject CanvasGameObject;
	public GameObject CanvasScorePanel;
    public GameObject ScoreLeftAnchor;
    public GameObject ScoreRightAnchor;
	public GameObject HelpPopupPanel;
    public GameObject PausePopupPanel;

	[Header("Prefabs")]
	public GameObject PlayerPrefab;
	public GameObject[] EnemyPrefabArray;
	public GameObject[] HugeEnemyPrefabArray;
	public GameObject[] PosPowerupPrefabArray;
	public GameObject[] NegPowerupPrefabArray;
	public GameObject ShipConnectionPrefab;
	public GameObject WaveScoreIndicatorPrefab;

	[Header("Interval Properties")]
	public float MinWaveSpawnIntervalCoef;
	public float MaxWaveSpawnIntervalCoef;
	public float MinHugeEnemySpawnIntervalCoef;
	public float MaxHugeEnemySpawnIntervalCoef;
	public float PowerupSpawnBaseInterval;

	private DifficultyManager _difficultyManagerScript;

	private float _previousWaveSpawnTime;
	private float _waveSpawnInterval;
	private float _previousHugeEnemySpawnTime;
	private float _hugeEnemySpawnInterval;

	private float _previousPosPowerupSpawnTime;
	private float _posPowerupSpawnInterval;
	private float _previousNegPowerupSpawnTime;
	private float _negPowerupSpawnInterval;

	private List<Formation> _formations;
	private List<EnemyWave> _enemyWaves;
	private Player _playerScript;
	private RectTransform _canvasRectTransform;

	private const float ShipColliderVertSize = 0.46f;
	private const float ShipGameObjectVertSize = 0.5f;
	private const float PlayerShipColliderHorzSize = 0.38f;

	private float _enemySpawnMinVertDist;
	private float _enemySpawnMaxVertDist = ShipColliderVertSize * 2.0f - 0.01f;
	private float _enemySpawnMinHorzDist;
	private float _enemySpawnMaxHorzDist = PlayerShipColliderHorzSize * 2.0f - 0.01f;

	private float _vertMinShipSpawnCoord;
	private float _vertMaxShipSpawnCoord;

	private bool _hugeEnemyExists;

    //UI Properties
    private float _scoreLeftAnchorXPos;
    private float _scoreRightAnchorXPos;

	//Tutorial Properties
	private bool _tutorialSequenceIsActive;
	private float _tutorialSequenceLastEventTime;
	private float _tutorialSequenceEventInterval;
	private bool _popupEventUpcoming;
	private List<TutorialItem> _tutorialSequenceItems;
	private Text _pausePopupText;

    private static void PauseGame()
    {
        Time.timeScale = 0.0f;
        print("Game paused.");
    }

    private static void UnpauseGame()
    {
        Time.timeScale = 1.0f;
        print("Game unpaused.");
    }

    public static bool IsGamePaused()
    {
        return Mathf.Approximately(Time.timeScale, 0.0f);
    }

    private static void SpeedUpGame()
    {
        Time.timeScale += 0.1f;
        print("Timescale Up: " + Time.timeScale);
    }

    private static void SlowDownGame()
    {
        Time.timeScale -= 0.1f;
        print("Timescale Down: " + Time.timeScale);
    }

	private void Awake()
	{
		//instantiate player
		GameObject playerGameObject = Instantiate(PlayerPrefab,
			new Vector2(0.0f, Random.Range(Player.MinVerticalMovementLimit, Player.MaxVerticalMovementLimit)),
			Quaternion.identity);
		_playerScript = playerGameObject.GetComponent<Player>();
	}

	private void Start()
	{
        UnpauseGame();

		_difficultyManagerScript = GetComponent<DifficultyManager>();
		
		_enemySpawnMinVertDist = Mathf.Min(ShipGameObjectVertSize + 0.05f, _enemySpawnMaxVertDist);
		_enemySpawnMinHorzDist = Mathf.Min(PlayerShipColliderHorzSize * 0.5f, _enemySpawnMaxHorzDist);

		_hugeEnemyExists = false;
		ResetVerticalSpawnLimits();

		_waveSpawnInterval = MinWaveSpawnIntervalCoef;
		_previousWaveSpawnTime = Time.time;
		_hugeEnemySpawnInterval = MinHugeEnemySpawnIntervalCoef;
		_previousHugeEnemySpawnTime = Time.time;
		_formations = new List<Formation>();
		_enemyWaves = new List<EnemyWave>();

	    _scoreLeftAnchorXPos = ScoreLeftAnchor.GetComponent<RectTransform>().anchoredPosition.x;
	    _scoreRightAnchorXPos = ScoreRightAnchor.GetComponent<RectTransform>().anchoredPosition.x;

        if (CanvasGameObject)
		{
			_canvasRectTransform = CanvasGameObject.GetComponent<RectTransform>();
		}

		if (HelpPopupPanel)
		{
		    var textComponents = HelpPopupPanel.GetComponentsInChildren<Text>();
		    foreach (var curText in textComponents)
		    {
		        if (curText.CompareTag("InfoText"))
		        {
		            _pausePopupText = HelpPopupPanel.GetComponentInChildren<Text>();
		            break;
		        }
		    }
		}

		ChangeTutorialSequenceState(LoadLevel.TutorialToggleValue);
		_popupEventUpcoming = false;
		if (_tutorialSequenceIsActive)
		{
		    _playerScript.BeginTutorial();
            FillTutorialSequence();
		}

		PregeneratePossibleWaves();

		_posPowerupSpawnInterval = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
		_previousPosPowerupSpawnTime = Time.time;

		_negPowerupSpawnInterval = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
		_previousNegPowerupSpawnTime = Time.time;
	}

	private void Update()
	{
		UpdateDebugStuff();

		if (_tutorialSequenceIsActive)
		{
			//1. if tutorial sequence timer is up
			//2. spawn new tutorial element
			//3. get next element and set next timer

			if (IsGamePaused())
			{
                if (Input.GetButtonDown("TogglePause") && HelpPopupPanel.activeSelf)
				{
					ResumeGameAfterPopup(true, HelpPopupPanel);
				    _tutorialSequenceEventInterval = _tutorialSequenceItems[0].TimeToWaitAfterEnd;
                    Input.ResetInputAxes(); //clear input so that it's not being used for this frame
                }
			}

			//if tutorial timer is up, start next tutorial event
		    if (Time.time - _tutorialSequenceLastEventTime > _tutorialSequenceEventInterval)
		    {
		        if (_popupEventUpcoming)
		        {
                    print("Event upcoming!");
		            _tutorialSequenceLastEventTime = Time.time;
                    PopupAndPause(HelpPopupPanel, _pausePopupText, SelectTutorialTextToDisplay());
		            _tutorialSequenceEventInterval = _tutorialSequenceItems[0].TimeToWaitAfterEnd;
                }
		        else
		        {
                    print("Switch to next!");
		            CheckTutorialEventAccomplishment();
		        }
		    }
		    else if (_tutorialSequenceItems.Count > 0 && !_tutorialSequenceItems[0].EventTriggered)
		    {
		        //check if event is triggered
		        if (_tutorialSequenceItems[0].CheckTrigger())
		        {
                    PopupAndPause(HelpPopupPanel, _pausePopupText, _tutorialSequenceItems[0].TriggerPopupText);
		            print("Voila!");
                }
		    }
		}
		else
		{
            if (Time.time - _previousWaveSpawnTime > _waveSpawnInterval)
			{
				SpawnNewWave();
				_previousWaveSpawnTime = Time.time;
			}

			if (Time.time - _previousHugeEnemySpawnTime > _hugeEnemySpawnInterval)
			{
				SpawnNewHugeEnemy();
				_previousHugeEnemySpawnTime = Time.time;
			}

			if (Time.time - _previousPosPowerupSpawnTime > _posPowerupSpawnInterval)
			{
				SpawnNewPowerup(true);
				_previousPosPowerupSpawnTime = Time.time;
			}

			if (Time.time - _previousNegPowerupSpawnTime > _negPowerupSpawnInterval)
			{
				SpawnNewPowerup(false);
				_previousNegPowerupSpawnTime = Time.time;
			}
		}

	    if (Input.GetButtonDown("TogglePause")) //input is previously cleared if the tutorial menu was active
	    {
	        if (IsGamePaused())
	        {
	            if (PausePopupPanel.activeSelf)
	            {
	                ResumeGameAfterPopup(true, PausePopupPanel);
                }
	        }
	        else
	        {
	            PopupAndPause(PausePopupPanel);
	        }
	    }

        //update each enemy wave and remove waves which have no remaining enemies
	    float previousWavexPos = float.MinValue;
	    foreach (EnemyWave currentWave in _enemyWaves)
	    {
	        currentWave.Update(previousWavexPos);
	        previousWavexPos = currentWave.GetWaveScorexPos();
	    }
		_enemyWaves.RemoveAll(item => item.SetForDestruction);
	}

	private void UpdateDebugStuff()
	{
		if (UpBound)
		{
			UpBound.transform.position = new Vector3(UpBound.transform.position.x, _vertMaxShipSpawnCoord, UpBound.transform.position.z);
		}
		if (DownBound)
		{
			DownBound.transform.position = new Vector3(DownBound.transform.position.x, _vertMinShipSpawnCoord, DownBound.transform.position.z);
		}
        
		if (Input.GetKeyDown(KeyCode.U))
		{
			SpeedUpGame();
		}
		else if (Input.GetKeyDown(KeyCode.J))
		{
			SlowDownGame();
		}
	}

	private void PregeneratePossibleWaves()
	{
        //TODO NEXT devise a boss fight of some kind, stopping movement of player, encountering boss and then moving on

		// TODO LATER include different movement patterns, might involve waypoints, etc.
		// waypoint system could make the wave change movement direction after a given amount of time.
		// be careful about randomizing too much as it will make us lose control over certain difficulty features

		List<WaveEntity> straightLine = new List<WaveEntity>
		{
			//  5
			//  4
			//  3
			//  2
			//  1
			//  0
			new WaveEntity(Vector2.zero, Vector2.left), new WaveEntity(new Vector2(0, 1), Vector2.left), new WaveEntity(new Vector2(0, 2), Vector2.left), new WaveEntity(new Vector2(0, 3), Vector2.left), new WaveEntity(new Vector2(0, 4), Vector2.left), new WaveEntity(new Vector2(0, 5), Vector2.left), new WaveEntity(new Vector2(0, 6), Vector2.left), new WaveEntity(new Vector2(0, 7), Vector2.left), new WaveEntity(new Vector2(0, 8), Vector2.left), new WaveEntity(new Vector2(0, 9), Vector2.left)
		};
		_formations.Add(new Formation(straightLine, 0));

		List<WaveEntity> echelonLine = new List<WaveEntity>()
		{
			//      5
			//  4
			//      3
			//  2
			//      1
			//  0
			new WaveEntity(Vector2.zero, Vector2.left), new WaveEntity(new Vector2(1, 1), Vector2.left), new WaveEntity(new Vector2(0, 2), Vector2.left), new WaveEntity(new Vector2(1, 3), Vector2.left), new WaveEntity(new Vector2(0, 4), Vector2.left), new WaveEntity(new Vector2(1, 5), Vector2.left), new WaveEntity(new Vector2(0, 6), Vector2.left), new WaveEntity(new Vector2(1, 7), Vector2.left), new WaveEntity(new Vector2(0, 8), Vector2.left), new WaveEntity(new Vector2(1, 9), Vector2.left)
		};
		_formations.Add(new Formation(echelonLine, 1));

		List<WaveEntity> forwardsWedge = new List<WaveEntity>
		{
			//          1
			//      3
			//  5
			//  4
			//      2
			//          0
			new WaveEntity(new Vector2(4, 0), Vector2.left), new WaveEntity(new Vector2(4, 9), Vector2.left), new WaveEntity(new Vector2(3, 1), Vector2.left), new WaveEntity(new Vector2(3, 8), Vector2.left), new WaveEntity(new Vector2(2, 2), Vector2.left), new WaveEntity(new Vector2(2, 7), Vector2.left), new WaveEntity(new Vector2(1, 3), Vector2.left), new WaveEntity(new Vector2(1, 6), Vector2.left), new WaveEntity(new Vector2(0, 4), Vector2.left), new WaveEntity(new Vector2(0, 5), Vector2.left)
		};
		_formations.Add(new Formation(forwardsWedge, 4));

		List<WaveEntity> backwardsWedge = new List<WaveEntity>
		{
			//  1
			//      3
			//          5
			//          4
			//      2
			//  0
			new WaveEntity(Vector2.zero, Vector2.left), new WaveEntity(new Vector2(0, 9), Vector2.left), new WaveEntity(new Vector2(1, 1), Vector2.left), new WaveEntity(new Vector2(1, 8), Vector2.left), new WaveEntity(new Vector2(2, 2), Vector2.left), new WaveEntity(new Vector2(2, 7), Vector2.left), new WaveEntity(new Vector2(3, 3), Vector2.left), new WaveEntity(new Vector2(3, 6), Vector2.left), new WaveEntity(new Vector2(4, 4), Vector2.left), new WaveEntity(new Vector2(4, 5), Vector2.left)
		};
		_formations.Add(new Formation(backwardsWedge, 4));
	}

    private bool FirstWaveIsDisrupted()
    {
        return _enemyWaves.Count > 0 && _enemyWaves[0].EnemyDisplacementChanged;
    }

    private bool SpeedUpGunHasAmmo()
    {
        return _playerScript.GetGun(GunType.GtSpeedUp).CurrentAmmoCount > 0;
    }

    private bool TeleportGunHasAmmo()
    {
        return _playerScript.GetGun(GunType.GtTeleport).CurrentAmmoCount > 0;
    }

    private bool TeleportIsPossible()
    {
        return _playerScript.GetGun(GunType.GtTeleport).LastBullet != null;
    }

    private bool TeleportSucceeded()
    {
        return _playerScript.TeleportedWithLastTrigger;
    }

    private bool EnemyHitWithSpeedUp()
    {
        return _enemyWaves.Count > 0 && _enemyWaves[0].HasSpedUpEnemy();
    }

    private bool PlayerHasShield()
    {
        return _playerScript.IsShielded;
    }

    private bool PlayerIncreasedHealth()
    {
        return _playerScript.PlayerHealth > Player.PlayerInitialHealth;
    }

    private bool PlayerCollectedResearch()
    {
        return _playerScript.TotalResearchPickedUp > 0;
    }

    public void TutorialOnPlayerDeath()
    {
        PopupAndPause(HelpPopupPanel, _pausePopupText, "Whoops! You got hit. A few more hits and it might mean game over. Try not to hit enemies, bullets or bombs.");
    }

    private string SelectTutorialTextToDisplay()
    {
        return CheckActiveControlModel.CurrentControlState == CheckActiveControlModel.ControlModel.CmKeyboard ||
            _tutorialSequenceItems[0].AlternativePopupText == ""
                ? _tutorialSequenceItems[0].StandardPopupText
                : _tutorialSequenceItems[0].AlternativePopupText;
    }

    private void FillTutorialSequence()
    {
        const float powerupTimer = 10.0f;
        const float waveTimer = 10.0f;

        //TODO LATER key help text is hardcoded, whereas making it modular would make changing key configurations possible

		_tutorialSequenceItems = new List<TutorialItem>
		{
			new TutorialItem(TutorialType.TtNone, 3.0f),
			new TutorialItem(TutorialType.TtNone, 3.0f, "Welcome to Dislocator Tutorial."),

			new TutorialItem(TutorialType.TtActivateMovement, 3.0f, "You can move your spaceship up or down with arrow keys.", 0f, "", null, 
                "You can move your spaceship up or down with directional pad."),
		    new TutorialWaveItem(2, 0, waveTimer, "This is a basic wave of enemies. Avoid colliding with them.", 1.5f),
		    new TutorialWaveItem(4, 1, waveTimer, "This is a more advanced wave. Be careful, these ships can shoot bullets.", 1.5f),

		    new TutorialPowerupItem(PowerupType.PtHealth, powerupTimer, "This is a health power-up to give you an additional life. Pick it up.", 2f,
		        "Great! Try to collect as much health power-ups as you can.", PlayerIncreasedHealth),

            new TutorialActivateGunItem(GunType.GtStun, 5.0f, "You can fire your Stun Gun with Z key.", 0f, "", null, "You can fire your Stun Gun with (A) button."),
			new TutorialWaveItem(3, 1, waveTimer, "Try your Stun Gun on these enemies. It will stop their engines for a short time.", 1.5f,
			    "Good shot! Enemies shot by a stun bullet won't be able to shoot bullets for a while.", FirstWaveIsDisrupted),

            new TutorialPowerupItem(PowerupType.PtShield, powerupTimer, "This is a shield which can protect you from a hit. Pick it up.", 2f,
		        "Shield only protects you from a single hit coming to the front side.", PlayerHasShield),

            new TutorialPowerupItem(PowerupType.PtSpeedup, powerupTimer, "This is an ammo for your Speed-Up Gun. Pick it up by moving through it.", 2f, 
                "", SpeedUpGunHasAmmo),
		    new TutorialActivateGunItem(GunType.GtSpeedUp, 5.0f, "You can fire your Speed-Up Gun with X key.", 0f, "", null, "You can fire your Speed-Up Gun with (B) button."),

            new TutorialWaveItem(3, 1, waveTimer, "Try your Speed-Up Gun on these enemies. It will make them go much faster.", 1.5f,
		        "Nice! Notice that as the connection between enemy ships gets thinner, you get more points.", EnemyHitWithSpeedUp),

		    new TutorialPowerupItem(PowerupType.PtResearch, powerupTimer, "This is a research power-up which gives you additional points. Pick it up.", 2f,
		        "Nice work! Each research collected will result in more and more points.", PlayerCollectedResearch),

            new TutorialPowerupItem(PowerupType.PtBomb, powerupTimer, "This is a bomb and it should be avoided as it will explode on impact.", 2f),

            new TutorialItem(TutorialType.TtActivateMovement, 3.0f, "You can move your spaceship left or right with arrow keys.", 0f, "", null, 
                "You can move your spaceship left or right with directional pad."),
            
            new TutorialPowerupItem(PowerupType.PtTeleport, powerupTimer, "This is an ammo for your Teleport Gun. Pick it up.", 2f,
			    "", TeleportGunHasAmmo),
		    new TutorialActivateGunItem(GunType.GtTeleport, 3.0f, "You can fire your Teleport Gun with C key.", 0f, "", TeleportIsPossible, "You can fire your Teleport Gun with (X) button."),
            new TutorialItem(TutorialType.TtNone, 5.0f, "Pressing C if there's a teleport beacon on the scene will beam you to its location.", 0f, 
                "Good job! You can avoid enemies by teleporting.", TeleportSucceeded, "Pressing (X) if there's a teleport beacon on the scene will beam you to its location."),
            
            new TutorialWaveItem(7, 1, waveTimer, "Try to use your weapons to get through this wave of enemies without getting hit.", 1.5f),

            new TutorialItem(TutorialType.TtHugeEnemy, 10.0f, "This is a huge enemy ship that you can not dislocate. Avoid it and its bullets.", 10.0f),
			
			new TutorialItem(TutorialType.TtNone, 17.0f, "You now have every information necessary to start your first play-through. Good luck!", 1.0f)
		};

		SwitchToNextItemInTutorial();
	}

    private void SwitchToNextItemInTutorial()
	{
		if (_tutorialSequenceItems.Count > 0)
		{
			//prepare for next event
			print("Next event: " + _tutorialSequenceItems[0].TutorialType + " " + _tutorialSequenceItems[0].TimeToWaitAfterEnd + " " + _tutorialSequenceItems[0].TimeBeforePopupAndPause);

			_tutorialSequenceLastEventTime = Time.time;

			SpawnNextTutorialItem();

			float timeToPopup = _tutorialSequenceItems[0].TimeBeforePopupAndPause;
			_popupEventUpcoming = true;

			if (Mathf.Approximately(timeToPopup, 0.0f))
			{
				print("Popup now!");
                PopupAndPause(HelpPopupPanel, _pausePopupText, SelectTutorialTextToDisplay());
			    _tutorialSequenceEventInterval = _tutorialSequenceItems[0].TimeToWaitAfterEnd;
            }
			else
			{
				//switch to popup event
				print("Popup in " + timeToPopup + " seconds.");
				_tutorialSequenceEventInterval = timeToPopup;
			}
		}
		else
		{
			print("Tutorial is finished!");
			ChangeTutorialSequenceState(false);
		}
	}

	private void ChangeTutorialSequenceState(bool newState)
	{
		_tutorialSequenceIsActive = newState;
		_difficultyManagerScript.ChangeTutorialSequenceState(newState);

		if (!_tutorialSequenceIsActive)
		{
			//reset weapons, movement limits, etc.
			_playerScript.EndTutorial();

			//also, reset all timers in spawn manager
			_previousWaveSpawnTime = Time.time;
			_previousHugeEnemySpawnTime = Time.time;
			_previousPosPowerupSpawnTime = Time.time;
			_previousPosPowerupSpawnTime = Time.time;
		}
	}

	private void PopupAndPause(GameObject popupPanel, Text popupTextField = null, string popupText = "")
	{
        print("popup!");
		if (popupTextField && popupText == "")
		{
			ResumeGameAfterPopup(false, popupPanel);
        }
		else
		{
            PauseGame();
		    if (popupTextField)
		    {
		        popupTextField.text = popupText;
            }
		    popupPanel.SetActive(true);
		}
	}

	private void SpawnNextTutorialItem()
	{
		switch (_tutorialSequenceItems[0].TutorialType)
		{
			case TutorialType.TtNone:
				break;
			case TutorialType.TtWave:
				SpawnTutorialWave((TutorialWaveItem)_tutorialSequenceItems[0]); 
				break;
			case TutorialType.TtPowerup:
				SpawnTutorialPowerup((TutorialPowerupItem)_tutorialSequenceItems[0]);
				break;
			case TutorialType.TtHugeEnemy:
				SpawnTutorialHugeEnemy();
				break;
			case TutorialType.TtActivateMovement:
				_playerScript.ActivateMovement();
				break;
			case TutorialType.TtActivateGun:
				_playerScript.GetGun(((TutorialActivateGunItem)_tutorialSequenceItems[0]).TypeOfGun).SetCanBeFired(true);
				break;
			default:
				Assert.IsTrue(false);
				break;
		}
	}

	private void ResumeGameAfterPopup(bool popupTriggered, GameObject popupPanel)
	{
	    bool pausePopupWasActive = PausePopupPanel.activeSelf;

		if (popupTriggered)
		{
            UnpauseGame();
		    popupPanel.SetActive(false);
		}

	    if (_tutorialSequenceIsActive && !pausePopupWasActive)
	    {
	        _popupEventUpcoming = false;
        }
	}

	private void CheckTutorialEventAccomplishment()
	{
	    if (_tutorialSequenceItems[0].EventTriggered)
	    {
	        //remove current event that we've just finished
            _tutorialSequenceItems.RemoveAt(0);
	    }
        //if the current event is not yet removed (triggered succesfully) we'll repeat it
		SwitchToNextItemInTutorial();
	}
	
	private void SpawnTutorialWave(TutorialWaveItem tutorialWaveItem)
	{
		float minVerticalStartCoord = _vertMinShipSpawnCoord;
		float maxVerticalStartCoord = _vertMaxShipSpawnCoord - (tutorialWaveItem.EnemyCountInWave - 1) * _enemySpawnMaxVertDist;

		//V. Select Enemies From Formation List
	    List<WaveEntity> selectedFormationEntities = SelectEnemiesFromFormation(tutorialWaveItem.EnemyCountInWave);
        
        //ship type is the same for the tutorial
	    int[] shipTypes = new int[selectedFormationEntities.Count];
	    for (int i = 0; i < shipTypes.Length; ++i)
	    {
	        shipTypes[i] = tutorialWaveItem.EnemyTypeIndex;
	    }

	    //VII. Spawn Enemies
        SpawnEnemies(selectedFormationEntities, shipTypes, minVerticalStartCoord, maxVerticalStartCoord, _enemySpawnMaxHorzDist, _enemySpawnMaxVertDist, _enemySpawnMaxHorzDist);
	}

	//Generate new waves and spawn them on scene
	private void SpawnNewWave()
	{
		EventLogger.PrintToLog("New Wave Spawn");

		float randomIntervalCoef = Random.Range(MinWaveSpawnIntervalCoef, MaxWaveSpawnIntervalCoef);
		_waveSpawnInterval = randomIntervalCoef/_difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpWaveSpawnRateIncrease);


		int randRange = 100;
		float stepSize = (float) randRange/GameConstants.DifficultyStepCount;

		float noExitProbability = _difficultyManagerScript.DifficultyCoefs[DifficultyParameter.DpWaveHasNoExitCoef]*stepSize - stepSize*0.5f;

		bool hasNoExit = Random.Range(0, randRange) < noExitProbability;

		//TODO NEXT low difficulty = wider spread & less enemies, high difficulty = shorter spread & more enemies

		//I. Pick a random formation type
		Formation selectedFormation = _formations[Random.Range(0, _formations.Count)];

		//II. Determine Horizontal Distance Between Enemies
		float nextWaveHorizontalDistance = _waveSpawnInterval*BasicEnemy.MoveSpeed;
		float maxEnemyHorizontalDist = nextWaveHorizontalDistance - _enemySpawnMaxHorzDist;
		if (selectedFormation.HorizontalShipSpan > 1)
		{
			maxEnemyHorizontalDist /= selectedFormation.HorizontalShipSpan;
		}

		float enemyHorizontalDist;
		if (maxEnemyHorizontalDist < _enemySpawnMinHorzDist)
		{
			enemyHorizontalDist = maxEnemyHorizontalDist;
		}
		else
		{
			maxEnemyHorizontalDist = Mathf.Clamp(maxEnemyHorizontalDist, _enemySpawnMinHorzDist, _enemySpawnMaxHorzDist);
			enemyHorizontalDist = Random.Range(_enemySpawnMinHorzDist, maxEnemyHorizontalDist);
		}

		//III. Determine Vertical Distance Between Enemies
		float verticalMovementLength = _vertMaxShipSpawnCoord - _vertMinShipSpawnCoord;
		float minEnemyVerticalDist = _enemySpawnMinVertDist;
		if (hasNoExit)
		{
			int maxIntervalCount = selectedFormation.WaveEntities.Count - 1;

			float minVerticalDistance = (verticalMovementLength - ShipColliderVertSize)/maxIntervalCount;
			if (minVerticalDistance > minEnemyVerticalDist)
			{
				minEnemyVerticalDist = minVerticalDistance;
			}
		}
		float enemyVerticalDist = Random.Range(minEnemyVerticalDist, _enemySpawnMaxVertDist);

		//IV. Determine Number of Enemies
		int lowerIntervalCount = Mathf.FloorToInt((verticalMovementLength - ShipColliderVertSize)/enemyVerticalDist);
		int higherIntervalCount = Mathf.FloorToInt(verticalMovementLength/enemyVerticalDist);

		int maxPossibleVerticalIntervalCount = (lowerIntervalCount == higherIntervalCount) && !hasNoExit ? lowerIntervalCount : lowerIntervalCount + 1;

		float distBetweenFirstAndLastShip = enemyVerticalDist*maxPossibleVerticalIntervalCount;
		Assert.IsTrue(!hasNoExit || (distBetweenFirstAndLastShip >= verticalMovementLength - ShipColliderVertSize));

		int maxPossibleShipCount = maxPossibleVerticalIntervalCount + 1;
		int enemyCount;
		if (hasNoExit)
		{
			enemyCount = maxPossibleShipCount;
		}
		else
		{
			//no possible no-exits here!
			int enemyMaxCount = Mathf.Min(maxPossibleShipCount - 1, selectedFormation.WaveEntities.Count);
			enemyCount = Random.Range(enemyMaxCount - 2, enemyMaxCount);
		}

		int actualVerticalIntervalCount = enemyCount - 1;
		float minVerticalStartCoord = _vertMinShipSpawnCoord;
		float maxVerticalStartCoord = _vertMaxShipSpawnCoord - actualVerticalIntervalCount*enemyVerticalDist;

		if (maxVerticalStartCoord < minVerticalStartCoord)
		{
			//we just went off the line, this is only possible for no exit formations!
			Assert.IsTrue(hasNoExit);

			//swap these two
			maxVerticalStartCoord += minVerticalStartCoord;
			minVerticalStartCoord = maxVerticalStartCoord - minVerticalStartCoord;
			maxVerticalStartCoord -= minVerticalStartCoord;

			if (_hugeEnemyExists)
			{
				if (!Mathf.Approximately(_vertMinShipSpawnCoord, Player.MinVerticalMovementLimit))
				{
					minVerticalStartCoord = maxVerticalStartCoord;
				}
				else if (!Mathf.Approximately(_vertMaxShipSpawnCoord, Player.MaxVerticalMovementLimit))
				{
					maxVerticalStartCoord = minVerticalStartCoord;
				}
				else
				{
					Assert.IsTrue(false); //something is fishy, spawning a huge enemy didn't change vertical spawn coords
				}
			}
		}
		else
		{
			Assert.IsTrue(distBetweenFirstAndLastShip <= verticalMovementLength);
		}

		//V. Select Enemies From Formation List
	    List<WaveEntity> selectedFormationEntities = SelectEnemiesFromFormation(enemyCount);

		//VI. Determine Advanced Enemy Count
		int enemyTypeCount = EnemyPrefabArray.Length;
		int[] enemyTypeSteps = new int[enemyTypeCount];
		for (int i = 0; i < enemyTypeSteps.Length; ++i)
		{
			enemyTypeSteps[i] = Mathf.RoundToInt(i*100.0f/(enemyTypeSteps.Length - 1));
			//enemyTypeSteps = {0, 100} for 2 enemies, {0, 50, 100} for 3 enemies, {0, 33, 67, 100} for 4 enemies, and so on
		}

		float advancedEnemyPercentage = _difficultyManagerScript.DifficultyCoefs[DifficultyParameter.DpEnemyShipStrength]*stepSize - stepSize*0.5f;

		int advEnemyTypeIndex = 1;
		float percentageOfStrongerEnemy = 0.0f;

		if (enemyTypeSteps.Length > 1)
		{
			int currentEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex];
			while (advancedEnemyPercentage > currentEnemyTypeStep)
			{
				++advEnemyTypeIndex;
				Assert.IsTrue(advEnemyTypeIndex < enemyTypeSteps.Length);
				currentEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex];
			}
			// if we're here, we know which two enemies we're gonna use
			int previousEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex - 1];
			percentageOfStrongerEnemy = (advancedEnemyPercentage - previousEnemyTypeStep)/(currentEnemyTypeStep - previousEnemyTypeStep);
		}

		int minAdvancedEnemyCount = Mathf.FloorToInt(percentageOfStrongerEnemy*selectedFormationEntities.Count);
		int maxAdvancedEnemyCount = Mathf.CeilToInt(percentageOfStrongerEnemy*selectedFormationEntities.Count);
		int advancedEnemyCount = Random.Range(minAdvancedEnemyCount, maxAdvancedEnemyCount + 1);

		//create ship types list
		int[] shipTypes = new int[selectedFormationEntities.Count];
		for (int i = 0; i < shipTypes.Length; ++i)
		{
			if (i < advancedEnemyCount)
			{
				shipTypes[i] = advEnemyTypeIndex;
			}
			else
			{
				shipTypes[i] = advEnemyTypeIndex - 1;
			}
		}

		//shuffle the list
		for (int i = 0; i < shipTypes.Length; ++i)
		{
			int temp = shipTypes[i];
			int randomIndex = Random.Range(i, shipTypes.Length);
			shipTypes[i] = shipTypes[randomIndex];
			shipTypes[randomIndex] = temp;
		}

		//VII. Spawn Enemies
		SpawnEnemies(selectedFormationEntities, shipTypes, minVerticalStartCoord, maxVerticalStartCoord, enemyHorizontalDist, enemyVerticalDist, maxEnemyHorizontalDist);
	}

    private List<WaveEntity> SelectEnemiesFromFormation(int enemyCountInWave)
    {
        List<WaveEntity> selectedFormationEntities = new List<WaveEntity>();
        for (int i = 0; i < enemyCountInWave; ++i)
        {
            selectedFormationEntities.Add(_formations[0].WaveEntities[i]);
        }
        selectedFormationEntities.Sort(FormationComparison);
        return selectedFormationEntities;
    }

    private void SpawnEnemies(List<WaveEntity> selectedFormationEntities, int[] shipTypes, float minVerticalStartCoord, float maxVerticalStartCoord, float enemyHorizontalDist, float enemyVerticalDist, float maxEnemyHorizontalDist)
    {
        //VII. Spawn Enemies
        GameObject waveScoreIndicator = Instantiate(WaveScoreIndicatorPrefab);
        waveScoreIndicator.transform.SetParent(CanvasScorePanel.transform, false);

        GameObject lineRendererObject = Instantiate(ShipConnectionPrefab, Vector3.zero, Quaternion.identity);
        EnemyWave curEnemyWave = new EnemyWave(lineRendererObject.GetComponent<LineRenderer>());
        curEnemyWave.Initialize(_playerScript, _canvasRectTransform, waveScoreIndicator, _scoreLeftAnchorXPos, _scoreRightAnchorXPos);

        for (int i = 0; i < selectedFormationEntities.Count; i++)
        {
            int enemyKind = shipTypes[i];
            GameObject enemyPrefab = EnemyPrefabArray[enemyKind];
            BasicEnemy enemyPrefabScript = enemyPrefab.GetComponent<BasicEnemy>();

            Vector2 enemyPos;
            if (i > 0)
            {
                Vector2 posDiff = selectedFormationEntities[i].Position - selectedFormationEntities[i - 1].Position;

                int xPosDiff = (int)posDiff.x;
                int yPosDiff = (int)posDiff.y;

                int xIncrement = xPosDiff != 0 ? Math.Sign(xPosDiff) : 0;
                int yIncrement = yPosDiff != 0 ? Math.Sign(yPosDiff) : 0;

                Vector3 previousEnemyPos = curEnemyWave.GetLastEnemyPosition();

                enemyPos = new Vector2(previousEnemyPos.x + xIncrement * enemyHorizontalDist, previousEnemyPos.y + yIncrement * enemyVerticalDist);
            }
            else
            {
                enemyPos = new Vector2(enemyPrefabScript.HorizontalSpawnCoord + selectedFormationEntities[i].Position.x * maxEnemyHorizontalDist, Random.Range(minVerticalStartCoord, maxVerticalStartCoord));
            }

            GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
            Assert.IsNotNull(enemy);

            BasicMove basicMoveScript = enemy.GetComponent<BasicMove>();
            basicMoveScript.SetMoveDir(selectedFormationEntities[i].MoveDir, false);

            curEnemyWave.AddNewEnemy(enemy);
            enemy.GetComponent<BasicEnemy>().Initialize(_playerScript, _difficultyManagerScript, curEnemyWave);
        }
        curEnemyWave.FinalizeWidthNodes();
        _enemyWaves.Add(curEnemyWave);
    }

	private int FormationComparison(WaveEntity entity1, WaveEntity entity2)
	{
		if (entity1.Position.y < entity2.Position.y)
		{
			return -1;
		}
		if (entity1.Position.y > entity2.Position.y)
		{
			return 1;
		}
		return 0;
	}

	private void SpawnTutorialHugeEnemy()
	{
		GameObject hugeEnemyPrefab = HugeEnemyPrefabArray[0];
		HugeEnemy hugeEnemyScript = hugeEnemyPrefab.GetComponent<HugeEnemy>();
		Vector3 hugeEnemyPos = new Vector2(hugeEnemyScript.HorizontalSpawnCoord, (hugeEnemyScript.VerticalSpawnLimits[0] + hugeEnemyScript.VerticalSpawnLimits[1]) * 0.5f);
		SpawnHugeEnemyOnPosition(hugeEnemyPrefab, hugeEnemyScript, hugeEnemyPos);
	}

	private void SpawnNewHugeEnemy()
	{
		Assert.IsTrue(!_hugeEnemyExists); //we only want one huge enemy at once on the screen
		EventLogger.PrintToLog("New Huge Enemy Spawn");
		ResetVerticalSpawnLimits();

		float randomIntervalCoef = Random.Range(MinHugeEnemySpawnIntervalCoef, MaxHugeEnemySpawnIntervalCoef);
		_hugeEnemySpawnInterval = randomIntervalCoef/_difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpHugeEnemySpawnRateIncrease);

		//randomly select a prefab among available options
		int hugeEnemyIndex = Random.Range(0, HugeEnemyPrefabArray.Length);
		GameObject hugeEnemyPrefab = HugeEnemyPrefabArray[hugeEnemyIndex];
		HugeEnemy hugeEnemyScript = hugeEnemyPrefab.GetComponent<HugeEnemy>();
        
	    float diffOfPosition = hugeEnemyScript.VerticalSpawnLimits[1] - hugeEnemyScript.VerticalSpawnLimits[0];
	    int hugeEnemyIntrusionSize = (int)DifficultyParameter.DpHugeEnemyIntrusionSize;

	    float curDiffCoef = (float)(hugeEnemyIntrusionSize - 1) / (GameConstants.MaxDifficulty - GameConstants.MinDifficulty); // 4/4, 3/4, 2/4, 1/4, 0/4

	    float verticalSpawnCoord = hugeEnemyScript.VerticalSpawnLimits[0] + diffOfPosition * curDiffCoef;

        Vector3 hugeEnemyPos = new Vector2(hugeEnemyScript.HorizontalSpawnCoord, verticalSpawnCoord);
		SpawnHugeEnemyOnPosition(hugeEnemyPrefab, hugeEnemyScript, hugeEnemyPos);
	}

	private void SpawnHugeEnemyOnPosition(GameObject hugeEnemyPrefab, HugeEnemy hugeEnemyScript, Vector2 spawnPos)
	{
		GameObject hugeEnemy = Instantiate(hugeEnemyPrefab, spawnPos, Quaternion.identity);
		hugeEnemy.GetComponent<HugeEnemy>().Initialize(_playerScript, _difficultyManagerScript);

		float colliderBoundary = hugeEnemyScript.VerticalColliderBoundary;
		if (Mathf.Approximately(Mathf.Sign(colliderBoundary), 1.0f))
		{
			//positive collider boundary means we're dealing with a huge enemy below, hence we should limit vMin
			_vertMinShipSpawnCoord = spawnPos.y + colliderBoundary + ShipColliderVertSize + 0.01f;
		}
		else
		{
			//negative collider boundary = huge enemy above = vMax
			_vertMaxShipSpawnCoord = spawnPos.y + colliderBoundary - ShipColliderVertSize - 0.01f;
		}

		SetHugeEnemyExists(true);
	}

	private void SpawnTutorialPowerup(TutorialPowerupItem tutorialPowerupItem)
	{
		GameObject selectedPowerup;
		int powerupIndex = (int) tutorialPowerupItem.TypeOfPowerup;
		if (powerupIndex >= PosPowerupPrefabArray.Length)
		{
			powerupIndex -= PosPowerupPrefabArray.Length;
			selectedPowerup = NegPowerupPrefabArray[powerupIndex];
		}
		else
		{
			selectedPowerup = PosPowerupPrefabArray[powerupIndex];
		}
		BasicMove powerupMoveScript = selectedPowerup.GetComponent<BasicMove>();
		Vector3 powerupPos = new Vector2(powerupMoveScript.HorizontalLimits[1], (powerupMoveScript.VerticalLimits[0] + powerupMoveScript.VerticalLimits[1]) * 0.5f);
		GameObject instantiatedPowerup = Instantiate(selectedPowerup, powerupPos, Quaternion.identity);
		instantiatedPowerup.GetComponent<BasicMove>().SetMoveDir(Vector2.left, false);
	}

	private void SpawnNewPowerup(bool isPositive)
	{
		float randomIntervalCoef = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval*2);

		GameObject[] powerupPrefabArray;
		if (isPositive)
		{
			_posPowerupSpawnInterval = randomIntervalCoef*_difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpPosPowerupSpawnRateDecrease);
			powerupPrefabArray = PosPowerupPrefabArray;
		}
		else
		{
			_negPowerupSpawnInterval = randomIntervalCoef/_difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpNegPowerupSpawnRateIncrease);
			powerupPrefabArray = NegPowerupPrefabArray;
		}

		List<int> occurenceList = new List<int>();
		int powerupCount = powerupPrefabArray.Length;
		for (int i = 0; i < powerupCount; ++i)
		{
			int powerupOccurence = powerupPrefabArray[i].GetComponent<Powerup>().PowerupOccurence;
			for (int j = 0; j < powerupOccurence; ++j)
			{
				occurenceList.Add(i);
			}
		}

		int powerupIndex = Random.Range(0, occurenceList.Count);
		GameObject selectedPowerup = powerupPrefabArray[occurenceList[powerupIndex]];
		BasicMove powerupMoveScript = selectedPowerup.GetComponent<BasicMove>();
		Vector3 powerupPos = new Vector2(powerupMoveScript.HorizontalLimits[1], Random.Range(powerupMoveScript.VerticalLimits[0], powerupMoveScript.VerticalLimits[1]));
		Instantiate(selectedPowerup, powerupPos, Quaternion.identity);
	}

	public void SetHugeEnemyExists(bool newValue)
	{
		_hugeEnemyExists = newValue;
	}

	public void ResetVerticalSpawnLimits()
	{
		_vertMinShipSpawnCoord = Player.MinVerticalMovementLimit;
		_vertMaxShipSpawnCoord = Player.MaxVerticalMovementLimit;
	}

	public float GetVertMinShipSpawnCoord()
	{
		return _vertMinShipSpawnCoord;
	}

	public float GetVertMaxShipSpawnCoord()
	{
		return _vertMaxShipSpawnCoord;
	}
}
