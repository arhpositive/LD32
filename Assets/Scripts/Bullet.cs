﻿/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Bullet.cs
 * Handles bullets which have a speed and a direction.
 */

using UnityEngine;

public enum BulletType
{
	BtStun,
	BtSpeedup,
	BtTeleport,
	BtKiller
}

public class Bullet : MonoBehaviour
{
	public BulletType CurrentBulletType;
	public AudioClip BulletHitClip;
	public bool ShotByPlayer;
	private bool _hasCollided;
	private bool _destroyedByCollision;
	private StatsManager _statsManagerScript;
	private Player _playerScript;

	private void Start()
	{
		_hasCollided = false;
		_destroyedByCollision = false;
		_statsManagerScript = Camera.main.GetComponent<StatsManager>();
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		if (playerObject)
		{
			_playerScript = playerObject.GetComponent<Player>();
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (_hasCollided)
		{
			return;
		}

		if (other.gameObject.CompareTag("Enemy"))
		{
			BasicEnemy enemyScript = other.gameObject.GetComponent<BasicEnemy>();

			switch (CurrentBulletType)
			{
				case BulletType.BtStun:
					enemyScript.TriggerStun();
					break;
				case BulletType.BtSpeedup:
					enemyScript.TriggerSpeedBoost();
					break;
			}
			_destroyedByCollision = true;
			Hit();
		}
		else if (other.gameObject.CompareTag("Player"))
		{
			switch (CurrentBulletType)
			{
				case BulletType.BtKiller:
					bool playerGotHit = _playerScript.PlayerGotHit();
					if (playerGotHit)
					{
						EventLogger.PrintToLog("Bullet Collision v Player");
						Hit();
					}
					break;
			}
		}
		else if (other.gameObject.CompareTag("Shield"))
		{
			switch (CurrentBulletType)
			{
				case BulletType.BtKiller:
					bool shieldGotHit = _playerScript.ShieldGotHit();
					if (shieldGotHit)
					{
						EventLogger.PrintToLog("Bullet Collision v Shield");
						Hit();
					}
					break;
			}
		}
	}

	private void Hit()
	{
		_hasCollided = true;
		AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
		Destroy(gameObject);
	}

	private void OnDestroy()
	{
		if (ShotByPlayer && _statsManagerScript)
		{
			_statsManagerScript.BulletDestructionCoroutine(_destroyedByCollision);
		}
	}
}