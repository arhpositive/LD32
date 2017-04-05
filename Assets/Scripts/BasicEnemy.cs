/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicEnemy.cs
 * Handles basic enemy behaviour
 */

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

	//TODO NEXT consider moving to hugeEnemy
	public float[] VerticalSpawnLimits;
	public float HorizontalSpawnCoord;
	public float VerticalColliderBoundary;

	public const float MoveSpeed = 1.2f;

	protected Player PlayerScript;
	protected bool HasCollided;

	private GameObject _playerGameObject;
	private DifficultyManager _difficultyManagerScript;
	private BasicMove _basicMoveScript;
	private BasicObject _basicObjectScript;

	private List<Vector3> _gunPositions;
	private List<Transform> _gunTransforms; 
	private bool _isStunned;
	private float _lastStunTime;
	private bool _speedBoostIsActive;
	private float _lastFireTime;
	private float _nextFiringInterval;
	private float _displacementLength; //used for scoring
	private bool _isDisplaced;

	protected virtual void Start()
	{
		_playerGameObject = GameObject.FindGameObjectWithTag("Player");
		if (_playerGameObject)
		{
			PlayerScript = _playerGameObject.GetComponent<Player>();
		}
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
		_displacementLength = 0.0f;
		_isDisplaced = false;
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
				_displacementLength -= Time.deltaTime * _basicMoveScript.MoveSpeed * (_basicMoveScript.SpeedCoef - 1.0f);
				OnDisplaced();
			}
			
			if (transform.position.x < DestructionHorizontalMinCoord)
			{
				ScoreAndRemoveFromScene();
			}
		}
	}

	public virtual void TriggerStun()
	{
		_isStunned = true;
		_lastStunTime = Time.time;
		_basicMoveScript.DoesMove = false;
	}

	private void RemoveStun()
	{
		_isStunned = false;
		_lastFireTime = Time.time;
		_basicMoveScript.DoesMove = true;
		_displacementLength += StunDuration * _basicMoveScript.MoveSpeed;
		OnDisplaced();
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
			Assert.IsNotNull(PlayerScript);
			bool playerGotHit = PlayerScript.PlayerGotHit();
			if (playerGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Player");
				collisionExists = true;
			}
		}
		else if (other.gameObject.tag == "Shield")
		{
			Assert.IsNotNull(PlayerScript);
			bool shieldGotHit = PlayerScript.ShieldGotHit();
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

			if (PlayerScript)
			{
				Vector3 playerPos = PlayerScript.transform.position;
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

	private void OnDisplaced()
	{
		if (PlayerScript && !_isDisplaced)
		{
			_isDisplaced = true;
			PlayerScript.EnemyGotDisplaced();
		}
	}

	protected virtual void ScoreAndRemoveFromScene()
	{
		//cash in the points
		if (PlayerScript)
		{
			int scoreAddition = (int)(Mathf.Abs(_displacementLength) * GameConstants.BaseScoreAddition);
			PlayerScript.TriggerEnemyDisplacement(scoreAddition);
			if (_isDisplaced)
			{
				PlayerScript.DisplacedEnemyGotDestroyed();
			}
		}
		Destroy(gameObject);
	}

	private void Explode()
	{
		if (PlayerScript)
		{
			//player might not be alive, game might have ended, do not score negative points in this case
			PlayerScript.TriggerEnemyDestruction();
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