/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Assets.Scripts
{
    struct WaveEntity
    {
        public Vector2 Position;
        public Vector2 MoveDir;
        public bool IsWiggling;

        public WaveEntity(Vector2 position, Vector2 moveDir, bool isWiggling)
        {
            Position = position;
            MoveDir = moveDir;
            IsWiggling = isWiggling;
        }
    };

    public class SpawnManager : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject[] EnemyPrefabArray;
        public GameObject[] PowerupPrefabArray;
        public GameObject[] MeteorPrefabArray;
        public GameObject StarPrefab;
        public bool IsGameScene;
        public float WaveSpawnBaseInterval;
        public float PowerupSpawnBaseInterval;

        int _initialMeteorCount;
        int _initialStarCount;

        float _previousWaveSpawnTime;
        float _waveSpawnInterval;

        DifficultyManager _difficultyManagerScript;

        float _previousPowerupSpawnTime;
        float _powerupSpawnInterval;

        float _previousMeteorSpawnTime;
        const float MeteorSpawnInterval = 1.0f;

        float _previousStarSpawnTime;
        const float StarSpawnInterval = MeteorSpawnInterval / GameConstants.StarToMeteorRatio;

        List<List<WaveEntity>> _pregeneratedWaves;
        Vector2[] _enemyCoordinates;

        const float HMaxCoord = GameConstants.HorizontalMaxCoord;
        readonly float[] _hArr = { HMaxCoord - 0.9f, HMaxCoord - 0.6f, HMaxCoord - 0.3f, 
                                     HMaxCoord, HMaxCoord + 0.3f, HMaxCoord + 0.6f, HMaxCoord + 0.9f };
        readonly float[] _vArr = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

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

            _waveSpawnInterval = WaveSpawnBaseInterval;
            _previousWaveSpawnTime = Time.time;
            _pregeneratedWaves = new List<List<WaveEntity>>();

            PregenerateEnemyCoordinates();
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
                    SpawnNewWave();
                    _previousWaveSpawnTime = Time.time;
                    float randomIntervalCoef = Random.Range(WaveSpawnBaseInterval, WaveSpawnBaseInterval * 2);
                    _waveSpawnInterval = randomIntervalCoef / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
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
                Vector2 starPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 0.1f, GameConstants.HorizontalMaxCoord + 0.1f),
                    Random.Range(GameConstants.MinVerticalMovementLimit - 1.0f, GameConstants.MaxVerticalMovementLimit + 1.0f));
                SpawnStar(starPos);
                _previousStarSpawnTime = Time.time;
            }
        }

        void PregenerateEnemyCoordinates()
        {
            _enemyCoordinates = new Vector2[_hArr.Length * _vArr.Length];

            for (int x = 0; x < _hArr.Length; ++x)
            {
                for (int y = 0; y < _vArr.Length; ++y)
                {
                    int index = y + _vArr.Length * x;
                    _enemyCoordinates[index].x = _hArr[x];
                    _enemyCoordinates[index].y = _vArr[y];
                }
            }
        }

        Vector2 GetNewEnemyCoordinates(int x, int y)
        {
            return _enemyCoordinates[y + _vArr.Length * x];
        }

        void PregeneratePossibleWaves()
        {
            Vector2 leftAndDown = new Vector2(-1.0f, -0.5f);
            leftAndDown.Normalize();
            Vector2 leftAndUp = new Vector2(-1.0f, 0.5f);
            leftAndUp.Normalize();

            // 1 straight formation, straight movement
            List<WaveEntity> waveEntities = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(3, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 1), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 3), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 5), Vector2.left, false)
            };
            _pregeneratedWaves.Add(waveEntities);

            // 2 concave formation, straight movement
            List<WaveEntity> waveEntities2 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(2, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 1), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(4, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(4, 3), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 5), Vector2.left, false)
            };
            _pregeneratedWaves.Add(waveEntities2);

            // 3 convex formation, straight movement
            List<WaveEntity> waveEntities3 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(4, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 1), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 3), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(4, 5), Vector2.left, false)
            };
            _pregeneratedWaves.Add(waveEntities3);

            // 4 skewed formation, straight movement
            List<WaveEntity> waveEntities4 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(2, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 1), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 3), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(3, 5), Vector2.left, false)
            };
            _pregeneratedWaves.Add(waveEntities4);
            /*
            // 5 skewed formation, cross movement
            List<WaveEntity> waveEntities5 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(0, 0), leftAndDown, false),
                new WaveEntity(GetNewEnemyCoordinates(4, 1), leftAndDown, false),
                new WaveEntity(GetNewEnemyCoordinates(6, 4), leftAndUp, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 5), leftAndUp, false)
            };
            _pregeneratedWaves.Add(waveEntities5);

            //6 skewed formation, wiggling movement
            List<WaveEntity> waveEntities6 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(2, 0), leftAndDown, true),
                new WaveEntity(GetNewEnemyCoordinates(3, 1), leftAndUp, true),
                new WaveEntity(GetNewEnemyCoordinates(2, 2), leftAndDown, true),
                new WaveEntity(GetNewEnemyCoordinates(3, 3), leftAndUp, true),
                new WaveEntity(GetNewEnemyCoordinates(2, 4), leftAndDown, true),
                new WaveEntity(GetNewEnemyCoordinates(3, 5), leftAndUp, true)
            };
            _pregeneratedWaves.Add(waveEntities6);

            //7 skewed triple formation, wiggling singles, very hard!
            List<WaveEntity> waveEntities7 = new List<WaveEntity>
            {
                new WaveEntity(GetNewEnemyCoordinates(0, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(6, 0), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 1), leftAndDown, true),
                new WaveEntity(GetNewEnemyCoordinates(0, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(6, 2), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 3), leftAndUp, true),
                new WaveEntity(GetNewEnemyCoordinates(0, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(6, 4), Vector2.left, false),
                new WaveEntity(GetNewEnemyCoordinates(2, 5), leftAndDown, true)
            };
            _pregeneratedWaves.Add(waveEntities7);
             */
        }

        void SpawnNewWave()
        {
            EventLogger.PrintToLog("New Wave Spawn");
            
            float difficultyInterval = GameConstants.MaxDifficultyMultiplier - GameConstants.MinDifficultyMultiplier;

            float advancedEnemyPercentage = 
                ((_difficultyManagerScript.DifficultyMultiplier - GameConstants.MinDifficultyMultiplier) / 
                difficultyInterval) * 100.0f;

            int randomWaveIndex = Random.Range(0, _pregeneratedWaves.Count);
            List<WaveEntity> entities = _pregeneratedWaves[randomWaveIndex];
            for (int i = 0; i < entities.Count; i++)
            {
                float randomEnemy = Random.Range(0.0f, 100.0f);
                int enemyKind = randomEnemy < advancedEnemyPercentage ? 1 : 0;

                GameObject enemy = Instantiate(EnemyPrefabArray[enemyKind], entities[i].Position, Quaternion.identity) as GameObject;
                Assert.IsNotNull(enemy);
                BasicMove basicMoveScript = enemy.GetComponent<BasicMove>();

                if (entities[i].IsWiggling)
                {
                    basicMoveScript.SetBounceLimits(entities[i].Position.y - 0.1f, entities[i].Position.y + 0.1f);
                }
                basicMoveScript.SetMoveDir(entities[i].MoveDir);
            }
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
