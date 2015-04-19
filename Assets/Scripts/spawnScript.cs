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

    public static float horizontalEnterCoord_ = 8.0f;
    public static float horizontalExitCoord_ = -1.0f;

    public static float spawnZCoord_ = 0.0f;

    float previousWaveSpawnTime_;
    float waveSpawnInterval_ = 5.0f;

    float previousPowerupSpawnTime_;
    float powerupSpawnInterval_ = 2.0f;

    //float enemyHorizontalSpawnInterval_ = 0.9f; //TODO_ARHAN open up later

    static float[] verticalSpawnPosArray_ = { 0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f };

    List<WaveEntity> nextWave_;

    // Use this for initialization
    void Start()
    {
        Instantiate(playerPrefab_,
            new Vector3(0.0f, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_),
            Quaternion.identity);
        previousWaveSpawnTime_ = Time.time;
        previousPowerupSpawnTime_ = Time.time;
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
        powerup.GetComponent<powerupScript>().setDirection(new Vector2(-1.0f, Random.Range(-1.0f, 1.0f)));
    }

}
