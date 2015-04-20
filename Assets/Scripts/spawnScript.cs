/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * spawnScript.cs
 * Handles every entity spawn in the game, including the player, enemies and powerups
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

struct WaveEntity
{
    public Vector3 position_;
    public GameObject element_;

    public WaveEntity(Vector3 position, GameObject element)
    {
        position_ = position;
        element_ = element;
    }
};

public class spawnScript : MonoBehaviour
{
    public GameObject playerPrefab_;
    public GameObject[] enemyPrefabs_;
    public GameObject[] powerupPrefabs_;
    public GameObject[] meteorPrefabs_;
    public GameObject starPrefab_;
    public int initMeteorCount_;
    public int initStarCount_;
    public bool isGameScene_;

    public static float horizontalEnterCoord_ = 8.0f;
    public static float horizontalExitCoord_ = -1.0f;

    public static float spawnZCoord_ = 0.0f;

    float previousWaveSpawnTime_;
    float waveSpawnInterval_;
    int waveCount_;

    float difficultyMultiplier_;

    float previousPowerupSpawnTime_;
    float powerupSpawnInterval_;

    float previousMeteorSpawnTime_;
    float meteorSpawnInterval_ = 1.0f;

    float previousStarSpawnTime_;
    float starSpawnInterval_ = 100.0f;

    GameObject playerObject_;
    playerScript scriptPlayer_;

    int previousWavePlayerHealth_;

    float enemyHorizontalSpawnInterval_ = 0.3f;

    static float[] verticalSpawnPosArray_ = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

    static float[] fixedOffset1 = { -0.4f, 0.0f, 0.4f, 0.4f, 0.0f, -0.4f };
    static float[] fixedOffset2 = { -0.4f, 0.4f, -0.4f, 0.4f, -0.4f, 0.4f };
    static float[] fixedOffset3 = { 0.4f, 0.0f, -0.4f, -0.4f, 0.0f, 0.4f };
    static float[] fixedOffset4 = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    float[][] offsets = { fixedOffset1, fixedOffset2, fixedOffset3, fixedOffset4 };


    List<WaveEntity> nextWave_;

    void Awake()
    {
        if (isGameScene_)
        {
            playerObject_ = Instantiate(playerPrefab_,
            new Vector3(0.0f, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_),
            Quaternion.identity) as GameObject;

            scriptPlayer_ = playerObject_.GetComponent<playerScript>();
        }        
    }

    // Use this for initialization
    void Start()
    {
        difficultyMultiplier_ = 1.0f; //the bigger the harder

        waveCount_ = 0;
        waveSpawnInterval_ = 5.0f;
        previousWaveSpawnTime_ = Time.time;
        nextWave_ = new List<WaveEntity>();

        previousWavePlayerHealth_ = 3; //duplication

        powerupSpawnInterval_ = Random.Range(3.0f, 6.0f) * difficultyMultiplier_;
        previousPowerupSpawnTime_ = Time.time;
        
        previousMeteorSpawnTime_ = Time.time;
        previousStarSpawnTime_ = Time.time;
        InitialMeteorSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameScene_)
        {
            if (Time.time - previousWaveSpawnTime_ > waveSpawnInterval_)
            {
                FormNewWave();
                SpawnNewWave();
                nextWave_.Clear();
                previousWaveSpawnTime_ = Time.time;
                waveSpawnInterval_ = Random.Range(8.0f, 10.0f) / difficultyMultiplier_;
            }

            if (Time.time - previousPowerupSpawnTime_ > powerupSpawnInterval_)
            {
                SpawnNewPowerup();
                previousPowerupSpawnTime_ = Time.time;
                powerupSpawnInterval_ = Random.Range(3.0f, 6.0f) * difficultyMultiplier_;
            }
        }

        if (Time.time - previousMeteorSpawnTime_ > meteorSpawnInterval_)
        {
            int meteorKind = Random.Range(0, meteorPrefabs_.Length);
            Vector3 meteorPos = new Vector3(Random.Range(horizontalEnterCoord_ - 0.5f, horizontalEnterCoord_ + 0.5f), 
                Random.Range(playerScript.minVerticalMovementLimit_ - 1.0f, playerScript.maxVerticalMovementLimit_ + 1.0f), spawnZCoord_);
            SpawnMeteor(meteorKind, meteorPos);
            previousMeteorSpawnTime_ = Time.time;
        }

        if (Time.time - previousStarSpawnTime_ > starSpawnInterval_)
        {
            Vector3 starPos = new Vector3(Random.Range(horizontalEnterCoord_ - 0.1f, horizontalEnterCoord_ + 0.1f),
                Random.Range(playerScript.minVerticalMovementLimit_ - 1.0f, playerScript.maxVerticalMovementLimit_ + 1.0f), spawnZCoord_);
            SpawnStar(starPos);
            previousStarSpawnTime_ = Time.time;
        }
    }

    public float getDifficultyMultiplier()
    {
        return difficultyMultiplier_;
    }

    void IncreaseDifficulty(float value)
    {
        difficultyMultiplier_ = Mathf.Clamp(difficultyMultiplier_ + value, 0.1f, 2.0f);
    }

    void DecreaseDifficulty(float value)
    {
        difficultyMultiplier_ = Mathf.Clamp(difficultyMultiplier_ - value, 0.1f, 2.0f);
    }

    void FormNewWave()
    {
        float advancedEnemyPercentage = (Mathf.Clamp(difficultyMultiplier_, 1.0f, 2.0f) - 1.0f) * 100.0f;

        int randomOffset = Random.Range(0, offsets.Length);

        for (int i = 0; i < verticalSpawnPosArray_.Length; i++)
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

            WaveEntity entity = new WaveEntity(new Vector3(horizontalEnterCoord_ + offsets[randomOffset][i], verticalSpawnPosArray_[i], spawnZCoord_), enemyPrefabs_[enemyKind]);
            nextWave_.Add(entity);
        }
    }

    void SpawnNewWave()
    {
        for (int i = 0; i < nextWave_.Count; i++)
        {
            Instantiate(nextWave_[i].element_, nextWave_[i].position_, Quaternion.identity);
        }

        waveCount_++;

        if (previousWavePlayerHealth_ != scriptPlayer_.getPlayerHealth() && scriptPlayer_.getPlayerHealth() == 1)
        {
            DecreaseDifficulty(0.2f);
        }

        if (scriptPlayer_.getPlayerHealth() > 5 || (waveCount_ % 5 == 0 && scriptPlayer_.getPlayerHealth() > 1))
        {
            IncreaseDifficulty(0.2f);
        }

        previousWavePlayerHealth_ = scriptPlayer_.getPlayerHealth();
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
            if (scriptPlayer_.getHealth() == 1)
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

        Vector3 powerupPos = new Vector3(horizontalEnterCoord_, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_);
        Instantiate(powerupPrefabs_[powerupKind], powerupPos, Quaternion.identity);
    }

    void InitialMeteorSpawn()
    {
        for (int i = 0; i < initMeteorCount_; i++)
        {
            int meteorKind = Random.Range(0, meteorPrefabs_.Length);
            Vector3 meteorPos = new Vector3(Random.Range(playerScript.minHorizontalMovementLimit_ - 0.5f, horizontalEnterCoord_ - 0.5f),
                Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_);
            SpawnMeteor(meteorKind, meteorPos);
        }

        for (int i = 0; i < initStarCount_; i++)
        {
            Vector3 starPos = new Vector3(Random.Range(playerScript.minHorizontalMovementLimit_ - 0.5f, horizontalEnterCoord_ - 0.5f),
                Random.Range(playerScript.minVerticalMovementLimit_ - 1.0f, playerScript.maxVerticalMovementLimit_ + 1.0f), spawnZCoord_);
            SpawnStar(starPos);
        }
    }

    void SpawnMeteor(int meteorKind, Vector3 meteorPos)
    {
        GameObject meteor = Instantiate(meteorPrefabs_[meteorKind], meteorPos, Quaternion.identity) as GameObject;
        Vector2 direction = new Vector2(-1.0f, Random.Range(-1.0f, 1.0f));
        direction.Normalize();
        meteor.GetComponent<meteorScript>().setDirection(direction);
    }

    void SpawnStar(Vector3 starPos)
    {
        Instantiate(starPrefab_, starPos, Quaternion.identity);
    }
}
