/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using System;
using System.Collections.Generic;
using CnControls;
using ui;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum GunType
{
	GtStun,
	GtSpeedUp,
	GtTeleport,
	GtCount
}

public class Gun
{
	public GameObject BulletPrefab { get; private set; }
	public float Cooldown { get; private set; }
	public float LastFireTime;
	public int AmmoCount;
	public GameObject LastBullet;
	public bool CanBeFired;

	public Gun(GameObject bulletPrefab, float cooldown, int ammoCount)
	{
		BulletPrefab = bulletPrefab;
		Cooldown = cooldown;
		AmmoCount = ammoCount;
		LastFireTime = -cooldown;
		CanBeFired = true;
	}
}

public class Player : MonoBehaviour
{
	public static int PlayerScore = 0;

	public bool UseTouchControls;

	public GameObject StunBulletPrefab;
	public GameObject SpeedUpBulletPrefab;
	public GameObject TeleportBulletPrefab;
	public GameObject TeleportDisappearPrefab;
	public GameObject TeleportAppearPrefab;
	public float PlayerSpeedLimit;

	public const float MinHorizontalMovementLimit = -0.15f;
	public const float MaxHorizontalMovementLimit = 7.15f;
	public const float MinVerticalMovementLimit = 0.45f;
	public const float MaxVerticalMovementLimit = 5.15f;

	public AudioClip FireStunGunClip;
	public AudioClip FireSpeedUpGunClip;
	public AudioClip FireTeleportGunClip;

	public const int PlayerInitialHealth = 3;

	public int PlayerHealth { get; private set; }
	public bool IsDead { get; private set; }

	private const float ShortTermHealthChangeInterval = 10.0f;
	private const float LongTermHealthChangeInterval = 30.0f;

	private float _currentMaxHorizontalMovementLimit;

	private bool _isInvulnerable;
	private bool _isShielded;

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
	private bool _canDoMovement;

	private void Awake()
	{
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		_childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
		_spawnManagerScript = Camera.main.GetComponent<SpawnManager>();
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();
		_endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();

		_guns = new Dictionary<GunType, Gun>
		{
			{GunType.GtStun, new Gun(StunBulletPrefab, 0.3f, -1)},
			{GunType.GtSpeedUp, new Gun(SpeedUpBulletPrefab, 5.0f, 3)},
			{GunType.GtTeleport, new Gun(TeleportBulletPrefab, 10.0f, 10)}
		};
		_canDoMovement = true;
		_currentMaxHorizontalMovementLimit = MaxHorizontalMovementLimit;
	}

	private void Start()
	{
		PlayerScore = 0;
		PlayerHealth = PlayerInitialHealth;
		IsDead = false;
		_isShielded = false;
		_isInvulnerable = true;
		_invulnerabilityStartTime = Time.time;

		_basicObjectScript = gameObject.GetComponent<BasicObject>();
		
		//find shield object in children
		foreach (Transform tr in transform)
		{
			if (tr.tag == "Shield")
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

		//shooting
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
			fireInputGiven = Input.GetKey(KeyCode.Z);
			speedUpInputGiven = Input.GetKeyDown(KeyCode.X);
			teleportInputGiven = Input.GetKeyDown(KeyCode.C);
		}
		
		if (fireInputGiven && _guns[GunType.GtStun].CanBeFired && Time.time - _guns[GunType.GtStun].LastFireTime > _guns[GunType.GtStun].Cooldown)
		{
			FireGun(GunType.GtStun);
			_statsManagerScript.StartFireGunCoroutine(GunType.GtStun);
		}

		if (speedUpInputGiven && _guns[GunType.GtSpeedUp].CanBeFired &&
			Time.time - _guns[GunType.GtSpeedUp].LastFireTime > _guns[GunType.GtSpeedUp].Cooldown &&
			_guns[GunType.GtSpeedUp].AmmoCount > 0)
		{
			FireGun(GunType.GtSpeedUp);
			_statsManagerScript.StartFireGunCoroutine(GunType.GtSpeedUp);
		}

		if (teleportInputGiven)
		{
			if (_guns[GunType.GtTeleport].CanBeFired)
			{
				if (_guns[GunType.GtTeleport].LastBullet)
				{
					TriggerTeleportBullet();
				}
				else if (Time.time - _guns[GunType.GtTeleport].LastFireTime > _guns[GunType.GtTeleport].Cooldown &&
						 _guns[GunType.GtTeleport].AmmoCount > 0)
				{
					FireGun(GunType.GtTeleport);
					_statsManagerScript.StartFireGunCoroutine(GunType.GtTeleport);
				}
			}
		}
		if (_canDoMovement)
		{
			DoMovement();
		}
	}

	private Vector2 GetMoveDirFromInput()
	{
		float horizontalMoveDir;
		float verticalMoveDir;
		if (UseTouchControls)
		{
			horizontalMoveDir = CnInputManager.GetAxis("Horizontal");
			horizontalMoveDir = Mathf.Abs(horizontalMoveDir) < GameConstants.JoystickDeadZoneCoef ? 
				0.0f : Mathf.Sign(horizontalMoveDir);

			verticalMoveDir = CnInputManager.GetAxis("Vertical");
			verticalMoveDir = Mathf.Abs(verticalMoveDir) < GameConstants.JoystickDeadZoneCoef ?
				0.0f : Mathf.Sign(verticalMoveDir);
		}
		else
		{
			horizontalMoveDir = Input.GetAxisRaw("Horizontal");
			verticalMoveDir = Input.GetAxisRaw("Vertical");
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
		Vector2 playerMovement = movementDir * PlayerSpeedLimit * Time.deltaTime;
		Vector3 oldPosition = transform.position;
		transform.Translate(playerMovement, Space.World);

		Vector3 clampedPlayerPosition = 
			new Vector3(Mathf.Clamp(transform.position.x, MinHorizontalMovementLimit, _currentMaxHorizontalMovementLimit), 
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

		PlayerHealth--;
		_statsManagerScript.HealthChangeCoroutine(-1);

		if (PlayerHealth == 0)
		{
			EventLogger.PrintToLog("Player Dies: Game Over");
			_endGameScoreText.SetTextVisible();
			Destroy(gameObject);
		}
		else
		{
			EventLogger.PrintToLog("Player Loses Health");
			if (_isShielded)
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

		_isShielded = false;
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
		PlayerHealth++;

		_statsManagerScript.HealthChangeCoroutine(1);
		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtHealth);
	}

	public void TriggerSpeedUpPickup()
	{
		EventLogger.PrintToLog("Player Gains Speedup Powerup");
		_guns[GunType.GtSpeedUp].AmmoCount++;

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtSpeedup);
	}

	public void TriggerResearchPickup()
	{
		EventLogger.PrintToLog("Player Gains Research Powerup");
		PlayerScore += 5 * GameConstants.BaseScoreMultiplier;

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtResearch);
	}

	public void TriggerShieldPickup()
	{
		EventLogger.PrintToLog("Player Gains Shield");
		if (!_isShielded)
		{
			_isShielded = true;
			_playerShield.SetActive(true);
		}
		else
		{
			PlayerScore += GameConstants.BaseScoreMultiplier;
		}

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtShield);
	}

	public void TriggerTeleportPickup()
	{
		EventLogger.PrintToLog("Player Gains Teleport Powerup");
		_guns[GunType.GtTeleport].AmmoCount++;

		_statsManagerScript.PickupPowerupCoroutine(PowerupType.PtTeleport);
	}

	public float GetGunAmmo(GunType gunType)
	{
		Assert.IsTrue(_guns.ContainsKey(gunType));
		return _guns[gunType].AmmoCount;
	}

	public float GetGunCooldownPercentage(GunType gunType)
	{
		Assert.IsTrue(_guns.ContainsKey(gunType));
		float gunCooldown = _guns[gunType].Cooldown;
		float lastFireTime = _guns[gunType].LastFireTime;
		return (Time.time - lastFireTime)/gunCooldown;
	}

	public void BeginTutorial()
	{
		_canDoMovement = false;
		DeactivateAllGuns(); //TODO TUTORIAL undo deactivating guns step by step during tutorial
		SetCurrentMaxHorizontalMovementLimit((MaxHorizontalMovementLimit + MinHorizontalMovementLimit) * 0.5f);
	}

	public void EndTutorial()
	{
		//TODO TUTORIAL anything?
	}

	public void ActivateMovement()
	{
		if (!_canDoMovement)
		{
			//first call will enable movement
			_canDoMovement = true;
		}
		else
		{
			//second call will set movement limit as whole screen
			SetCurrentMaxHorizontalMovementLimit(MaxHorizontalMovementLimit);
		}
	}

	public void ActivateGun(GunType gunType)
	{
		Assert.IsTrue(_guns.ContainsKey(gunType));
		_guns[gunType].CanBeFired = true;
	}

	private void DeactivateAllGuns()
	{
		//TODO TUTORIAL potentially, we might want to reset gun ammo counts here!
		foreach (var element in _guns)
		{
			element.Value.CanBeFired = false;
		}
	}

	private void SetCurrentMaxHorizontalMovementLimit(float newMaxLimit)
	{
		_currentMaxHorizontalMovementLimit = newMaxLimit;
	}

	private void SetChildRenderers(bool value)
	{
		foreach (SpriteRenderer r in _childRenderers)
		{
			if (r.gameObject.tag == "Shield")
			{
				if (_isShielded)
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

	private void FireGun(GunType gunType)
	{
		switch (gunType)
		{
			case GunType.GtStun:
				FireStunGun();
				break;
			case GunType.GtSpeedUp:
				FireSpeedUpGun();
				break;
			case GunType.GtTeleport:
				FireTeleportGun();
				break;
			default:
				//TODO LATER perhaps you'll learn how to catch an exception properly
				throw new ArgumentOutOfRangeException("gunType", gunType, null);
		}
	}

	private void FireStunGun()
	{
		_guns[GunType.GtStun].LastFireTime = Time.time;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				Instantiate(_guns[GunType.GtStun].BulletPrefab, bulletStartPoint, Quaternion.identity);
				AudioSource.PlayClipAtPoint(FireStunGunClip, transform.GetChild(i).position);
			}
		}
	}

	private void FireSpeedUpGun()
	{
		_guns[GunType.GtSpeedUp].LastFireTime = Time.time;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				Instantiate(_guns[GunType.GtSpeedUp].BulletPrefab, bulletStartPoint, Quaternion.identity);
				_guns[GunType.GtSpeedUp].AmmoCount--;
				AudioSource.PlayClipAtPoint(FireSpeedUpGunClip, bulletStartPoint);
			}
		}
	}

	private void FireTeleportGun()
	{
		_guns[GunType.GtTeleport].LastFireTime = Time.time;
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				_guns[GunType.GtTeleport].LastBullet = Instantiate(_guns[GunType.GtTeleport].BulletPrefab, bulletStartPoint, Quaternion.identity);
				_guns[GunType.GtTeleport].AmmoCount--;
				AudioSource.PlayClipAtPoint(FireTeleportGunClip, bulletStartPoint);
			}
		}
	}

	private void TriggerTeleportBullet()
	{
		if (_guns[GunType.GtTeleport].LastBullet)
		{
			EventLogger.PrintToLog("Player Triggers Teleport");

			if (TeleportDisappearPrefab != null)
			{
				GameObject disappearEffect = Instantiate(TeleportDisappearPrefab, transform.position, Quaternion.identity);
				Assert.IsNotNull(disappearEffect);
				disappearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
			}

			transform.position = _guns[GunType.GtTeleport].LastBullet.transform.position;
			AudioSource.PlayClipAtPoint(_guns[GunType.GtTeleport].LastBullet.GetComponent<Bullet>().BulletHitClip, transform.position);

			if (TeleportAppearPrefab != null)
			{
				GameObject appearEffect = Instantiate(TeleportAppearPrefab, transform.position, Quaternion.identity);
				Assert.IsNotNull(appearEffect);
				appearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
			}

			Destroy(_guns[GunType.GtTeleport].LastBullet.gameObject);
			_guns[GunType.GtTeleport].LastBullet = null;
		}
	}
}
