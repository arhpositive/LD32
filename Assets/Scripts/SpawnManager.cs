/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

internal struct WaveEntity
{
	public Vector2 Position;
	public Vector2 MoveDir;

	public WaveEntity(Vector2 position, Vector2 moveDir)
	{
		Position = position;
		MoveDir = moveDir;
	}
};

internal class Formation
{
	public List<WaveEntity> WaveEntities { get; private set; }
	public int HorizontalShipSpan { get; private set; }

	public Formation(List<WaveEntity> waveEntities, int horizontalShipSpan)
	{
		WaveEntities = waveEntities;
		HorizontalShipSpan = horizontalShipSpan;
	}
}

public class SpawnManager : MonoBehaviour
{
	public bool IsGameScene;

	//TODO temporary
	public GameObject UpBound;
	public GameObject DownBound;

	[Header("Prefabs")]
	public GameObject PlayerPrefab;
	public GameObject[] EnemyPrefabArray;
	public GameObject[] HugeEnemyPrefabArray;
	public GameObject[] PosPowerupPrefabArray;
	public GameObject[] NegPowerupPrefabArray;
	public GameObject[] MeteorPrefabArray;
	public GameObject StarPrefab;

	[Header("Interval Properties")]
	public float MinWaveSpawnIntervalCoef;
	public float MaxWaveSpawnIntervalCoef;
	public float MinHugeEnemySpawnIntervalCoef;
	public float MaxHugeEnemySpawnIntervalCoef;
	public float PowerupSpawnBaseInterval;

	[Header("Parallax Counts")]
	public int MeteorCount;
	public int StarCount;

	private DifficultyManager _difficultyManagerScript;

	private float _previousWaveSpawnTime;
	private float _waveSpawnInterval;
	private float _previousHugeEnemySpawnTime;
	private float _hugeEnemySpawnInterval;

	private float _previousPosPowerupSpawnTime;
	private float _posPowerupSpawnInterval;
	private float _previousNegPowerupSpawnTime;
	private float _negPowerupSpawnInterval;

	private List<Formation> _formations;

	private const float ShipColliderVertSize = 0.46f;
	private const float ShipGameObjectVertSize = 0.5f;
	private const float PlayerShipColliderHorzSize = 0.38f;

	private float _enemySpawnMinVertDist;
	private float _enemySpawnMaxVertDist;
	private float _enemySpawnMinHorzDist;
	private float _enemySpawnMaxHorzDist;

	private float _vertMinShipSpawnCoord;
	private float _vertMaxShipSpawnCoord;

	private bool _hugeEnemyExists;

	private void Awake()
	{
		if (!IsGameScene)
		{
			return;
		}

		//instantiate player
		Instantiate(PlayerPrefab,
			new Vector2(0.0f, Random.Range(Player.MinVerticalMovementLimit, Player.MaxVerticalMovementLimit)),
			Quaternion.identity);
	}

	private void Start()
	{
		Time.timeScale = 1.35f;
		_difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();

		_enemySpawnMaxVertDist = ShipColliderVertSize * 2.0f - 0.01f;
		_enemySpawnMinVertDist = Mathf.Min(ShipGameObjectVertSize + 0.05f, _enemySpawnMaxVertDist);
		_enemySpawnMaxHorzDist = PlayerShipColliderHorzSize * 2.0f - 0.01f;
		_enemySpawnMinHorzDist = Mathf.Min(PlayerShipColliderHorzSize * 0.5f, _enemySpawnMaxHorzDist);

		_hugeEnemyExists = false;
		ResetVerticalSpawnLimits();

		_waveSpawnInterval = MinWaveSpawnIntervalCoef;
		_previousWaveSpawnTime = Time.time;
		_hugeEnemySpawnInterval = MinHugeEnemySpawnIntervalCoef;
		_previousHugeEnemySpawnTime = Time.time;
		_formations = new List<Formation>();

		PregeneratePossibleWaves();

		_posPowerupSpawnInterval = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
		_previousPosPowerupSpawnTime = Time.time;

		_negPowerupSpawnInterval = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
		_previousNegPowerupSpawnTime = Time.time;

		//_meteorSpawnInterval = 1.0f;
		//_previousMeteorSpawnTime = Time.time;

		//float meteorToStarSpeedRatio = 
		//    MeteorPrefabArray[0].GetComponent<BasicMove>().MoveSpeed / StarPrefab.GetComponent<BasicMove>().MoveSpeed;
		//_starSpawnInterval = (_meteorSpawnInterval / GameConstants.StarToMeteorRatio) * meteorToStarSpeedRatio;
		//_previousStarSpawnTime = Time.time;

		InitialMeteorAndStarSpawn();
	}

	private void Update()
	{
		UpBound.transform.position = new Vector3(UpBound.transform.position.x, _vertMaxShipSpawnCoord, UpBound.transform.position.z);
		DownBound.transform.position = new Vector3(DownBound.transform.position.x, _vertMinShipSpawnCoord, DownBound.transform.position.z);


		if (Input.GetKeyDown(KeyCode.U))
		{
			Time.timeScale += 0.1f;
			print("Timescale Up: " + Time.timeScale);
		}
		else if (Input.GetKeyDown(KeyCode.J))
		{
			Time.timeScale -= 0.1f;
			print("Timescale Down: " + Time.timeScale);
		}

		if (IsGameScene)
		{
			if (Time.time - _previousWaveSpawnTime > _waveSpawnInterval)
			{
				SpawnNewWave();
				_previousWaveSpawnTime = Time.time;
			}

			if (Time.time - _previousHugeEnemySpawnTime > _hugeEnemySpawnInterval)
			{
				SpawnNewHugeEnemy();
				_previousHugeEnemySpawnTime = Time.time;
			}

			if (Time.time - _previousPosPowerupSpawnTime > _posPowerupSpawnInterval)
			{
				SpawnNewPowerup(true);
				_previousPosPowerupSpawnTime = Time.time;
			}

			if (Time.time - _previousNegPowerupSpawnTime > _negPowerupSpawnInterval)
			{
				SpawnNewPowerup(false);
				_previousNegPowerupSpawnTime = Time.time;
			}
		}
	}

	private void PregeneratePossibleWaves()
	{
		// TODO LATER include different movement patterns, might involve waypoints, etc.
		// waypoint system could make the wave change movement direction after a given amount of time.
		// be careful about randomizing too much as it will make us lose control over certain difficulty features

		Vector2 leftAndDown = new Vector2(-1.0f, -0.5f);
		leftAndDown.Normalize();
		Vector2 leftAndUp = new Vector2(-1.0f, 0.5f);
		leftAndUp.Normalize();

		List<WaveEntity> straightLine = new List<WaveEntity>
		{
			//  5
			//  4
			//  3
			//  2
			//  1
			//  0
			new WaveEntity(Vector2.zero, Vector2.left),
			new WaveEntity(new Vector2(0, 1), Vector2.left),
			new WaveEntity(new Vector2(0, 2), Vector2.left),
			new WaveEntity(new Vector2(0, 3), Vector2.left),
			new WaveEntity(new Vector2(0, 4), Vector2.left),
			new WaveEntity(new Vector2(0, 5), Vector2.left),
			new WaveEntity(new Vector2(0, 6), Vector2.left),
			new WaveEntity(new Vector2(0, 7), Vector2.left),
			new WaveEntity(new Vector2(0, 8), Vector2.left),
			new WaveEntity(new Vector2(0, 9), Vector2.left)
		};
		_formations.Add(new Formation(straightLine, 0));

		List<WaveEntity> echelonLine = new List<WaveEntity>()
		{
			//      5
			//  4
			//      3
			//  2
			//      1
			//  0
			new WaveEntity(Vector2.zero, Vector2.left),
			new WaveEntity(new Vector2(1, 1), Vector2.left),
			new WaveEntity(new Vector2(0, 2), Vector2.left),
			new WaveEntity(new Vector2(1, 3), Vector2.left),
			new WaveEntity(new Vector2(0, 4), Vector2.left),
			new WaveEntity(new Vector2(1, 5), Vector2.left),
			new WaveEntity(new Vector2(0, 6), Vector2.left),
			new WaveEntity(new Vector2(1, 7), Vector2.left),
			new WaveEntity(new Vector2(0, 8), Vector2.left),
			new WaveEntity(new Vector2(1, 9), Vector2.left)
		};
		_formations.Add(new Formation(echelonLine, 1));

		List<WaveEntity> forwardsWedge = new List<WaveEntity>
		{
			//          1
			//      3
			//  5
			//  4
			//      2
			//          0
			new WaveEntity(new Vector2(4, 0), Vector2.left),
			new WaveEntity(new Vector2(4, 9), Vector2.left),
			new WaveEntity(new Vector2(3, 1), Vector2.left),
			new WaveEntity(new Vector2(3, 8), Vector2.left),
			new WaveEntity(new Vector2(2, 2), Vector2.left),
			new WaveEntity(new Vector2(2, 7), Vector2.left),
			new WaveEntity(new Vector2(1, 3), Vector2.left),
			new WaveEntity(new Vector2(1, 6), Vector2.left),
			new WaveEntity(new Vector2(0, 4), Vector2.left),
			new WaveEntity(new Vector2(0, 5), Vector2.left)
		};
		_formations.Add(new Formation(forwardsWedge, 4));

		List<WaveEntity> backwardsWedge = new List<WaveEntity>
		{
			//  1
			//      3
			//          5
			//          4
			//      2
			//  0
			new WaveEntity(Vector2.zero, Vector2.left),
			new WaveEntity(new Vector2(0, 9), Vector2.left),
			new WaveEntity(new Vector2(1, 1), Vector2.left),
			new WaveEntity(new Vector2(1, 8), Vector2.left),
			new WaveEntity(new Vector2(2, 2), Vector2.left),
			new WaveEntity(new Vector2(2, 7), Vector2.left),
			new WaveEntity(new Vector2(3, 3), Vector2.left),
			new WaveEntity(new Vector2(3, 6), Vector2.left),
			new WaveEntity(new Vector2(4, 4), Vector2.left),
			new WaveEntity(new Vector2(4, 5), Vector2.left)
		};
		_formations.Add(new Formation(backwardsWedge, 4));
	}

	//Generate new waves and spawn them on scene
	private void SpawnNewWave()
	{
		EventLogger.PrintToLog("New Wave Spawn");

		float randomIntervalCoef = Random.Range(MinWaveSpawnIntervalCoef, MaxWaveSpawnIntervalCoef);
		_waveSpawnInterval = randomIntervalCoef / _difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpWaveSpawnRateIncrease);


		int randRange = 100;
		float stepSize = (float)randRange/GameConstants.DifficultyStepCount;

		float noExitProbability = _difficultyManagerScript.DifficultyCoefs[DifficultyParameter.DpWaveHasNoExitCoef] * stepSize - stepSize * 0.5f;

		bool hasNoExit = Random.Range(0, randRange) < noExitProbability;

		//TODO low difficulty = wider spread & less enemies
		//TODO high difficulty = shorter spread & more enemies

		//I. Pick a random formation type
		int randomWaveIndex = Random.Range(0, _formations.Count);

		//II. Determine Horizontal Distance Between Enemies
		float nextWaveHorizontalDistance = _waveSpawnInterval * BasicEnemy.MoveSpeed;
		float maxEnemyHorizontalDist = nextWaveHorizontalDistance - _enemySpawnMaxHorzDist;
		if (_formations[randomWaveIndex].HorizontalShipSpan > 1)
		{
			maxEnemyHorizontalDist /= _formations[randomWaveIndex].HorizontalShipSpan;
		}

		float enemyHorizontalDist;
		if (maxEnemyHorizontalDist < _enemySpawnMinHorzDist)
		{
			enemyHorizontalDist = maxEnemyHorizontalDist;
		}
		else
		{
			maxEnemyHorizontalDist = Mathf.Clamp(maxEnemyHorizontalDist, _enemySpawnMinHorzDist, _enemySpawnMaxHorzDist);
			enemyHorizontalDist = Random.Range(_enemySpawnMinHorzDist, maxEnemyHorizontalDist);
		}

		//III. Determine Vertical Distance Between Enemies
		float verticalMovementLength = _vertMaxShipSpawnCoord - _vertMinShipSpawnCoord;
		float minEnemyVerticalDist = _enemySpawnMinVertDist;
		if (hasNoExit)
		{
			int maxIntervalCount = _formations[randomWaveIndex].WaveEntities.Count - 1;

			float minVerticalDistance = (verticalMovementLength - ShipColliderVertSize) / maxIntervalCount;
			if (minVerticalDistance > minEnemyVerticalDist)
			{
				minEnemyVerticalDist = minVerticalDistance;
			}
		}
		float enemyVerticalDist = Random.Range(minEnemyVerticalDist, _enemySpawnMaxVertDist);

		//IV. Determine Number of Enemies
		int lowerIntervalCount = Mathf.FloorToInt((verticalMovementLength - ShipColliderVertSize) / enemyVerticalDist);
		int higherIntervalCount = Mathf.FloorToInt(verticalMovementLength / enemyVerticalDist);

		int maxPossibleVerticalIntervalCount = (lowerIntervalCount == higherIntervalCount) && !hasNoExit
			? lowerIntervalCount
			: lowerIntervalCount + 1;

		float distBetweenFirstAndLastShip = enemyVerticalDist * maxPossibleVerticalIntervalCount;
		Assert.IsTrue(!hasNoExit || (distBetweenFirstAndLastShip >= verticalMovementLength - ShipColliderVertSize));

		int maxPossibleShipCount = maxPossibleVerticalIntervalCount + 1;
		int enemyCount;
		if (hasNoExit)
		{
			enemyCount = maxPossibleShipCount;
		}
		else
		{
			//no possible no-exits here!
			int enemyMaxCount = Mathf.Min(maxPossibleShipCount - 1, _formations[randomWaveIndex].WaveEntities.Count);
			enemyCount = Random.Range(enemyMaxCount - 2, enemyMaxCount);
		}

		int actualVerticalIntervalCount = enemyCount - 1;
		float minVerticalStartCoord = _vertMinShipSpawnCoord;
		float maxVerticalStartCoord = _vertMaxShipSpawnCoord - actualVerticalIntervalCount * enemyVerticalDist;

		if (maxVerticalStartCoord < minVerticalStartCoord)
		{
			//we just went off the line, this is only possible for no exit formations!
			Assert.IsTrue(hasNoExit);
			
			//swap these two
			maxVerticalStartCoord += minVerticalStartCoord;
			minVerticalStartCoord = maxVerticalStartCoord - minVerticalStartCoord;
			maxVerticalStartCoord -= minVerticalStartCoord;

			if (_hugeEnemyExists)
			{
				if (_vertMinShipSpawnCoord != Player.MinVerticalMovementLimit)
				{
					minVerticalStartCoord = maxVerticalStartCoord;
				}
				else if (_vertMaxShipSpawnCoord != Player.MaxVerticalMovementLimit)
				{
					maxVerticalStartCoord = minVerticalStartCoord;
				}
				else
				{
					Assert.IsTrue(false); //something is fishy, spawning a huge enemy didn't change vertical spawn coords
				}
			}
		}
		else
		{
			Assert.IsTrue(distBetweenFirstAndLastShip <= verticalMovementLength);
		}

		//V. Select Enemies From Formation List
		List<WaveEntity> selectedFormationEntities = new List<WaveEntity>();
		for (int i = 0; i < enemyCount; ++i)
		{
			selectedFormationEntities.Add(_formations[randomWaveIndex].WaveEntities[i]);
		}
		selectedFormationEntities.Sort(FormationComparison);

		//VI. Determine Advanced Enemy Count
		int enemyTypeCount = EnemyPrefabArray.Length;
		int[] enemyTypeSteps = new int[enemyTypeCount];
		for (int i = 0; i < enemyTypeSteps.Length; ++i)
		{
			enemyTypeSteps[i] = Mathf.RoundToInt(i * 100.0f / (enemyTypeSteps.Length - 1));
			//enemyTypeSteps = {0, 100} for 2 enemies, {0, 50, 100} for 3 enemies, {0, 33, 67, 100} for 4 enemies, and so on
		}

		float advancedEnemyPercentage = _difficultyManagerScript.DifficultyCoefs[DifficultyParameter.DpEnemyShipStrength] * stepSize - stepSize * 0.5f;
		
		int advEnemyTypeIndex = 1;
		float percentageOfStrongerEnemy = 0.0f;

		if (enemyTypeSteps.Length > 1)
		{
			int currentEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex];
			while (advancedEnemyPercentage > currentEnemyTypeStep)
			{
				++advEnemyTypeIndex;
				Assert.IsTrue(advEnemyTypeIndex < enemyTypeSteps.Length);
				currentEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex];
			}
			// if we're here, we know which two enemies we're gonna use
			int previousEnemyTypeStep = enemyTypeSteps[advEnemyTypeIndex - 1];
			percentageOfStrongerEnemy = (advancedEnemyPercentage - previousEnemyTypeStep) / (currentEnemyTypeStep - previousEnemyTypeStep);
		}

		int minAdvancedEnemyCount = Mathf.FloorToInt(percentageOfStrongerEnemy * selectedFormationEntities.Count);
		int maxAdvancedEnemyCount = Mathf.CeilToInt(percentageOfStrongerEnemy * selectedFormationEntities.Count);
		int advancedEnemyCount = Random.Range(minAdvancedEnemyCount, maxAdvancedEnemyCount + 1);

		//create ship types list
		int[] shipTypes = new int[selectedFormationEntities.Count];
		for (int i = 0; i < shipTypes.Length; ++i)
		{
			if (i < advancedEnemyCount)
			{
				shipTypes[i] = advEnemyTypeIndex;
			}
			else
			{
				shipTypes[i] = advEnemyTypeIndex - 1;
			}
		}

		//shuffle the list
		for (int i = 0; i < shipTypes.Length; ++i)
		{
			int temp = shipTypes[i];
			int randomIndex = Random.Range(i, shipTypes.Length);
			shipTypes[i] = shipTypes[randomIndex];
			shipTypes[randomIndex] = temp;
		}

		//VII. Spawn Enemies
		Vector2 previousEnemyPos = Vector2.zero;
		for (int i = 0; i < selectedFormationEntities.Count; i++)
		{
			int enemyKind = shipTypes[i];
			GameObject enemyPrefab = EnemyPrefabArray[enemyKind];
			BasicEnemy enemyPrefabScript = enemyPrefab.GetComponent<BasicEnemy>();

			Vector2 enemyPos;
			if (i > 0)
			{
				Vector2 posDiff = selectedFormationEntities[i].Position - selectedFormationEntities[i - 1].Position;

				int xPosDiff = (int)posDiff.x;
				int yPosDiff = (int)posDiff.y;

				int xIncrement = xPosDiff != 0 ? Math.Sign(xPosDiff) : 0;
				int yIncrement = yPosDiff != 0 ? Math.Sign(yPosDiff) : 0;

				enemyPos = new Vector2(previousEnemyPos.x + xIncrement * enemyHorizontalDist, previousEnemyPos.y + yIncrement * enemyVerticalDist);
			}
			else
			{
				enemyPos = new Vector2(enemyPrefabScript.HorizontalSpawnCoord + selectedFormationEntities[i].Position.x * maxEnemyHorizontalDist, 
					Random.Range(minVerticalStartCoord, maxVerticalStartCoord));
			}

			GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
			Assert.IsNotNull(enemy);
			BasicMove basicMoveScript = enemy.GetComponent<BasicMove>();

			//TODO LATER this might be completely unnecessary, but then again we might need it in the future
			basicMoveScript.SetMoveDir(selectedFormationEntities[i].MoveDir, false);

			previousEnemyPos = enemyPos;
		}
	}

	private int FormationComparison(WaveEntity entity1, WaveEntity entity2)
	{
		if (entity1.Position.y < entity2.Position.y)
		{
			return -1;
		}
		if (entity1.Position.y > entity2.Position.y)
		{
			return 1;
		}
		return 0;
	}

	private void SpawnNewHugeEnemy()
	{
		Assert.IsTrue(!_hugeEnemyExists); //we only want one huge enemy at once on the screen
		EventLogger.PrintToLog("New Huge Enemy Spawn");
		ResetVerticalSpawnLimits();

		float randomIntervalCoef = Random.Range(MinHugeEnemySpawnIntervalCoef, MaxHugeEnemySpawnIntervalCoef);
		_hugeEnemySpawnInterval = randomIntervalCoef / _difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpWaveSpawnRateIncrease); 

		//randomly select a prefab among available options
		int hugeEnemyIndex = Random.Range(0, HugeEnemyPrefabArray.Length);
		GameObject hugeEnemyPrefab = HugeEnemyPrefabArray[hugeEnemyIndex];
		HugeEnemy hugeEnemyScript = hugeEnemyPrefab.GetComponent<HugeEnemy>();

		//TODO as difficulty is increased, huge enemies should be closer
		Vector3 hugeEnemyPos = new Vector2(hugeEnemyScript.HorizontalSpawnCoord, Random.Range(hugeEnemyScript.VerticalSpawnLimits[0], hugeEnemyScript.VerticalSpawnLimits[1]));
		Instantiate(hugeEnemyPrefab, hugeEnemyPos, Quaternion.identity);
		
		float colliderBoundary = hugeEnemyScript.VerticalColliderBoundary;
		if (Mathf.Sign(colliderBoundary) == 1.0f)
		{
			//positive collider boundary means we're dealing with a huge enemy below, hence we should limit vMin
			_vertMinShipSpawnCoord = hugeEnemyPos.y + colliderBoundary + ShipColliderVertSize + 0.01f;
		}
		else
		{
			//negative collider boundary = huge enemy above = vMax
			_vertMaxShipSpawnCoord = hugeEnemyPos.y + colliderBoundary - ShipColliderVertSize - 0.01f;
		}

		SetHugeEnemyExists(true);
	}

	private void SpawnNewPowerup(bool isPositive)
	{
		float randomIntervalCoef = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);

		GameObject[] powerupPrefabArray;
		if (isPositive)
		{
			_posPowerupSpawnInterval = randomIntervalCoef * _difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpPosPowerupSpawnRateDecrease);
			powerupPrefabArray = PosPowerupPrefabArray;
		}
		else
		{
			_negPowerupSpawnInterval = randomIntervalCoef / _difficultyManagerScript.GetDifficultyMultiplier(DifficultyParameter.DpNegPowerupSpawnRateIncrease);
			powerupPrefabArray = NegPowerupPrefabArray;
		}

		List<int> occurenceList = new List<int>();
		int powerupCount = powerupPrefabArray.Length;
		for (int i = 0; i < powerupCount; ++i)
		{
			int powerupOccurence = powerupPrefabArray[i].GetComponent<Powerup>().PowerupOccurence;
			for (int j = 0; j < powerupOccurence; ++j)
			{
				occurenceList.Add(i);
			}
		}

		int powerupIndex = Random.Range(0, occurenceList.Count);
		GameObject selectedPowerup = powerupPrefabArray[occurenceList[powerupIndex]];
		BasicMove powerupMoveScript = selectedPowerup.GetComponent<BasicMove>();
		Vector3 powerupPos = new Vector2(powerupMoveScript.HorizontalLimits[1], Random.Range(powerupMoveScript.VerticalLimits[0], powerupMoveScript.VerticalLimits[1]));
		Instantiate(selectedPowerup, powerupPos, Quaternion.identity);
	}

	private void InitialMeteorAndStarSpawn()
	{
		for (int i = 0; i < MeteorCount; i++)
		{
			int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
			GameObject selectedMeteor = MeteorPrefabArray[meteorKind];
			BasicMove meteorMoveScript = selectedMeteor.GetComponent<BasicMove>();
			
			Vector2 meteorPos =
				new Vector2(Random.Range(meteorMoveScript.HorizontalLimits[0], meteorMoveScript.HorizontalLimits[1]),
					Random.Range(meteorMoveScript.VerticalLimits[0], meteorMoveScript.VerticalLimits[1]));
			Instantiate(selectedMeteor, meteorPos, Quaternion.identity);
		}

		for (int i = 0; i < StarCount; i++)
		{
			BasicMove starMoveScript = StarPrefab.GetComponent<BasicMove>();

			Vector2 starPos =
				new Vector2(Random.Range(starMoveScript.HorizontalLimits[0], starMoveScript.HorizontalLimits[1]),
					Random.Range(starMoveScript.VerticalLimits[0], starMoveScript.VerticalLimits[1]));
			Instantiate(StarPrefab, starPos, Quaternion.identity);
		}
	}
	
	public void SetHugeEnemyExists(bool newValue)
	{
		_hugeEnemyExists = newValue;
	}

	public void ResetVerticalSpawnLimits()
	{
		_vertMinShipSpawnCoord = Player.MinVerticalMovementLimit;
		_vertMaxShipSpawnCoord = Player.MaxVerticalMovementLimit;
	}

	public float GetVertMinShipSpawnCoord()
	{
		return _vertMinShipSpawnCoord;
	}

	public float GetVertMaxShipSpawnCoord()
	{
		return _vertMaxShipSpawnCoord;
	}
}
