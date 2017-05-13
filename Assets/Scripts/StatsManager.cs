/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * StatsManager.cs
 * Holds All Time Player Stats and related information
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
	public List<PlayerStats> AllPlayerStats { get; private set; }

	private const float ShortTermHealthChangeInterval = 10.0f;
	private const float LongTermHealthChangeInterval = 30.0f;

	// Use this for initialization
	void Start ()
	{
		AllPlayerStats = new List<PlayerStats>
		{
			new PlayerStats(true, ShortTermHealthChangeInterval),
			new PlayerStats(true, LongTermHealthChangeInterval),
			new PlayerStats(false)
		};
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void StartFireGunCoroutine(GunType typeOfGun)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator fireGunCoroutine = ps.OnBulletInit(typeOfGun);
			StartCoroutine(fireGunCoroutine);
		}
	}

	public void StartMovementCoroutine(Vector3 clampedPlayerPosition, float movementMagnitude)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator playerMovementCoroutine = ps.OnPlayerMovement(clampedPlayerPosition, movementMagnitude);
			StartCoroutine(playerMovementCoroutine);
		}
	}

	public void HealthChangeCoroutine(int healthChange)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator playerHealthChangeCoroutine = ps.OnPlayerHealthChange(healthChange);
			StartCoroutine(playerHealthChangeCoroutine); //start coroutine for stats
		}
	}

	public void BulletDestructionCoroutine(bool bulletHitEnemy)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator bulletDestructionCoroutine = ps.OnBulletDestruction(bulletHitEnemy);
			StartCoroutine(bulletDestructionCoroutine);
		}
	}

	public void PickupPowerupCoroutine(PowerupType typeOfPowerup)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			IEnumerator pickupPowerupCoroutine = ps.OnPowerupPickup(typeOfPowerup);
			StartCoroutine(pickupPowerupCoroutine);
		}
	}

	public void OnWaveDestruction(int waveBaseScore)
	{
		foreach (PlayerStats ps in AllPlayerStats)
		{
			ps.OnWaveDestruction(waveBaseScore);
		}
	}

	public PlayerStats GetAllTimeStats()
	{
		return AllPlayerStats[0];
	}
}
