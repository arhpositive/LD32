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
    public bool SpeedBoostIsActive { get; private set; }
	public Vector2 InitialMoveDir { get; private set; }

    private Player _playerScript;
	private bool _isDisplaced;
	private bool _hasCollided;

	private DifficultyManager _difficultyManagerScript;
	private BasicMove _basicMoveScript;
	private BasicObject _basicObjectScript;
	private EnemyWave _assignedEnemyWave;
	
	private List<Transform> _gunTransforms; 
	public bool IsStunned { get; private set; }
	private float _lastStunTime;
	private float _lastFireTime;
	private float _nextFiringInterval;

	protected virtual void Start()
	{
		_basicObjectScript = gameObject.GetComponent<BasicObject>();

		InitGunPositions();
		_hasCollided = false;
		IsStunned = false;
		_lastStunTime = 0.0f;
		SpeedBoostIsActive = false;
		_lastFireTime = Time.time;
		SetNextFiringInterval();
		DisplacementLength = 0.0f;
		InitialMoveDir = Vector2.zero;
		_isDisplaced = false;
	}

	private void Update()
	{
		// if stun timer expired
		if (IsStunned && Time.time - _lastStunTime > StunDuration)
		{
			RemoveStun();
		}

		if (!IsStunned)
		{
			// if fire timer expired
			if (CanShoot && Time.time - _lastFireTime > _nextFiringInterval)
			{
				FireGun();
			}

			if (SpeedBoostIsActive)
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

	public void Initialize(Player playerScript, DifficultyManager difficultyManagerScript, BasicMove basicMoveScript, 
		Vector2 initialMoveDir, EnemyWave assignedWave = null)
	{
		_playerScript = playerScript;
		_difficultyManagerScript = difficultyManagerScript;
		_basicMoveScript = basicMoveScript;
		_assignedEnemyWave = assignedWave;
		InitialMoveDir = initialMoveDir;
		SetMoveDir(initialMoveDir);
	}

	public virtual void TriggerStun()
	{
		IsStunned = true;
		_lastStunTime = Time.time;
		_basicMoveScript.DoesMove = false;

		if (_assignedEnemyWave != null)
		{
			_assignedEnemyWave.OnEnemyDisplacementChanged();
		}
	}

	public void SetMoveDir(Vector2 newMoveDir)
	{
		_basicMoveScript.SetMoveDir(newMoveDir);
	}

	public void ResetMoveDir()
	{
		_basicMoveScript.SetMoveDir(InitialMoveDir);
	}

	private void RemoveStun()
	{
		IsStunned = false;
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
		if (!SpeedBoostIsActive)
		{
			SpeedBoostIsActive = true;
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
		if (_hasCollided)
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

		if (other.gameObject.CompareTag("Player"))
		{
			Assert.IsNotNull(_playerScript);
			bool playerGotHit = _playerScript.PlayerGotHit();
			if (playerGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Player");
				collisionExists = true;
			}
		}
		else if (other.gameObject.CompareTag("Shield"))
		{
			Assert.IsNotNull(_playerScript);
			bool shieldGotHit = _playerScript.ShieldGotHit();
			if (shieldGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Shield");
				collisionExists = true;
			}
		}
		else if (other.gameObject.CompareTag("Enemy"))
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
					(Mathf.Approximately(xSign, Mathf.Sign(playerPos.x - referenceTargetPos.x)) || Mathf.Approximately(bulletDir.x, 0.0f)) && 
				    (Mathf.Approximately(ySign, Mathf.Sign(playerPos.y - referenceTargetPos.y)) || Mathf.Approximately(bulletDir.y, 0.0f)))
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
		if (!_isDisplaced)
		{
			_isDisplaced = true;
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
		_hasCollided = true;
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
	}
}