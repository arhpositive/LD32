/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicEnemy.cs
 * Handles basic enemy behaviour
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class BasicEnemy : MonoBehaviour
{
	public float StunDuration;
	public float SpeedBoostCoef;
	public bool CanShoot;
	public bool ShootsStraightAtPlayer;
	public float MinFiringInterval;
	public float MaxFiringInterval;
	public GameObject BulletPrefab;
	public float DestructionHorizontalMinCoord;
	
	public float HorizontalSpawnCoord;

	public const float MoveSpeed = 1.2f;

	//enemy displacement is used for scoring
	public float DisplacementLength { get; private set; }
	public bool IsDisplaced { get; private set; }

	private Player _playerScript;
	protected bool HasCollided;

	private DifficultyManager _difficultyManagerScript;
	private BasicMove _basicMoveScript;
	private BasicObject _basicObjectScript;
	private EnemyWave _assignedEnemyWave;
	
	private List<Vector3> _gunPositions;
	private List<Transform> _gunTransforms; 
	private bool _isStunned;
	private float _lastStunTime;
	private bool _speedBoostIsActive;
	private float _lastFireTime;
	private float _nextFiringInterval;

	protected virtual void Start()
	{
		_difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
		_basicMoveScript = gameObject.GetComponent<BasicMove>();
		_basicObjectScript = gameObject.GetComponent<BasicObject>();

		InitGunPositions();
		HasCollided = false;
		_isStunned = false;
		_lastStunTime = 0.0f;
		_speedBoostIsActive = false;
		_lastFireTime = Time.time;
		SetNextFiringInterval();
		DisplacementLength = 0.0f;
		IsDisplaced = false;
	}

	private void Update()
	{
		// if stun timer expired
		if (_isStunned && Time.time - _lastStunTime > StunDuration)
		{
			RemoveStun();
		}

		if (!_isStunned)
		{
			// if fire timer expired
			if (CanShoot && Time.time - _lastFireTime > _nextFiringInterval)
			{
				FireGun();
			}

			if (_speedBoostIsActive)
			{
				float currentDisplacement = -Time.deltaTime*_basicMoveScript.MoveSpeed*(_basicMoveScript.SpeedCoef - 1.0f);
				float displacementChange = MakeDisplacementChange(currentDisplacement);
				OnDisplaced(displacementChange);

				if (_assignedEnemyWave != null)
				{
					_assignedEnemyWave.OnEnemyDisplacementChanged();
				}
			}
			
			if (transform.position.x < DestructionHorizontalMinCoord)
			{
				if (_assignedEnemyWave != null)
				{
					_assignedEnemyWave.OnEnemyCountChanged();
				}
				RemoveFromScene();
			}
		}
	}

	public void Initialize(Player playerScript, DifficultyManager difficultyManagerScript, EnemyWave assignedWave = null)
	{
		_playerScript = playerScript;
		_difficultyManagerScript = difficultyManagerScript;
		_assignedEnemyWave = assignedWave;
	}

	public virtual void TriggerStun()
	{
		_isStunned = true;
		_lastStunTime = Time.time;
		_basicMoveScript.DoesMove = false;

		if (_assignedEnemyWave != null)
		{
			_assignedEnemyWave.OnEnemyDisplacementChanged();
		}
	}

	private void RemoveStun()
	{
		_isStunned = false;
		_lastFireTime = Time.time;
		_basicMoveScript.DoesMove = true;
		float currentDisplacement = StunDuration*_basicMoveScript.MoveSpeed;
		DisplacementLength += currentDisplacement;

		float displacementChange = MakeDisplacementChange(currentDisplacement);
		OnDisplaced(displacementChange);

		if (_assignedEnemyWave != null)
		{
			_assignedEnemyWave.OnEnemyDisplacementChanged();
		}
	}

	public virtual void TriggerSpeedBoost()
	{
		EventLogger.PrintToLog("Enemy Gains Speedup");
		if (!_speedBoostIsActive)
		{
			_speedBoostIsActive = true;
			_basicMoveScript.SpeedCoef = SpeedBoostCoef;
		}
	}

	private float MakeDisplacementChange(float changeToMake)
	{
		float oldDisplacement = DisplacementLength;
		DisplacementLength += changeToMake;
		return Mathf.Abs(DisplacementLength) - Mathf.Abs(oldDisplacement);
	}

	protected virtual void RemoveFromScene()
	{
		Destroy(gameObject);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (HasCollided)
		{
			return;
		}

		if (CheckForCollision(other))
		{
			Explode();
		}
	}

	protected bool CheckForCollision(Collider2D other)
	{
		bool collisionExists = false;

		if (other.gameObject.tag == "Player")
		{
			Assert.IsNotNull(_playerScript);
			bool playerGotHit = _playerScript.PlayerGotHit();
			if (playerGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Player");
				collisionExists = true;
			}
		}
		else if (other.gameObject.tag == "Shield")
		{
			Assert.IsNotNull(_playerScript);
			bool shieldGotHit = _playerScript.ShieldGotHit();
			if (shieldGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Shield");
				collisionExists = true;
			}
		}
		else if (other.gameObject.tag == "Enemy")
		{
			EventLogger.PrintToLog("Enemy Collision v Enemy");
			collisionExists = true;
		}

		return collisionExists;
	}

	private void FireGun()
	{
		_lastFireTime = Time.time;
		foreach (Transform gunTransform in _gunTransforms)
		{
			Vector3 globalGunPos = gunTransform.position;
			GameObject bulletGameObject = Instantiate(BulletPrefab, globalGunPos, Quaternion.identity);
			Vector3 bulletDir = gunTransform.up;

			if (_playerScript)
			{
				Vector3 playerPos = _playerScript.transform.position;
				Vector3 referenceTargetPos = globalGunPos + bulletDir*2.0f;

				float xSign = Mathf.Sign(bulletDir.x);
				float ySign = Mathf.Sign(bulletDir.y);

				if (ShootsStraightAtPlayer &&
					(Mathf.Sign(playerPos.x - referenceTargetPos.x) == xSign || bulletDir.x == 0.0f) && 
				    (Mathf.Sign(playerPos.y - referenceTargetPos.y) == ySign || bulletDir.y == 0.0f))
				{
					bulletDir = playerPos - globalGunPos;
				}
			}
			bulletGameObject.GetComponent<BasicMove>().SetMoveDir(bulletDir.normalized, true);
		}
		SetNextFiringInterval();
	}

	private void SetNextFiringInterval()
	{
		float randomIntervalCoef = Random.Range(MinFiringInterval, MaxFiringInterval);
		_nextFiringInterval = randomIntervalCoef / _difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpShipFireRateIncrease);
	}

	private void OnDisplaced(float displacementChange)
	{
		if (!IsDisplaced)
		{
			IsDisplaced = true;
			if (_assignedEnemyWave != null)
			{
				_assignedEnemyWave.IncreaseWaveMultiplier();

				//pop with coroutine!
				IEnumerator popWaveScoreCoroutine = _assignedEnemyWave.OnWaveMultiplierIncreased();
				StartCoroutine(popWaveScoreCoroutine);
			}
		}

		if (_assignedEnemyWave != null)
		{
			_assignedEnemyWave.AddDisplacementScore(displacementChange);
		}
	}

	private void Explode()
	{
		if (_assignedEnemyWave != null)
		{
			_assignedEnemyWave.OnEnemyCountChanged();
		}

		if (_playerScript)
		{
			//player might not be alive, game might have ended, do not score negative points in this case
			_playerScript.TriggerEnemyDestruction();
		}
		HasCollided = true;
		_basicObjectScript.OnDestruction();
		Destroy(gameObject);
	}

	private void InitGunPositions()
	{
		_gunTransforms = new List<Transform>();
		for (int i = 0; i < transform.childCount; ++i)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				_gunTransforms.Add((transform.GetChild(i).transform));
			}
		}

		_gunPositions = new List<Vector3>();
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).CompareTag("BulletStart"))
			{
				_gunPositions.Add(transform.GetChild(i).localPosition);
			}
		}
	}
}