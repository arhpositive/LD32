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
        public int InitialMeteorCount; //TODO calculate ideal meteor and star count for having a seamless sky
        public int InitialStarCount;
        public bool IsGameScene;

        float _previousWaveSpawnTime;
        float _waveSpawnInterval;
        int _waveCount;

        DifficultyManager _difficultyManagerScript;

        float _previousPowerupSpawnTime;
        float _powerupSpawnInterval;

        float _previousMeteorSpawnTime;
        const float MeteorSpawnInterval = 1.0f;

        float _previousStarSpawnTime;
        const float StarSpawnInterval = 100.0f;

        GameObject _playerGameObject;
        Player _playerScript;

        int _previousWavePlayerHealth;

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

	        _playerGameObject = Instantiate(PlayerPrefab,
		        new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit)),
		        Quaternion.identity) as GameObject;

            Assert.IsNotNull(_playerGameObject);
            _playerScript = _playerGameObject.GetComponent<Player>();
        }

        void Start()
        {
            _difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();

            _waveCount = 0;
            _waveSpawnInterval = 5.0f;
            _previousWaveSpawnTime = Time.time;
            _pregeneratedWaves = new List<List<WaveEntity>>();

            PregenerateEnemyCoordinates();
            PregeneratePossibleWaves();

            _previousWavePlayerHealth = GameConstants.PlayerInitialHealth;

            _powerupSpawnInterval = Random.Range(3.0f, 6.0f);
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
                    AdjustDifficultyAfterWave();
                    _previousWaveSpawnTime = Time.time;
                    _waveSpawnInterval = Random.Range(8.0f, 10.0f) / _difficultyManagerScript.DifficultyMultiplier;
                }

                if (Time.time - _previousPowerupSpawnTime > _powerupSpawnInterval)
                {
                    SpawnNewPowerup();
                    _previousPowerupSpawnTime = Time.time;
                    _powerupSpawnInterval = Random.Range(3.0f, 6.0f) * _difficultyManagerScript.DifficultyMultiplier;
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
        }

        void SpawnNewWave()
        {
            float advancedEnemyPercentage = (Mathf.Clamp(_difficultyManagerScript.DifficultyMultiplier, 1.0f, 2.0f) - 1.0f) * 100.0f;

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
            _waveCount++;
        }

        void SpawnNewPowerup()
        {
            int powerupKind;
            float randomizePowerup = Random.Range(0.0f, 100.0f);

            if (randomizePowerup < 5.0f)
            {
                powerupKind = (int)PowerupType.PtHealth;
            }
            else if (randomizePowerup < 35.0f)
            {
                powerupKind = (int)PowerupType.PtSpeedup;
            }
            else if (randomizePowerup < 50.0f)
            {
                powerupKind = (int)PowerupType.PtShield;
            }
            else
            {
                powerupKind = (int)PowerupType.PtResearch;
            }

            // TODO spawning powerups are dependent upon order of the array, fix
            Vector3 powerupPos = new Vector2(GameConstants.HorizontalMaxCoord, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit));
            Instantiate(PowerupPrefabArray[powerupKind], powerupPos, Quaternion.identity);
        }

        void InitialMeteorAndStarSpawn()
        {
            for (int i = 0; i < InitialMeteorCount; i++)
            {
                int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
                Vector2 meteorPos = new Vector2(Random.Range(GameConstants.MinHorizontalMovementLimit - 0.5f, GameConstants.HorizontalMaxCoord - 0.5f),
                    Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit));
                SpawnMeteor(meteorKind, meteorPos);
            }

            for (int i = 0; i < InitialStarCount; i++)
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

	    void AdjustDifficultyAfterWave()
	    {
            if (_previousWavePlayerHealth != _playerScript.PlayerHealth && _playerScript.PlayerHealth == 1)
            {
                _difficultyManagerScript.DecreaseDifficulty(0.2f);
            }

            if (_playerScript.PlayerHealth > 5 || (_waveCount % 5 == 0 && _playerScript.PlayerHealth > 1))
            {
                _difficultyManagerScript.IncreaseDifficulty(0.2f);
            }

            _previousWavePlayerHealth = _playerScript.PlayerHealth;
	    }
    }
}
