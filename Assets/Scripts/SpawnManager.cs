/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    struct WaveEntity
    {
        public Vector2 Position;
        public Vector2 MoveDir;

        public WaveEntity(Vector2 position, Vector2 moveDir)
        {
            Position = position;
            MoveDir = moveDir;
        }
    };

    struct Formation
    {
        public List<WaveEntity> WaveEntities { get; private set; }
        public int HorizontalShipSpan { get; private set; }
        public bool RequiresShipwideHorizontalSpan { get; private set; }

        public Formation(List<WaveEntity> waveEntities, int horizontalShipSpan, bool requiresShipwideHorizontalSpan)
        {
            WaveEntities = waveEntities;
            HorizontalShipSpan = horizontalShipSpan;
            RequiresShipwideHorizontalSpan = requiresShipwideHorizontalSpan;
        }
    }

    public class SpawnManager : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject[] EnemyPrefabArray;
        public GameObject[] PowerupPrefabArray;
        public GameObject[] MeteorPrefabArray;
        public GameObject StarPrefab;
        public bool IsGameScene;
        public float MinWaveSpawnIntervalCoef;
        public float MaxWaveSpawnIntervalCoef;
        public float PowerupSpawnBaseInterval;
        public float EnemyMinVertDist;
        public float EnemyMaxVertDist;
        public float EnemyMinHorzDist;
        public float EnemyMaxHorzDist;

        int _initialMeteorCount;
        int _initialStarCount;

        float _previousWaveSpawnTime;
        float _waveSpawnInterval;

        DifficultyManager _difficultyManagerScript;

        float _previousPowerupSpawnTime;
        float _powerupSpawnInterval;

        //TODO NEXT for every star and meteor destroyed, just spawn another one, remove all timers and intervals
        float _previousMeteorSpawnTime;
        const float MeteorSpawnInterval = 1.0f;

        float _previousStarSpawnTime;
        const float StarSpawnInterval = MeteorSpawnInterval * GameConstants.StarToMeteorRatio; 

        List<Formation> _formations;
        
        const float HSpawnCoord = GameConstants.HorizontalMaxCoord;
        const float VMinCoord = GameConstants.MinVerticalMovementLimit;
        const float VMaxCoord = GameConstants.MaxVerticalMovementLimit;

        void Awake()
        {
	        if (!IsGameScene)
	        {
		        return;
	        }

	        Instantiate(PlayerPrefab,
		        new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit)),
		        Quaternion.identity);
        }

        void Start()
        {
            _difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();

            _initialMeteorCount = 30;
            _initialStarCount = _initialMeteorCount * GameConstants.StarToMeteorRatio;
            
            _waveSpawnInterval = MinWaveSpawnIntervalCoef;
            _previousWaveSpawnTime = Time.time;
            _formations = new List<Formation>();
            
            PregeneratePossibleWaves();

            _powerupSpawnInterval = PowerupSpawnBaseInterval;
            _previousPowerupSpawnTime = Time.time;

            _previousMeteorSpawnTime = Time.time;
            _previousStarSpawnTime = Time.time;

            InitialMeteorAndStarSpawn();
        }

        void Update()
        {
            if (IsGameScene)
            {
                if (Time.time - _previousWaveSpawnTime > _waveSpawnInterval)
                {
                    _previousWaveSpawnTime = Time.time;
                    float randomIntervalCoef = Random.Range(MinWaveSpawnIntervalCoef, MaxWaveSpawnIntervalCoef);
                    _waveSpawnInterval = randomIntervalCoef / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
                    //spawn new wave uses new _waveSpawnInterval to calculate spawn horizontal position, do not change the order here!
                    SpawnNewWave();
                }

                if (Time.time - _previousPowerupSpawnTime > _powerupSpawnInterval)
                {
                    SpawnNewPowerup();
                    _previousPowerupSpawnTime = Time.time;
                    float randomIntervalCoef = Random.Range(PowerupSpawnBaseInterval, PowerupSpawnBaseInterval * 2);
                    _powerupSpawnInterval = randomIntervalCoef * Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
                }
            }

            if (Time.time - _previousMeteorSpawnTime > MeteorSpawnInterval)
            {
                int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
                Vector2 meteorPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 1.0f, GameConstants.HorizontalMaxCoord),
                    Random.Range(GameConstants.VerticalMinCoord, GameConstants.VerticalMaxCoord));
                SpawnMeteor(meteorKind, meteorPos);
                _previousMeteorSpawnTime = Time.time;
            }

            if (Time.time - _previousStarSpawnTime > StarSpawnInterval)
            {
                Vector2 starPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 0.1f, GameConstants.HorizontalMaxCoord),
                    Random.Range(GameConstants.MinVerticalMovementLimit - 1.0f, GameConstants.MaxVerticalMovementLimit + 1.0f));
                SpawnStar(starPos);
                _previousStarSpawnTime = Time.time;
            }
        }

        void PregeneratePossibleWaves()
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
            _formations.Add(new Formation(straightLine, 0, false));

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
            _formations.Add(new Formation(echelonLine, 1, false));

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
            _formations.Add(new Formation(forwardsWedge, 4, false));
            
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
            _formations.Add(new Formation(backwardsWedge, 4, false));

            List<WaveEntity> phalanx = new List<WaveEntity>
            {
                //  8   9
                //  6   7
                //  4   5
                //  2   3
                //  0   1
                new WaveEntity(Vector2.zero, Vector2.left),
                new WaveEntity(new Vector2(1, 0), Vector2.left),
                new WaveEntity(new Vector2(0, 1), Vector2.left),
                new WaveEntity(new Vector2(1, 1), Vector2.left),
                new WaveEntity(new Vector2(0, 2), Vector2.left),
                new WaveEntity(new Vector2(1, 2), Vector2.left),
                new WaveEntity(new Vector2(0, 3), Vector2.left),
                new WaveEntity(new Vector2(1, 3), Vector2.left),
                new WaveEntity(new Vector2(0, 4), Vector2.left),
                new WaveEntity(new Vector2(1, 4), Vector2.left),
                new WaveEntity(new Vector2(0, 5), Vector2.left),
                new WaveEntity(new Vector2(1, 5), Vector2.left),
                new WaveEntity(new Vector2(0, 6), Vector2.left),
                new WaveEntity(new Vector2(1, 6), Vector2.left),
                new WaveEntity(new Vector2(0, 7), Vector2.left),
                new WaveEntity(new Vector2(1, 7), Vector2.left)
            };
            _formations.Add(new Formation(phalanx, 1, true));
        }

        //Generate new waves and spawn them on scene
        void SpawnNewWave()
        {
            EventLogger.PrintToLog("New Wave Spawn");

            // 1. determine number of enemies

            //TODO low difficulty = wider spread & less enemies
            //TODO high difficulty = shorter spread & more enemies

            //based on spread, determine max number of enemies vertically
            //determine number of enemies, low number is fixed (tied to max. spread aswell), high number is calculated based on spread
            //based on number of enemies and spread, starting point has a min and max vertically, randomize between constraints

            int randomWaveIndex = Random.Range(0, _formations.Count);
            
            //adjustments are being made for formations which require more than one column of enemies
            float minEnemyHorizontalDist = EnemyMinHorzDist;
            if (_formations[randomWaveIndex].RequiresShipwideHorizontalSpan)
            {
                //we're assuming that width and height of an enemy ship is approx. the same
                //we might want to improve this with real, calculated width and height values for each ship
                minEnemyHorizontalDist = Mathf.Min(EnemyMinVertDist, EnemyMaxHorzDist);
            }

            float nextWaveHorizontalDistance = _waveSpawnInterval * BasicEnemy.MoveSpeed;
            float maxEnemyHorizontalDist = nextWaveHorizontalDistance - EnemyMaxHorzDist;
            if (_formations[randomWaveIndex].HorizontalShipSpan > 1)
            {
                maxEnemyHorizontalDist /= _formations[randomWaveIndex].HorizontalShipSpan;
            }
            maxEnemyHorizontalDist = Mathf.Clamp(maxEnemyHorizontalDist, EnemyMinHorzDist, EnemyMaxHorzDist);
            print(maxEnemyHorizontalDist);

            //I. Determine Spread
            float enemyVerticalDist = Random.Range(EnemyMinVertDist, EnemyMaxVertDist);
            float enemyHorizontalDist = Random.Range(minEnemyHorizontalDist, maxEnemyHorizontalDist);

            //II. Determine Number of Enemies
            float verticalMovementLength = GameConstants.MaxVerticalMovementLimit - GameConstants.MinVerticalMovementLimit;

            //TODO these counts only make sense for single line formations, phalanx formation breaks these rules
            int verticalIntervalCount = Mathf.FloorToInt(verticalMovementLength/enemyVerticalDist);

            int enemyMaxCount = Mathf.Min(1 + verticalIntervalCount, _formations[randomWaveIndex].WaveEntities.Count);
            int enemyMinCount = Mathf.Max(2, enemyMaxCount - 6);
            int enemyCount = Random.Range(enemyMinCount, enemyMaxCount);

            int verticalSpanCount = enemyCount;
            if (_formations[randomWaveIndex].RequiresShipwideHorizontalSpan)
            {
                verticalSpanCount = Mathf.CeilToInt(verticalSpanCount / 2);
            }
            verticalSpanCount -= 1;
            float maxVerticalStartCoord = VMaxCoord - (verticalSpanCount * enemyVerticalDist);

            //III. Select Enemies From Formation List
            List<WaveEntity> selectedFormationEntities = new List<WaveEntity>();
            for (int i = 0; i < enemyCount; ++i)
            {
                selectedFormationEntities.Add(_formations[randomWaveIndex].WaveEntities[i]);
            }
            selectedFormationEntities.Sort(FormationComparison);

            //IV. Determine Advanced Enemy Percentage
            float difficultyInterval = GameConstants.MaxDifficultyMultiplier - GameConstants.MinDifficultyMultiplier;
            float advancedEnemyPercentage =
                ((_difficultyManagerScript.DifficultyMultiplier - GameConstants.MinDifficultyMultiplier) / 
                difficultyInterval) * 100.0f;

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
                    
                    int xIncrement = (xPosDiff != 0) ? Math.Sign(xPosDiff) : 0;
                    int yIncrement = (yPosDiff != 0) ? Math.Sign(yPosDiff) : 0;

                    enemyPos = new Vector2(previousEnemyPos.x + xIncrement * enemyHorizontalDist, previousEnemyPos.y + yIncrement * enemyVerticalDist);
                }
                else
                {
                    enemyPos = new Vector2(HSpawnCoord + (selectedFormationEntities[i].Position.x * maxEnemyHorizontalDist), Random.Range(VMinCoord, maxVerticalStartCoord));
                    print("EnemyPos: " + enemyPos);
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

        void SpawnNewPowerup()
        {
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

        void InitialMeteorAndStarSpawn()
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

        void SpawnMeteor(int meteorKind, Vector2 meteorPos)
        {
            Instantiate(MeteorPrefabArray[meteorKind], meteorPos, Quaternion.identity);
        }

        void SpawnStar(Vector2 starPos)
        {
            Instantiate(StarPrefab, starPos, Quaternion.identity);
        }
    }
}
