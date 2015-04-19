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

    public static float horizontalEnterCoord_ = 8.0f;
    public static float horizontalExitCoord_ = -1.0f;

    public static float spawnZCoord_ = 0.0f;

    float previousWaveSpawnTime_;
    float waveSpawnInterval_ = 8.0f;

    float previousPowerupSpawnTime_;
    float powerupSpawnInterval_ = 2.0f;

    float previousMeteorSpawnTime_;
    float meteorSpawnInterval_ = 1.0f;

    float previousStarSpawnTime_;
    float starSpawnInterval_ = 100.0f;

    //float enemyHorizontalSpawnInterval_ = 0.9f; //TODO_ARHAN open up later

    static float[] verticalSpawnPosArray_ = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

    List<WaveEntity> nextWave_;

    void Awake()
    {
        Instantiate(playerPrefab_,
            new Vector3(0.0f, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_),
            Quaternion.identity);
    }

    // Use this for initialization
    void Start()
    {
        previousWaveSpawnTime_ = Time.time - (waveSpawnInterval_ * 0.5f);
        previousPowerupSpawnTime_ = Time.time;
        previousMeteorSpawnTime_ = Time.time;
        previousStarSpawnTime_ = Time.time;
        InitialMeteorSpawn();
        nextWave_ = new List<WaveEntity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - previousWaveSpawnTime_ > waveSpawnInterval_)
        {
            FormNewWave();
            SpawnNewWave();
            nextWave_.Clear();
            previousWaveSpawnTime_ = Time.time;
        }

        if (Time.time - previousPowerupSpawnTime_ > powerupSpawnInterval_)
        {
            SpawnNewPowerup();
            previousPowerupSpawnTime_ = Time.time;
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

    void FormNewWave()
    {
        for (int i = 0; i < verticalSpawnPosArray_.Length; i++)
        {
            int enemyKind = Random.Range(0, enemyPrefabs_.Length);
            WaveEntity entity = new WaveEntity(new Vector3(horizontalEnterCoord_, verticalSpawnPosArray_[i], spawnZCoord_), enemyPrefabs_[enemyKind]);
            nextWave_.Add(entity);
        }
    }

    void SpawnNewWave()
    {
        for (int i = 0; i < nextWave_.Count; i++)
        {
            Instantiate(nextWave_[i].element_, nextWave_[i].position_, Quaternion.identity);
        }
    }

    void SpawnNewPowerup()
    {
        int powerupKind = Random.Range(0, powerupPrefabs_.Length);

        Vector3 powerupPos = new Vector3(horizontalEnterCoord_, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_);

        GameObject powerup = Instantiate(powerupPrefabs_[powerupKind], powerupPos, Quaternion.identity) as GameObject;
        powerup.GetComponent<powerupScript>().setDirection(new Vector2(-1.0f, Random.Range(-0.5f, 0.5f)));
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
