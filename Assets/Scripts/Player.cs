﻿/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using CnControls;
using ui;
using UnityEngine;

public enum GunType //TODO use gun type in gun struct to categorize
{
	GtStun,
	GtSpeedUp,
	GtTeleport,
	GtCount
}

internal struct Gun
{
	public GunType TypeOfGun { get; private set; }
	public GameObject BulletPrefab { get; private set; }
	public float Cooldown { get; private set; }
	public float LastFireTime;
	public int AmmoCount;
	public GameObject LastBullet;

	public Gun(GunType typeOfGun, GameObject bulletPrefab, float cooldown, int ammoCount)
		: this()
	{
		TypeOfGun = typeOfGun;
		BulletPrefab = bulletPrefab;
		Cooldown = cooldown;
		AmmoCount = ammoCount;
		LastFireTime = 0.0f;
	}
}

public class Player : MonoBehaviour
{
	public static int PlayerScore = 0;

	public bool UseTouchControls;

	public GameObject StunBulletPrefab;
	public GameObject SpeedUpBulletPrefab;
	public GameObject TeleportBulletPrefab;
	public float PlayerSpeedLimit;

	public AudioClip FireStunGunClip;
	public AudioClip FireSpeedUpGunClip;
	public AudioClip FireTeleportGunClip;

	public const int PlayerInitialHealth = 3;

	public int PlayerHealth { get; private set; }
	public bool IsDead { get; private set; }
	public int DisplacedActiveEnemyCount { get; private set; }

	private bool _isInvulnerable;
	private bool _isShielded;

	private RefreshEndScoreText _endGameScoreText;

	private GameObject _playerShield;

	private BasicObject _basicObjectScript;

	public PlayerStats Stats { get; private set; }

	private const float DeathDuration = 2.0f;
	private float _deathTime;

	private const float InvulnerabilityDuration = 2.0f;
	private float _invulnerabilityStartTime;

	private SpriteRenderer _spriteRenderer;
	private SpriteRenderer[] _childRenderers;

	private Gun _stunGun;
	private Gun _speedUpGun;
	private Gun _teleportGun;

	private void Awake()
	{
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		_childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
		_endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();
	}

	private void Start()
	{
		PlayerScore = 0;
		PlayerHealth = PlayerInitialHealth;
		IsDead = false;
		DisplacedActiveEnemyCount = 0;
		_isShielded = false;
		_isInvulnerable = true;
		_invulnerabilityStartTime = Time.time;

		_stunGun = new Gun(GunType.GtStun, StunBulletPrefab, 0.3f, -1);
		_speedUpGun = new Gun(GunType.GtSpeedUp, SpeedUpBulletPrefab, 0.5f, 3);
		_teleportGun = new Gun(GunType.GtTeleport, TeleportBulletPrefab, 1.0f, 10);

		_basicObjectScript = gameObject.GetComponent<BasicObject>();

		Stats = new PlayerStats();

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
			transform.position = new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit,
				GameConstants.MaxVerticalMovementLimit));
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
		bool teleportInputGiven; //teleport device will teleport our ship to wherever the beacon is

		if (UseTouchControls)
		{
			fireInputGiven = CnInputManager.GetButton("TouchFire");
			speedUpInputGiven = CnInputManager.GetButtonDown("TouchSpeedUp");
			teleportInputGiven = false; //TODO LATER touch control for third weapon
		}
		else
		{
			fireInputGiven = Input.GetKey(KeyCode.Z);
			speedUpInputGiven = Input.GetKeyDown(KeyCode.X);
			teleportInputGiven = Input.GetKeyDown(KeyCode.C);
		}

		if (fireInputGiven && Time.time - _stunGun.LastFireTime > _stunGun.Cooldown)
		{
			FireStunGun();
		}

		if (speedUpInputGiven &&
			Time.time - _speedUpGun.LastFireTime > _speedUpGun.Cooldown &&
			_speedUpGun.AmmoCount > 0)
		{
			FireSpeedUpGun();
			//TODO LATER display weapon cooldowns on UI
			//cooldown can be represented by a bar filling up on the area that shows the weapon type
		}

		if (teleportInputGiven)
		{
			if (_teleportGun.LastBullet)
			{
				TriggerTeleportBullet();
			}
			else if (Time.time - _teleportGun.LastFireTime > _teleportGun.Cooldown &&
					 _teleportGun.AmmoCount > 0)
			{
				FireTeleportGun();
			}
		}

		DoMovement();
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
		movementDir *= PlayerSpeedLimit * Time.deltaTime;
		transform.Translate(movementDir, Space.World);

		transform.position = new Vector3(Mathf.Clamp(transform.position.x, GameConstants.MinHorizontalMovementLimit,
			GameConstants.MaxHorizontalMovementLimit), Mathf.Clamp(transform.position.y, GameConstants.MinVerticalMovementLimit,
			GameConstants.MaxVerticalMovementLimit), transform.position.z);
	}

	public bool PlayerGotHit()
	{
		if (_isInvulnerable || IsDead)
		{
			// bullet goes right through player
			return false;
		}

		PlayerHealth--;
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

	public void EnemyGotDisplaced()
	{
		++DisplacedActiveEnemyCount;
	}

	public void DisplacedEnemyGotDestroyed()
	{
		--DisplacedActiveEnemyCount;
	}

	public void OnBulletDestruction(bool bulletHitEnemy)
	{
		Stats.OnBulletDestruction(bulletHitEnemy);
	}

	public void TriggerEnemyDestruction()
	{
		PlayerScore -= 100;
	}

	public void TriggerEnemyDisplacement(int scoreAddition)
	{
		PlayerScore += scoreAddition * DisplacedActiveEnemyCount;
		EventLogger.PrintToLog("Enemy Scored: " + scoreAddition * DisplacedActiveEnemyCount);
	}

	public void TriggerHealthPickup()
	{
		EventLogger.PrintToLog("Player Gains Health");
		PlayerHealth++;
	}

	public void TriggerSpeedUpPickup()
	{
		EventLogger.PrintToLog("Player Gains Speedup Powerup");
		_speedUpGun.AmmoCount++;
	}

	public void TriggerResearchPickup()
	{
		EventLogger.PrintToLog("Player Gains Research Powerup");
		PlayerScore += (int)(5 * GameConstants.BaseScoreAddition * DisplacedActiveEnemyCount);
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
			PlayerScore += (int)(GameConstants.BaseScoreAddition * DisplacedActiveEnemyCount);
		}
	}

	public void TriggerTeleportPickup()
	{
		EventLogger.PrintToLog("Player Gains Teleport Powerup");
		_teleportGun.AmmoCount++;
	}

	public int GetSpeedUpGunAmmo()
	{
		return _speedUpGun.AmmoCount;
	}

	public int GetTeleportGunAmmo()
	{
		return _teleportGun.AmmoCount;
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

	private void FireStunGun()
	{
		_stunGun.LastFireTime = Time.time;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				Instantiate(_stunGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
				AudioSource.PlayClipAtPoint(FireStunGunClip, transform.GetChild(i).position);
			}
		}
	}

	private void FireSpeedUpGun()
	{
		_speedUpGun.LastFireTime = Time.time;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				Instantiate(_speedUpGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
				_speedUpGun.AmmoCount--;
				AudioSource.PlayClipAtPoint(FireSpeedUpGunClip, bulletStartPoint);
			}
		}
	}

	private void FireTeleportGun()
	{
		_teleportGun.LastFireTime = Time.time;
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				Vector3 bulletStartPoint = transform.GetChild(i).position;
				_teleportGun.LastBullet = Instantiate(_teleportGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
				_teleportGun.AmmoCount--;
				AudioSource.PlayClipAtPoint(FireTeleportGunClip, bulletStartPoint);
			}
		}
	}

	private void TriggerTeleportBullet()
	{
		if (_teleportGun.LastBullet)
		{
			EventLogger.PrintToLog("Player Triggers Teleport");
			transform.position = _teleportGun.LastBullet.transform.position;
			AudioSource.PlayClipAtPoint(_teleportGun.LastBullet.GetComponent<Bullet>().BulletHitClip, transform.position);

			Destroy(_teleportGun.LastBullet.gameObject);
			_teleportGun.LastBullet = null;
		}
	}
}
