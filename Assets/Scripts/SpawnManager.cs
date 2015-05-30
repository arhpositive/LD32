/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * SpawnManager.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    float PreviousWaveSpawnTime;
    float WaveSpawnInterval;
    int WaveCount;

    public float DifficultyMultiplier { get; private set; }

    float PreviousPowerupSpawnTime;
    float PowerupSpawnInterval;

    float PreviousMeteorSpawnTime;
    float MeteorSpawnInterval = 1.0f;

    float PreviousStarSpawnTime;
    float StarSpawnInterval = 100.0f;

    GameObject PlayerGameObject;
    Player PlayerScript;

    int PreviousWavePlayerHealth;

    static float[] VerticalSpawnPositionsArray = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

    static float[] HorizontalSpawnOffset1 = { -0.4f, 0.0f, 0.4f, 0.4f, 0.0f, -0.4f };
    static float[] HorizontalSpawnOffset2 = { -0.4f, 0.4f, -0.4f, 0.4f, -0.4f, 0.4f };
    static float[] HorizontalSpawnOffset3 = { 0.4f, 0.0f, -0.4f, -0.4f, 0.0f, 0.4f };
    static float[] HorizontalSpawnOffset4 = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    float[][] HorizontalSpawnOffsets = { HorizontalSpawnOffset1, HorizontalSpawnOffset2, HorizontalSpawnOffset3, HorizontalSpawnOffset4 };

    List<WaveEntity> NextWave;

    void Awake()
    {
        if (IsGameScene)
        {
            PlayerGameObject = Instantiate(PlayerPrefab,
            new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit)),
            Quaternion.identity) as GameObject;

            PlayerScript = PlayerGameObject.GetComponent<Player>();
        }        
    }

    void Start()
    {
        DifficultyMultiplier = 1.0f; //the bigger the harder

        WaveCount = 0;
        WaveSpawnInterval = 5.0f;
        PreviousWaveSpawnTime = Time.time;
        NextWave = new List<WaveEntity>();

        PreviousWavePlayerHealth = 3; //duplication

        PowerupSpawnInterval = Random.Range(3.0f, 6.0f) * DifficultyMultiplier;
        PreviousPowerupSpawnTime = Time.time;
        
        PreviousMeteorSpawnTime = Time.time;
        PreviousStarSpawnTime = Time.time;
        InitialMeteorSpawn();
    }

    void Update()
    {
        if (IsGameScene)
        {
            if (Time.time - PreviousWaveSpawnTime > WaveSpawnInterval)
            {
                FormNewWave();
                SpawnNewWave();
                NextWave.Clear();
                PreviousWaveSpawnTime = Time.time;
                WaveSpawnInterval = Random.Range(8.0f, 10.0f) / DifficultyMultiplier;
            }

            if (Time.time - PreviousPowerupSpawnTime > PowerupSpawnInterval)
            {
                SpawnNewPowerup();
                PreviousPowerupSpawnTime = Time.time;
                PowerupSpawnInterval = Random.Range(3.0f, 6.0f) * DifficultyMultiplier;
            }
        }

        if (Time.time - PreviousMeteorSpawnTime > MeteorSpawnInterval)
        {
            int meteorKind = Random.Range(0, MeteorPrefabArray.Length);
            Vector2 meteorPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 1.0f, GameConstants.HorizontalMaxCoord),
                Random.Range(GameConstants.VerticalMinCoord, GameConstants.VerticalMaxCoord));
            SpawnMeteor(meteorKind, meteorPos);
            PreviousMeteorSpawnTime = Time.time;
        }

        if (Time.time - PreviousStarSpawnTime > StarSpawnInterval)
        {
            Vector2 starPos = new Vector2(Random.Range(GameConstants.HorizontalMaxCoord - 0.1f, GameConstants.HorizontalMaxCoord + 0.1f),
                Random.Range(GameConstants.MinVerticalMovementLimit - 1.0f, GameConstants.MaxVerticalMovementLimit + 1.0f));
            SpawnStar(starPos);
            PreviousStarSpawnTime = Time.time;
        }
    }

    void IncreaseDifficulty(float value)
    {
        DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier + value, 0.1f, 2.0f);
    }

    void DecreaseDifficulty(float value)
    {
        DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier - value, 0.1f, 2.0f);
    }

    void FormNewWave()
    {
        float advancedEnemyPercentage = (Mathf.Clamp(DifficultyMultiplier, 1.0f, 2.0f) - 1.0f) * 100.0f;

        int randomOffset = Random.Range(0, HorizontalSpawnOffsets.Length);

        for (int i = 0; i < VerticalSpawnPositionsArray.Length; i++)
        {
            float randomEnemy = Random.Range(0.0f, 100.0f);
            int enemyKind = 0;

            if (randomEnemy < advancedEnemyPercentage)
            {
                enemyKind = 1;
            }
            else
            {
                enemyKind = 0;
            }

            WaveEntity entity = new WaveEntity(new Vector2(GameConstants.HorizontalMaxCoord + HorizontalSpawnOffsets[randomOffset][i], VerticalSpawnPositionsArray[i]), EnemyPrefabArray[enemyKind]);
            NextWave.Add(entity);
        }
    }

    void SpawnNewWave()
    {
        for (int i = 0; i < NextWave.Count; i++)
        {
            Instantiate(NextWave[i].WaveObject, NextWave[i].Position, Quaternion.identity);
        }

        WaveCount++;

        if (PreviousWavePlayerHealth != PlayerScript.PlayerHealth && PlayerScript.PlayerHealth == 1)
        {
            DecreaseDifficulty(0.2f);
        }

        if (PlayerScript.PlayerHealth > 5 || (WaveCount % 5 == 0 && PlayerScript.PlayerHealth > 1))
        {
            IncreaseDifficulty(0.2f);
        }

        PreviousWavePlayerHealth = PlayerScript.PlayerHealth;
    }

    void SpawnNewPowerup()
    {
        int powerupKind = 0;
        float randomizePowerup = Random.Range(0.0f, 100.0f);

        if (randomizePowerup < 5.0f)
        {
            powerupKind = (int)PowerupType.pt_health;
        }
        else if (randomizePowerup < 35.0f)
        {
            if (PlayerScript.PlayerHealth == 1)
            {
                powerupKind = (int)PowerupType.pt_health;
            }
            else
            {
                powerupKind = (int)PowerupType.pt_speedup;
            }            
        }
        else
        {
            powerupKind = (int)PowerupType.pt_research;
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
