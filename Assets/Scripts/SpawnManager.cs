/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    struct WaveEntity
    {
        public Vector3 Position;
        public GameObject WaveObject;

        public WaveEntity(Vector3 position, GameObject waveObject)
        {
            Position = position;
            WaveObject = waveObject;
        }
    };

    public class SpawnManager : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        public GameObject[] EnemyPrefabArray;
        public GameObject[] PowerupPrefabArray;
        public GameObject[] MeteorPrefabArray;
        public GameObject StarPrefab;
        public int InitialMeteorCount;
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

        readonly static float[] VerticalSpawnPositionsArray = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

        readonly static float[] HorizontalSpawnOffset1 = { -0.4f, 0.0f, 0.4f, 0.4f, 0.0f, -0.4f };
        readonly static float[] HorizontalSpawnOffset2 = { -0.4f, 0.4f, -0.4f, 0.4f, -0.4f, 0.4f };
        readonly static float[] HorizontalSpawnOffset3 = { 0.4f, 0.0f, -0.4f, -0.4f, 0.0f, 0.4f };
        readonly static float[] HorizontalSpawnOffset4 = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        readonly static float[][] HorizontalSpawnOffsets = { HorizontalSpawnOffset1, HorizontalSpawnOffset2, HorizontalSpawnOffset3, HorizontalSpawnOffset4 };

        List<WaveEntity> _nextWave;

        void Awake()
        {
            if (IsGameScene)
            {
                _playerGameObject = Instantiate(PlayerPrefab,
                new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit)),
                Quaternion.identity) as GameObject;

                _playerScript = _playerGameObject.GetComponent<Player>();
            }
        }

        void Start()
        {
            _difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();

            _waveCount = 0;
            _waveSpawnInterval = 5.0f;
            _previousWaveSpawnTime = Time.time;
            _nextWave = new List<WaveEntity>();

            _previousWavePlayerHealth = 3; //duplication

            _powerupSpawnInterval = Random.Range(3.0f, 6.0f);
            _previousPowerupSpawnTime = Time.time;

            _previousMeteorSpawnTime = Time.time;
            _previousStarSpawnTime = Time.time;
            InitialMeteorSpawn();
        }

        void Update()
        {
            if (IsGameScene)
            {
                if (Time.time - _previousWaveSpawnTime > _waveSpawnInterval)
                {
                    FormNewWave();
                    SpawnNewWave();
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



        void FormNewWave()
        {
            float advancedEnemyPercentage = (Mathf.Clamp(_difficultyManagerScript.DifficultyMultiplier, 1.0f, 2.0f) - 1.0f) * 100.0f;

            int randomOffset = Random.Range(0, HorizontalSpawnOffsets.Length);

            for (int i = 0; i < VerticalSpawnPositionsArray.Length; i++)
            {
                float randomEnemy = Random.Range(0.0f, 100.0f);
                int enemyKind = randomEnemy < advancedEnemyPercentage ? 1 : 0;

                WaveEntity entity = new WaveEntity(new Vector2(GameConstants.HorizontalMaxCoord + HorizontalSpawnOffsets[randomOffset][i], VerticalSpawnPositionsArray[i]), EnemyPrefabArray[enemyKind]);
                _nextWave.Add(entity);
            }
        }

        void SpawnNewWave()
        {
            for (int i = 0; i < _nextWave.Count; i++)
            {
                Instantiate(_nextWave[i].WaveObject, _nextWave[i].Position, Quaternion.identity);
            }

            _waveCount++;

            if (_previousWavePlayerHealth != _playerScript.PlayerHealth && _playerScript.PlayerHealth == 1)
            {
                _difficultyManagerScript.DecreaseDifficulty(0.2f);
            }

            if (_playerScript.PlayerHealth > 5 || (_waveCount % 5 == 0 && _playerScript.PlayerHealth > 1))
            {
                _difficultyManagerScript.IncreaseDifficulty(0.2f);
            }

            _previousWavePlayerHealth = _playerScript.PlayerHealth;
            _nextWave.Clear();
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
                //TODO change this line and make it dependent upon difficulty settings
                if (_playerScript.PlayerHealth == 1)
                {
                    powerupKind = (int)PowerupType.PtShield;
                }
                else
                {
                    powerupKind = (int)PowerupType.PtSpeedup;
                }
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

        void InitialMeteorSpawn()
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
    }
}
