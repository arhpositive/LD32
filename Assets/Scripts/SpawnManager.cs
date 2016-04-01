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

    [Header("Prefabs")]
    public GameObject PlayerPrefab;
    public GameObject[] EnemyPrefabArray;
    public GameObject[] PowerupPrefabArray;
    public GameObject[] MeteorPrefabArray;
    public GameObject StarPrefab;
        
    [Header("Interval Properties")]
    public float MinWaveSpawnIntervalCoef;
    public float MaxWaveSpawnIntervalCoef;
    public float PowerupSpawnBaseInterval;

    private DifficultyManager _difficultyManagerScript;

    private float _previousWaveSpawnTime;
    private float _waveSpawnInterval;

    private float _previousPowerupSpawnTime;
    private float _powerupSpawnInterval;

    //TODO LATER for every star and meteor destroyed, just spawn another one, remove all timers and intervals
    private int _initialMeteorCount;
    private float _previousMeteorSpawnTime;
    private float _meteorSpawnInterval;

    private int _initialStarCount;
    private float _previousStarSpawnTime;
    private float _starSpawnInterval;

    private List<Formation> _formations;

    private const float ShipColliderVertSize = 0.46f;
    private const float ShipGameObjectVertSize = 0.5f;
    private const float PlayerShipColliderHorzSize = 0.38f;

    private float _enemySpawnMinVertDist;
    private float _enemySpawnMaxVertDist;
    private float _enemySpawnMinHorzDist;
    private float _enemySpawnMaxHorzDist;

    private const float HSpawnCoord = GameConstants.HorizontalMaxCoord;
    private const float VMinCoord = GameConstants.MinVerticalMovementLimit;
    private const float VMaxCoord = GameConstants.MaxVerticalMovementLimit;

    private void Awake()
    {
        if (!IsGameScene)
        {
            return;
        }
            
        //instantiate player
        Instantiate(PlayerPrefab,
            new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit)),
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

        _initialMeteorCount = 30;
        _initialStarCount = _initialMeteorCount * GameConstants.StarToMeteorRatio;
            
        _waveSpawnInterval = MinWaveSpawnIntervalCoef;
        _previousWaveSpawnTime = Time.time;
        _formations = new List<Formation>();
            
        PregeneratePossibleWaves();

        _powerupSpawnInterval = PowerupSpawnBaseInterval;
        _previousPowerupSpawnTime = Time.time;

        _meteorSpawnInterval = 1.0f;
        _previousMeteorSpawnTime = Time.time;

        float meteorToStarSpeedRatio = 
            MeteorPrefabArray[0].GetComponent<BasicMove>().MoveSpeed / StarPrefab.GetComponent<BasicMove>().MoveSpeed;
        _starSpawnInterval = (_meteorSpawnInterval / GameConstants.StarToMeteorRatio) * meteorToStarSpeedRatio;
        _previousStarSpawnTime = Time.time;

        InitialMeteorAndStarSpawn();
    }

    private void Update()
    {
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

            if (Time.time - _previousPowerupSpawnTime > _powerupSpawnInterval)
            {
                SpawnNewPowerup();
                _previousPowerupSpawnTime = Time.time;
            }
        }

        if (Time.time - _previousMeteorSpawnTime > _meteorSpawnInterval)
        {
            int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
            Vector2 meteorPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 1.0f, GameConstants.HorizontalMaxCoord),
                Random.Range(GameConstants.VerticalMinCoord, GameConstants.VerticalMaxCoord));
            SpawnMeteor(meteorKind, meteorPos);
            _previousMeteorSpawnTime = Time.time;
        }

        if (Time.time - _previousStarSpawnTime > _starSpawnInterval)
        {
            Vector2 starPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 0.1f, GameConstants.HorizontalMaxCoord),
                Random.Range(GameConstants.MinVerticalMovementLimit - 1.0f, GameConstants.MaxVerticalMovementLimit + 1.0f));
            SpawnStar(starPos);
            _previousStarSpawnTime = Time.time;
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
        _waveSpawnInterval = randomIntervalCoef / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
        
        bool hasNoExit = Random.Range(0.0f, 100.0f) < 70.0f * Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);

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
        float verticalMovementLength = VMaxCoord - VMinCoord;
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
        int lowerIntervalCount = Mathf.FloorToInt((verticalMovementLength - ShipColliderVertSize)/enemyVerticalDist);
        int higherIntervalCount = Mathf.FloorToInt(verticalMovementLength/enemyVerticalDist);

        int maxPossibleVerticalIntervalCount = (lowerIntervalCount == higherIntervalCount) && !hasNoExit
            ? lowerIntervalCount
            : lowerIntervalCount + 1;

        float distBetweenFirstAndLastShip = enemyVerticalDist*maxPossibleVerticalIntervalCount;
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
            int enemyMinCount = enemyMaxCount - 1;
            enemyCount = Random.Range(enemyMinCount, enemyMaxCount);
        }

        int actualVerticalIntervalCount = enemyCount - 1;
        float minVerticalStartCoord = VMinCoord;
        float maxVerticalStartCoord = VMaxCoord - actualVerticalIntervalCount*enemyVerticalDist;

        if (maxVerticalStartCoord < minVerticalStartCoord)
        {
            //we just went off the line, this is only possible for no exit formations!
            Assert.IsTrue(hasNoExit);

            float difference = minVerticalStartCoord - maxVerticalStartCoord;
            maxVerticalStartCoord = minVerticalStartCoord + difference;
            minVerticalStartCoord = maxVerticalStartCoord;
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

        //VI. Determine Advanced Enemy Percentage
        float difficultyInterval = GameConstants.MaxDifficultyMultiplier - GameConstants.MinDifficultyMultiplier;
        float advancedEnemyPercentage =
            ((_difficultyManagerScript.DifficultyMultiplier - GameConstants.MinDifficultyMultiplier) / 
             difficultyInterval) * 100.0f;

        //VII. Spawn Enemies
        Vector2 previousEnemyPos = Vector2.zero;
        for (int i = 0; i < selectedFormationEntities.Count; i++)
        {
            float randomEnemy = Random.Range(0.0f, 100.0f);
            int enemyKind = randomEnemy < advancedEnemyPercentage ? 1 : 0;

            Vector2 enemyPos;
            if (i > 0)
            {
                Vector2 posDiff = selectedFormationEntities[i].Position - selectedFormationEntities[i - 1].Position;

                int xPosDiff = (int) posDiff.x;
                int yPosDiff = (int) posDiff.y;
                    
                int xIncrement = xPosDiff != 0 ? Math.Sign(xPosDiff) : 0;
                int yIncrement = yPosDiff != 0 ? Math.Sign(yPosDiff) : 0;

                enemyPos = new Vector2(previousEnemyPos.x + xIncrement * enemyHorizontalDist, previousEnemyPos.y + yIncrement * enemyVerticalDist);
            }
            else
            {
                enemyPos = new Vector2(HSpawnCoord + selectedFormationEntities[i].Position.x * maxEnemyHorizontalDist, Random.Range(minVerticalStartCoord, maxVerticalStartCoord));
            }

            GameObject enemy = Instantiate(EnemyPrefabArray[enemyKind], enemyPos, Quaternion.identity) as GameObject;
            Assert.IsNotNull(enemy);
            BasicMove basicMoveScript = enemy.GetComponent<BasicMove>();

            basicMoveScript.SetMoveDir(selectedFormationEntities[i].MoveDir);

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

    private void SpawnNewPowerup()
    {
        float randomIntervalCoef = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
        _powerupSpawnInterval = randomIntervalCoef * Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);

        float currentDifficulty = _difficultyManagerScript.DifficultyMultiplier;

        int powerupCount = PowerupPrefabArray.Length;

        float[] occurenceArray = new float[powerupCount];
        float totalOccurence = 0.0f;

        for (int i = 0; i < powerupCount; ++i)
        {
            Powerup powerupScript = PowerupPrefabArray[i].GetComponent<Powerup>();
            if (powerupScript.IsNegativePowerup)
            {
                occurenceArray[i] = powerupScript.PowerupOccurence * currentDifficulty;
            }
            else
            {
                occurenceArray[i] = powerupScript.PowerupOccurence / currentDifficulty;
            }
            totalOccurence += occurenceArray[i];
        }

        float randomOccurence = Random.Range(0.0f, totalOccurence);
        float currentOccurence = 0.0f;

        int j = 0;
        while (j < powerupCount)
        {
            currentOccurence += occurenceArray[j];
            if (randomOccurence < currentOccurence)
            {
                break;
            }
            ++j;
        }
        Vector3 powerupPos = new Vector2(GameConstants.HorizontalMaxCoord, 
            Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit));
        Instantiate(PowerupPrefabArray[j], powerupPos, Quaternion.identity);
    }

    private void InitialMeteorAndStarSpawn()
    {
        for (int i = 0; i < _initialMeteorCount; i++)
        {
            int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
            Vector2 meteorPos = new Vector2(Random.Range(GameConstants.MinHorizontalMovementLimit - 0.5f, GameConstants.HorizontalMaxCoord - 0.5f),
                Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit));
            SpawnMeteor(meteorKind, meteorPos);
        }

        for (int i = 0; i < _initialStarCount; i++)
        {
            Vector2 starPos = new Vector2(Random.Range(GameConstants.MinHorizontalMovementLimit - 0.5f, GameConstants.HorizontalMaxCoord - 0.5f),
                Random.Range(GameConstants.MinVerticalMovementLimit - 1.0f, GameConstants.MaxVerticalMovementLimit + 1.0f));
            SpawnStar(starPos);
        }
    }

    private void SpawnMeteor(int meteorKind, Vector2 meteorPos)
    {
        Instantiate(MeteorPrefabArray[meteorKind], meteorPos, Quaternion.identity);
    }

    private void SpawnStar(Vector2 starPos)
    {
        Instantiate(StarPrefab, starPos, Quaternion.identity);
    }
}