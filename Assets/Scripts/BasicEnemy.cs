﻿/* 
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
	public GameObject BulletPrefab;

	public const float MoveSpeed = 1.2f;

	private GameObject _playerGameObject;
	private Player _playerScript;
	private DifficultyManager _difficultyManagerScript;
	private BasicMove _basicMoveScript;
	private BasicObject _basicObjectScript;

	private List<Vector3> _gunPositions;
	private bool _hasCollided;
	private bool _isStunned;
	private float _lastStunTime;
	private bool _speedBoostIsActive;
	private float _lastFireTime;
	private float _nextFiringInterval;
	private float _displacementLength; //used for scoring
	private bool _isDisplaced;

	private void Start()
	{
		_playerGameObject = GameObject.FindGameObjectWithTag("Player");
		if (_playerGameObject)
		{
			_playerScript = _playerGameObject.GetComponent<Player>();
		}
		_difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
		_basicMoveScript = gameObject.GetComponent<BasicMove>();
		_basicMoveScript.MoveSpeed = MoveSpeed;
		_basicObjectScript = gameObject.GetComponent<BasicObject>();

		InitGunPositions();
		_hasCollided = false;
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

			if (transform.position.x < GameConstants.HorizontalMinCoord)
			{
				//cash in the points
				if (_playerScript)
				{
					int scoreAddition = (int)(Mathf.Abs(_displacementLength) * GameConstants.BaseScoreAddition);
					_playerScript.TriggerEnemyDisplacement(scoreAddition);
					if (_isDisplaced)
					{
						_playerScript.DisplacedEnemyGotDestroyed();
					}
				}

				Destroy(gameObject);
			}
		}
	}

	public void TriggerStun()
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

	public void TriggerSpeedBoost()
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
		if (_hasCollided)
		{
			return;
		}

		if (other.gameObject.tag == "Player")
		{
			Assert.IsNotNull(_playerScript);
			bool playerGotHit = _playerScript.PlayerGotHit();
			if (playerGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Player");
				Explode();
			}
		}
		else if (other.gameObject.tag == "Shield")
		{
			Assert.IsNotNull(_playerScript);
			bool shieldGotHit = _playerScript.ShieldGotHit();
			if (shieldGotHit)
			{
				EventLogger.PrintToLog("Enemy Collision v Shield");
				Explode();
			}
		}
		else if (other.gameObject.tag == "Enemy")
		{
			EventLogger.PrintToLog("Enemy Collision v Enemy");
			Explode();
		}
	}

	private void FireGun()
	{
		_lastFireTime = Time.time;
		foreach (Vector3 localGunPos in _gunPositions)
		{
			Vector3 globalGunPos = transform.position + localGunPos;
			GameObject bulletGameObject = Instantiate(BulletPrefab, globalGunPos, Quaternion.identity);

			if (_playerScript)
			{
				Vector3 playerPos = _playerScript.transform.position;
				if (ShootsStraightAtPlayer && playerPos.x + 2.0f < globalGunPos.x)
				{
					//set bullet direction to match player direction, only if player is *comfortably* in front of the enemy
					Vector3 bulletDir = playerPos - globalGunPos;
					bulletGameObject.GetComponent<BasicMove>().SetMoveDir(bulletDir.normalized, true);
				}
			}
		}
		SetNextFiringInterval();
	}

	private void SetNextFiringInterval()
	{
		float randomIntervalCoef = Random.Range(MinFiringInterval, 2 * MinFiringInterval);
		_nextFiringInterval = randomIntervalCoef / _difficultyManagerScript.DifficultyCoefs[DifficultyParameter.DpShipFireRateIncrease];
	}

	private void OnDisplaced()
	{
		if (_playerScript && !_isDisplaced)
		{
			_isDisplaced = true;
			_playerScript.EnemyGotDisplaced();
		}
	}

	private void Explode()
	{
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