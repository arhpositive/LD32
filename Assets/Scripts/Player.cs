/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using System.Collections.Generic;
using CnControls;
using ui;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum MovementState
{
    MsNoMovement,
    MsVerticalMovement,
    MsFreeMovement
}

public class Player : MonoBehaviour
{
    // ReSharper disable once RedundantDefaultMemberInitializer
	public static int PlayerScore = 0;

	public bool UseTouchControls;

	public GameObject StunBulletPrefab;
	public GameObject SpeedUpBulletPrefab;
	public GameObject TeleportBulletPrefab;
	public GameObject TeleportDisappearPrefab;
	public GameObject TeleportAppearPrefab;
	public float PlayerSpeed;

	public const float MinHorizontalMovementLimit = -0.15f;
	public const float MaxHorizontalMovementLimit = 7.15f;
	public const float MinVerticalMovementLimit = 0.45f;
	public const float MaxVerticalMovementLimit = 5.15f;

	public AudioClip FireStunGunClip;
	public AudioClip FireSpeedUpGunClip;
	public AudioClip FireTeleportGunClip;

	public const int PlayerInitialHealth = 3;

	public int PlayerHealth { get; private set; }
    public int TotalResearchPickedUp { get; private set; }
	public bool IsDead { get; private set; }
    public bool IsShielded { get; private set; }
    public bool TeleportedWithLastTrigger { get; private set; }

	private bool _isInvulnerable;
    private bool _losesHealth;
    private bool _inTutorial;

	private SpawnManager _spawnManagerScript;
	private StatsManager _statsManagerScript;
	private RefreshEndScoreText _endGameScoreText;

	private GameObject _playerShield;

	private BasicObject _basicObjectScript;

	private const float DeathDuration = 2.0f;
	private float _deathTime;

	private const float InvulnerabilityDuration = 2.0f;
	private float _invulnerabilityStartTime;

	private SpriteRenderer _spriteRenderer;
	private SpriteRenderer[] _childRenderers;

	private Dictionary<GunType, Gun> _guns;
    private MovementState _currentMovementState;

	private void Awake()
	{
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		_childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
		_spawnManagerScript = Camera.main.GetComponent<SpawnManager>();
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();
		_endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();

		_guns = new Dictionary<GunType, Gun>
		{
			{GunType.GtStun, new Gun(StunBulletPrefab, FireStunGunClip, 0.3f)},
			{GunType.GtSpeedUp, new Gun(SpeedUpBulletPrefab, FireSpeedUpGunClip, 5.0f, true, 3)},
			{GunType.GtTeleport, new Gun(TeleportBulletPrefab, FireTeleportGunClip, 10.0f, true, 10)}
		};
        _currentMovementState = MovementState.MsFreeMovement;
	    _losesHealth = true;
        _inTutorial = false;
	}

	private void Start()
	{
		PlayerScore = 0;
		PlayerHealth = PlayerInitialHealth;
	    TotalResearchPickedUp = 0;
		IsDead = false;
		IsShielded = false;
	    TeleportedWithLastTrigger = false;
		_isInvulnerable = true;
		_invulnerabilityStartTime = Time.time;

		_basicObjectScript = gameObject.GetComponent<BasicObject>();
		
		//find shield object in children
		foreach (Transform tr in transform)
		{
			if (tr.CompareTag("Shield"))
			{
				_playerShield = tr.gameObject;
				break;
			}
		}
	}

	private void Update()
	{
		if (IsDead)
		{
			if (Time.time - _deathTime <= DeathDuration)
			{
				return;
			}

			EventLogger.PrintToLog("Player Respawns");

			//rise from dead
			IsDead = false;
			_spriteRenderer.enabled = true;
			SetChildRenderers(true);

			EventLogger.PrintToLog("Invulnerability Start");

			//spawn
			transform.position = new Vector2(0.0f, Random.Range(_spawnManagerScript.GetVertMinShipSpawnCoord(), _spawnManagerScript.GetVertMaxShipSpawnCoord()));
			_isInvulnerable = true;
			_invulnerabilityStartTime = Time.time;
		}

		//invulnerability
		if (_isInvulnerable)
		{
			if ((Time.time - _invulnerabilityStartTime) % 0.5f > 0.25f)
			{
				_spriteRenderer.enabled = false;
				SetChildRenderers(false);
			}
			else
			{
				_spriteRenderer.enabled = true;
				SetChildRenderers(true);
			}
		}

		//end of invulnerability
		if (Time.time - _invulnerabilityStartTime > InvulnerabilityDuration && _isInvulnerable)
		{
			EventLogger.PrintToLog("Invulnerability End");

			_isInvulnerable = false;
			_spriteRenderer.enabled = true;
			SetChildRenderers(true);
		}

	    if (!SpawnManager.IsGamePaused())
	    {
	        //shooting
	        Gun stunGun = _guns[GunType.GtStun];
	        Gun speedUpGun = _guns[GunType.GtSpeedUp];
	        Gun teleportGun = _guns[GunType.GtTeleport];
	        bool fireInputGiven;
	        bool speedUpInputGiven;
	        bool teleportInputGiven;

	        if (UseTouchControls)
	        {
	            fireInputGiven = CnInputManager.GetButton("TouchFire");
	            speedUpInputGiven = CnInputManager.GetButtonDown("TouchSpeedUp");
	            teleportInputGiven = CnInputManager.GetButtonDown("TouchTeleport");
	        }
	        else
	        {
	            fireInputGiven = Input.GetButton("Fire1");
	            speedUpInputGiven = Input.GetButtonDown("Fire2");
	            teleportInputGiven = Input.GetButtonDown("Fire3");
	        }

	        if (fireInputGiven && stunGun.CanBeFired && Time.time - stunGun.LastFireTime > stunGun.Cooldown)
	        {
	            FireGun(stunGun);
	            _statsManagerScript.StartFireGunCoroutine(GunType.GtStun);
	        }

	        if (speedUpInputGiven && speedUpGun.CanBeFired && Time.time - speedUpGun.LastFireTime > speedUpGun.Cooldown && speedUpGun.CurrentAmmoCount > 0)
	        {
	            FireGun(speedUpGun);
	            _statsManagerScript.StartFireGunCoroutine(GunType.GtSpeedUp);
	        }

	        if (teleportInputGiven)
	        {
	            if (teleportGun.CanBeFired)
	            {
	                if (teleportGun.LastBullet)
	                {
	                    TriggerTeleportBullet();
	                }
	                else if (Time.time - teleportGun.LastFireTime > teleportGun.Cooldown && teleportGun.CurrentAmmoCount > 0)
	                {
	                    FireGun(teleportGun);
	                    TeleportedWithLastTrigger = false;
	                    _statsManagerScript.StartFireGunCoroutine(GunType.GtTeleport);
	                }
	            }
	        }
        }

	    DoMovement();
    }

	private Vector2 GetMoveDirFromInput()
	{
		float horizontalMoveDir = 0.0f;
		float verticalMoveDir = 0.0f;
		if (UseTouchControls)
		{
		    if (_currentMovementState == MovementState.MsFreeMovement)
		    {
		        horizontalMoveDir = CnInputManager.GetAxis("Horizontal");
		        horizontalMoveDir = Mathf.Abs(horizontalMoveDir) < GameConstants.JoystickDeadZoneCoef ?
		            0.0f : Mathf.Sign(horizontalMoveDir);
            }
		    if (_currentMovementState >= MovementState.MsVerticalMovement)
		    {
		        verticalMoveDir = CnInputManager.GetAxis("Vertical");
		        verticalMoveDir = Mathf.Abs(verticalMoveDir) < GameConstants.JoystickDeadZoneCoef ?
		            0.0f : Mathf.Sign(verticalMoveDir);
            }
		}
		else
		{
		    if (_currentMovementState == MovementState.MsFreeMovement)
		    {
		        horizontalMoveDir = Input.GetAxisRaw("Horizontal");
		    }
		    if (_currentMovementState >= MovementState.MsVerticalMovement)
		    {
		        verticalMoveDir = Input.GetAxisRaw("Vertical");
            }
		}

		Vector2 moveDir = new Vector2(horizontalMoveDir, verticalMoveDir);
		moveDir.Normalize();
		return moveDir;
	}

	private void DoMovement()
	{
		//movement
		Vector2 inputDir = GetMoveDirFromInput();

		Vector2 movementDir = Vector2.ClampMagnitude(inputDir, 1.0f);
		Vector2 playerMovement = movementDir * PlayerSpeed * Time.deltaTime;
		Vector3 oldPosition = transform.position;
		transform.Translate(playerMovement, Space.World);

		Vector3 clampedPlayerPosition = 
			new Vector3(Mathf.Clamp(transform.position.x, MinHorizontalMovementLimit, MaxHorizontalMovementLimit), 
			Mathf.Clamp(transform.position.y, MinVerticalMovementLimit, MaxVerticalMovementLimit), 
			transform.position.z);

		float movementMagnitude = (clampedPlayerPosition - oldPosition).magnitude;
		_statsManagerScript.StartMovementCoroutine(clampedPlayerPosition, movementMagnitude);
		
		transform.position = clampedPlayerPosition;
	}

	public bool PlayerGotHit()
	{
		if (_isInvulnerable || IsDead || PlayerHealth == 0)
		{
			// bullet goes right through player
			return false;
		}

	    if (_inTutorial)
	    {
	        _spawnManagerScript.TutorialOnPlayerDeath();
        }

	    if (_losesHealth)
	    {
	        --PlayerHealth;
	        _statsManagerScript.HealthChangeCoroutine(-1);
	    }

		if (PlayerHealth == 0)
		{
			EventLogger.PrintToLog("Player Dies: Game Over");
			_endGameScoreText.SetTextVisible();
			Destroy(gameObject);
		}
		else
		{
			EventLogger.PrintToLog("Player Loses Health");
			if (IsShielded)
			{
				//shield also disappears
				ShieldGotHit();
			}
			_deathTime = Time.time;
			IsDead = true;
			_spriteRenderer.enabled = false;
			SetChildRenderers(false);
		}
		_basicObjectScript.OnDestruction();
		return true;
	}

	public bool ShieldGotHit()
	{
		if (_isInvulnerable || IsDead)
		{
			return false;
		}

		EventLogger.PrintToLog("Player Loses Shield");

		IsShielded = false;
		_playerShield.SetActive(false);
		return true;
	}

	public void TriggerEnemyDestruction()
	{
		PlayerScore -= 100;
	}

	public void TriggerEnemyWaveScoring(int scoreAddition)
	{
		PlayerScore += scoreAddition;
		EventLogger.PrintToLog("Enemy Wave Scored: " + scoreAddition);
	}

	public void TriggerHealthPickup()
	{
		EventLogger.PrintToLog("Player Gains Health");
		++PlayerHealth;

		_statsManagerScript.HealthChangeCoroutine(1);
		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtHealth);
	}

    public void TriggerAmmoPickup(GunType gunType, PowerupType powerupType)
    {
        EventLogger.PrintToLog("Player Gains Ammo Powerup: " + gunType);
        Assert.IsTrue(_guns.ContainsKey(gunType));
        _guns[gunType].PickupAmmo();

        _statsManagerScript.PickupPowerupCoroutine(powerupType);
    }

    public void TriggerResearchPickup()
	{
		EventLogger.PrintToLog("Player Gains Research Powerup");
	    ++TotalResearchPickedUp;
		PlayerScore += 5 * GameConstants.BaseScoreMultiplier * TotalResearchPickedUp;

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtResearch);
	}

	public void TriggerShieldPickup()
	{
		EventLogger.PrintToLog("Player Gains Shield");
		if (!IsShielded)
		{
			IsShielded = true;
			_playerShield.SetActive(true);
		}
		else
		{
			PlayerScore += GameConstants.BaseScoreMultiplier;
		}

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtShield);
	}

    public Gun GetGun(GunType gunType)
    {
        Assert.IsTrue(_guns.ContainsKey(gunType));
        return _guns[gunType];
    }

    public void BeginTutorial()
	{
        _currentMovementState = MovementState.MsNoMovement;
	    _losesHealth = false;
	    _inTutorial = true;

	    foreach (var gun in _guns.Values)
	    {
	        gun.SetCanBeFired(false);
            gun.DepleteAmmo();
            gun.SetAmmoUsage(false);
	    }
	}

    public void EndTutorial()
    {
        _inTutorial = false;
        foreach (var gun in _guns.Values)
        {
            gun.ResetAmmoUsage();
            gun.ResetAmmoCount();
        }
        ResetHealth();
    }

    private void ResetHealth()
    {
        _losesHealth = true;
        PlayerHealth = PlayerInitialHealth;
    }

    public void ActivateMovement()
	{
	    if (_currentMovementState != MovementState.MsFreeMovement)
	    {
	        _currentMovementState += 1;
	    }
	}

	private void SetChildRenderers(bool value)
	{
		foreach (SpriteRenderer r in _childRenderers)
		{
			if (r.gameObject.CompareTag("Shield"))
			{
				if (IsShielded)
				{
					r.enabled = value;
				}
			}
			else
			{
				r.enabled = value;
			}
		}
	}

	private void FireGun(Gun gun)
	{
	    for (int i = 0; i < transform.childCount; i++)
	    {
	        if (transform.GetChild(i).CompareTag("BulletStart"))
	        {
	            Vector3 bulletStartPoint = transform.GetChild(i).position;
	            gun.Fire(bulletStartPoint);
	        }
	    }
	}

	private void TriggerTeleportBullet()
	{
        //you can only trigger teleport if you have the last bullet, it's mandatory

	    Gun teleportGun = _guns[GunType.GtTeleport];

        Assert.IsTrue(teleportGun.LastBullet);

	    EventLogger.PrintToLog("Player Triggers Teleport");

	    if (TeleportDisappearPrefab != null)
	    {
	        GameObject disappearEffect = Instantiate(TeleportDisappearPrefab, transform.position, Quaternion.identity);
	        Assert.IsNotNull(disappearEffect);
	        disappearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
	    }

	    transform.position = teleportGun.LastBullet.transform.position;
	    AudioSource.PlayClipAtPoint(teleportGun.LastBulletScript.BulletHitClip, transform.position);

	    if (TeleportAppearPrefab != null)
	    {
	        GameObject appearEffect = Instantiate(TeleportAppearPrefab, transform.position, Quaternion.identity);
	        Assert.IsNotNull(appearEffect);
	        appearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
	    }

	    teleportGun.ResetLastBullet();
        TeleportedWithLastTrigger = true;
    }
}
