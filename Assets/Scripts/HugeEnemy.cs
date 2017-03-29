using UnityEngine;
using UnityEngine.Assertions;

public class HugeEnemy : BasicEnemy
{
	//TODO work hard play hard work hard play hard

	private SpawnManager _spawnManagerScript;

	protected override void Start()
	{
		base.Start();
		_spawnManagerScript = Camera.main.GetComponent<SpawnManager>();
	}

	public override void TriggerStun()
	{
		
	}

	public override void TriggerSpeedBoost()
	{
		
	}

	protected override void ScoreAndRemoveFromScene()
	{
		_spawnManagerScript.ResetVerticalSpawnLimits();
		_spawnManagerScript.SetHugeEnemyExists(false);
		Destroy(gameObject);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		CheckForCollision(other);
	}
}
