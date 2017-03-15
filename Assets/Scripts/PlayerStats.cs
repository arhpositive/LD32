/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * PlayerStats.cs
 * Handles player stats
 */

using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerStats
{
	private bool _statsAreTemporary;
	private float _statDuration;

	private int _hitBulletCount;
	private int _shotBulletCount;
	private int _movementUpdateCount;

	//TODO NEXT we should hold several statistics such as:
	//TODO NEXT how often does the player manouver
		// calculate player total movement length over time

		// another stat to hold would be where the player hangs out on the screen
			// if they hang out in the right side, we can change that
			// if they hug the baseline we can change that aswell
			// we would try to split the screen into 6 or 9 sections, this should be enough
			
	// what types of weapons does the player use often

	//TODO flesh it out, add more parameters
	public float PlayerAccuracy { get; private set; }
	public int HealthDifference { get; private set; }
	public float MovementAmount { get; private set; }
	public Vector3 PlayerAveragePosition { get; private set; }

	public PlayerStats(bool statsAreTemporary, float statDuration = 0.0f)
	{
		_statsAreTemporary = statsAreTemporary;
		_statDuration = statDuration;
		_hitBulletCount = 0;
		_shotBulletCount = 0;
		_movementUpdateCount = 0;

		PlayerAccuracy = 0.0f;
		HealthDifference = 0;
		MovementAmount = 0.0f;
		PlayerAveragePosition = Vector3.zero;
	}

	public void OnBulletDestruction(bool bulletHitEnemy)
	{
		_shotBulletCount++;
		if (bulletHitEnemy)
		{
			_hitBulletCount++;
		}
		PlayerAccuracy = (float)_hitBulletCount / _shotBulletCount;
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
		Vector3 positionTotal = _movementUpdateCount * PlayerAveragePosition + playerPosition;

		MovementAmount += movementMagnitude;
		++_movementUpdateCount;

		PlayerAveragePosition = positionTotal / _movementUpdateCount;

		if (_statsAreTemporary)
		{
			yield return new WaitForSeconds(_statDuration);

			positionTotal = _movementUpdateCount * PlayerAveragePosition - playerPosition;

			MovementAmount -= movementMagnitude;
			--_movementUpdateCount;

			PlayerAveragePosition = positionTotal / _movementUpdateCount;
		}
	}
}
