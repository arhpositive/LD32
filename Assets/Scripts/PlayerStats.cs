/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * PlayerStats.cs
 * Handles player stats
 */

using System.Collections;
using UnityEngine;

public class PlayerStats
{
	private bool _statsAreTemporary;
	private float _statDuration;

	private int _hitBulletCount;
	private int _destroyedBulletCount;
	private int _totalShotBulletCount;
	private int[] _shotBulletCountsPerGun;
	private int _pickedUpPowerupCount;
	private int[] _pickupCountsPerPowerup;
	private int _movementUpdateCount;

	//TODO NEXT flesh it out, add more parameters
	public float PlayerAccuracy { get; private set; }
	public int HealthDifference { get; private set; }
	public float MovementAmount { get; private set; }
	public Vector3 PlayerAveragePosition { get; private set; }
	public float[] GunUsageFrequencies { get; private set; }
	public float[] PowerupPickupFrequencies { get; private set; }
	

	public PlayerStats(bool statsAreTemporary, float statDuration = 0.0f)
	{
		_statsAreTemporary = statsAreTemporary;
		_statDuration = statDuration;
		_hitBulletCount = 0;
		_destroyedBulletCount = 0;
		_totalShotBulletCount = 0;
		_shotBulletCountsPerGun = new int[(int)GunType.GtCount];
		_pickedUpPowerupCount = 0;
		_pickupCountsPerPowerup = new int[(int)PowerupType.PtCount];
		_movementUpdateCount = 0;

		PlayerAccuracy = 0.0f;
		HealthDifference = 0;
		MovementAmount = 0.0f;
		PlayerAveragePosition = Vector3.zero;
		GunUsageFrequencies = new float[(int) GunType.GtCount];
		PowerupPickupFrequencies = new float[(int)PowerupType.PtCount];
	}

	public IEnumerator OnBulletInit(GunType gunType)
	{
		++_shotBulletCountsPerGun[(int) gunType];
		++_totalShotBulletCount;
		CalculateGunUsageFrequencies();
		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);
			--_shotBulletCountsPerGun[(int)gunType];
			--_totalShotBulletCount;
			CalculateGunUsageFrequencies();
		}
	}

	public IEnumerator OnBulletDestruction(bool bulletHitEnemy)
	{
		++_destroyedBulletCount;
		if (bulletHitEnemy)
		{
			++_hitBulletCount;
		}
		PlayerAccuracy = (float)_hitBulletCount / _destroyedBulletCount;

		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);
			--_destroyedBulletCount;
			if (bulletHitEnemy)
			{
				--_hitBulletCount;
			}
			PlayerAccuracy = (float) _hitBulletCount / _destroyedBulletCount;
		}
	}

	public IEnumerator OnPowerupPickup(PowerupType powerupType)
	{
		++_pickupCountsPerPowerup[(int)powerupType];
		++_pickedUpPowerupCount;
		CalculatePowerupPickupFrequencies();
		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);
			--_pickupCountsPerPowerup[(int)powerupType];
			--_pickedUpPowerupCount;
			CalculatePowerupPickupFrequencies();
		}
	}

	public IEnumerator OnPlayerHealthChange(int difference)
	{
		HealthDifference += difference;
		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);
			HealthDifference -= difference;
		}
	}

	public IEnumerator OnPlayerMovement(Vector3 playerPosition, float movementMagnitude)
	{
		CalculateAveragePosition(playerPosition, movementMagnitude, false);

		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);
			CalculateAveragePosition(playerPosition, movementMagnitude, true);
		}
	}

	private void CalculateAveragePosition(Vector3 playerPosition, float movementMagnitude, bool revertChanges)
	{
		int multiplier = revertChanges ? -1 : 1;

		Vector3 positionTotal = _movementUpdateCount * PlayerAveragePosition + (playerPosition * multiplier);

		MovementAmount += movementMagnitude * multiplier;
		_movementUpdateCount = _movementUpdateCount + multiplier;

		PlayerAveragePosition = positionTotal / _movementUpdateCount;
	}

	private void CalculateGunUsageFrequencies()
	{
		for (int i = 0; i < _shotBulletCountsPerGun.Length; ++i)
		{
			GunUsageFrequencies[i] = (float)_shotBulletCountsPerGun[i] / _totalShotBulletCount;
		}
	}

	private void CalculatePowerupPickupFrequencies()
	{
		for (int i = 0; i < _pickupCountsPerPowerup.Length; ++i)
		{
			PowerupPickupFrequencies[i] = (float)_pickupCountsPerPowerup[i] / _pickedUpPowerupCount;
		}
	}
}
