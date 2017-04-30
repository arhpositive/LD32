/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using System.Collections;
using System.Collections.Generic;
using CnControls;
using ui;
using UnityEngine;
using UnityEngine.Assertions;

public enum GunType
{
	GtStun,
	GtSpeedUp,
	GtTeleport,
	GtCount
}

public struct Gun
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
		LastFireTime = -cooldown;
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

	public List<PlayerStats> AllPlayerStats { get; private set; }

	private const float ShortTermHealthChangeInterval = 10.0f;
	private const float LongTermHealthChangeInterval = 30.0f;

	private bool _isInvulnerable;
	private bool _isShielded;

	private SpawnManager _spawnManagerScript;
	private RefreshEndScoreText _endGameScoreText;

	private GameObject _playerShield;

	private BasicObject _basicObjectScript;

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
		_spawnManagerScript = Camera.main.GetComponent<SpawnManager>();
		_endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();
	}

	private void Start()
	{
		PlayerScore = 0;
		PlayerHealth = PlayerInitialHealth;
		IsDead = false;
		_isShielded = false;
		_isInvulnerable = true;
		_invulnerabilityStartTime = Time.time;

		_stunGun = new Gun(GunType.GtStun, StunBulletPrefab, 0.3f, -1);
		_speedUpGun = new Gun(GunType.GtSpeedUp, SpeedUpBulletPrefab, 5.0f, 3);
		_teleportGun = new Gun(GunType.GtTeleport, TeleportBulletPrefab, 10.0f, 10);

		_basicObjectScript = gameObject.GetComponent<BasicObject>();

		AllPlayerStats = new List<PlayerStats>()
		{
			new PlayerStats(true, ShortTermHealthChangeInterval),
			new PlayerStats(true, LongTermHealthChangeInterval),
			new PlayerStats(false)
		};
		
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

		//TODO make gun a class?
		if (fireInputGiven && Time.time - _stunGun.LastFireTime > _stunGun.Cooldown)
		{
			FireStunGun();
			StartFireGunCoroutine(_stunGun.TypeOfGun);
		}

		if (speedUpInputGiven &&
			Time.time - _speedUpGun.LastFireTime > _speedUpGun.Cooldown &&
			_speedUpGun.AmmoCount > 0)
		{
			FireSpeedUpGun();
			StartFireGunCoroutine(_speedUpGun.TypeOfGun);
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
				StartFireGunCoroutine(_teleportGun.TypeOfGun);
			}
		}

		DoMovement();
	}

	private void StartFireGunCoroutine(GunType gunType)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator fireGunCoroutine = ps.OnBulletInit(gunType);
			StartCoroutine(fireGunCoroutine);
		}
	}

	private void StartPickupPowerupCoroutine(PowerupType powerupType)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator pickupPowerupCoroutine = ps.OnPowerupPickup(powerupType);
			StartCoroutine(pickupPowerupCoroutine);
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
			new Vector3(Mathf.Clamp(transform.position.x, MinHorizontalMovementLimit, MaxHorizontalMovementLimit), 
			Mathf.Clamp(transform.position.y, MinVerticalMovementLimit, MaxVerticalMovementLimit), 
			transform.position.z);

		float movementMagnitude = (clampedPlayerPosition - oldPosition).magnitude;
		foreach(PlayerStats ps in AllPlayerStats)
		{
			IEnumerator playerMovementCoroutine = ps.OnPlayerMovement(clampedPlayerPosition, movementMagnitude);
			StartCoroutine(playerMovementCoroutine);
		}

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
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator playerHealthLossCoroutine = ps.OnPlayerHealthChange(-1);
			StartCoroutine(playerHealthLossCoroutine); //start coroutine for stats
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

	public void OnBulletDestruction(bool bulletHitEnemy)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator bulletDestructionCoroutine = ps.OnBulletDestruction(bulletHitEnemy);
			StartCoroutine(bulletDestructionCoroutine);
		}
	}

	public void TriggerEnemyDestruction()
	{
		PlayerScore -= 100;
	}

	public void TriggerEnemyWaveScoring(int waveBaseScore, int scoreAddition)
	{
		PlayerScore += scoreAddition;
		foreach (PlayerStats ps in AllPlayerStats)
		{
			ps.OnWaveDestruction(waveBaseScore);
		}
		EventLogger.PrintToLog("Enemy Wave Scored: " + scoreAddition);
	}

	public void TriggerHealthPickup()
	{
		EventLogger.PrintToLog("Player Gains Health");
		PlayerHealth++;

		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator playerHealthGainCoroutine = ps.OnPlayerHealthChange(1);
			StartCoroutine(playerHealthGainCoroutine); //start coroutine for stats
		}

		StartPickupPowerupCoroutine(PowerupType.PtHealth);
	}

	public void TriggerSpeedUpPickup()
	{
		EventLogger.PrintToLog("Player Gains Speedup Powerup");
		_speedUpGun.AmmoCount++;

		StartPickupPowerupCoroutine(PowerupType.PtSpeedup);
	}

	public void TriggerResearchPickup()
	{
		EventLogger.PrintToLog("Player Gains Research Powerup");
		PlayerScore += 5 * GameConstants.BaseScoreMultiplier;

		StartPickupPowerupCoroutine(PowerupType.PtResearch);
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

		StartPickupPowerupCoroutine(PowerupType.PtShield);
	}

	public void TriggerTeleportPickup()
	{
		EventLogger.PrintToLog("Player Gains Teleport Powerup");
		_teleportGun.AmmoCount++;

		StartPickupPowerupCoroutine(PowerupType.PtTeleport);
	}

	public float GetGunAmmo(GunType gunType)
	{
		switch (gunType)
		{
			case GunType.GtStun:
				return _stunGun.AmmoCount;
			case GunType.GtSpeedUp:
				return _speedUpGun.AmmoCount;
			case GunType.GtTeleport:
				return _teleportGun.AmmoCount;
			default:
				Assert.IsTrue(false); // gun is not added to the above list
				return 0.0f;
		}
	}

	public float GetGunCooldownPercentage(GunType gunType)
	{
		float gunCooldown;
		float lastFireTime;
		switch (gunType)
		{
			case GunType.GtStun:
				gunCooldown = _stunGun.Cooldown;
				lastFireTime = _stunGun.LastFireTime;
				break;
			case GunType.GtSpeedUp:
				gunCooldown = _speedUpGun.Cooldown;
				lastFireTime = _speedUpGun.LastFireTime;
				break;
			case GunType.GtTeleport:
				gunCooldown = _teleportGun.Cooldown;
				lastFireTime = _teleportGun.LastFireTime;
				break;
			default:
				Assert.IsTrue(false); // gun is not added to the above list
				gunCooldown = 0.0f;
				lastFireTime = 0.0f;
				break;
		}

		return (Time.time - lastFireTime)/gunCooldown;
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

			if (TeleportDisappearPrefab != null)
			{
				GameObject disappearEffect = Instantiate(TeleportDisappearPrefab, transform.position, Quaternion.identity);
				Assert.IsNotNull(disappearEffect);
				disappearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
			}

			transform.position = _teleportGun.LastBullet.transform.position;
			AudioSource.PlayClipAtPoint(_teleportGun.LastBullet.GetComponent<Bullet>().BulletHitClip, transform.position);

			if (TeleportAppearPrefab != null)
			{
				GameObject appearEffect = Instantiate(TeleportAppearPrefab, transform.position, Quaternion.identity);
				Assert.IsNotNull(appearEffect);
				appearEffect.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
			}

			Destroy(_teleportGun.LastBullet.gameObject);
			_teleportGun.LastBullet = null;
		}
	}

	public PlayerStats GetAllTimeStats()
	{
		return AllPlayerStats[0];
	}
}
