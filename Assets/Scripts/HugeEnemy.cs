using UnityEngine;

public class HugeEnemy : BasicEnemy
{
	public float[] VerticalSpawnLimits;
	public float VerticalColliderBoundary;

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

	protected override void RemoveFromScene()
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
