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
    public GameObject basicEnemyPrefab_;

    public static float horizontalEnterCoord_ = 8.0f;
    public static float horizontalExitCoord_ = -1.0f;

    float spawnZCoord_ = 0.0f;

    float previousWaveSpawnTime_;
    float waveSpawnInterval_ = 5.0f;

    bool playerNeedsRespawn_;
    float playerDeathTime_;
    float playerRespawnTimer_ = 2.0f;
    
    //float enemyHorizontalSpawnInterval_ = 0.9f; //TODO_ARHAN open up later
    
    static float[] enemyVerticalSpawnPosArray_ = {0.55f, 1.45f, 2.35f, 3.25f, 4.15f, 5.05f};
    
    List<WaveEntity> nextWave_;

	// Use this for initialization
	void Start () 
    {
        SpawnPlayer();
        previousWaveSpawnTime_ = Time.time;
        playerDeathTime_ = Time.time;
        nextWave_ = new List<WaveEntity>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if (Time.time - previousWaveSpawnTime_ > waveSpawnInterval_)
        {
            FormNewWave();
            SpawnNewWave();
            nextWave_.Clear();
            previousWaveSpawnTime_ = Time.time;
        }

        if (playerNeedsRespawn_ && Time.time - playerDeathTime_ > playerRespawnTimer_)
        {
            //TODO_ARHAN end game if player is out of lives
            SpawnPlayer();
        }
	}

    void FormNewWave()
    {
        for (int i = 0; i < enemyVerticalSpawnPosArray_.Length; i++)
        {
            WaveEntity entity = new WaveEntity(new Vector3(horizontalEnterCoord_, enemyVerticalSpawnPosArray_[i], spawnZCoord_), basicEnemyPrefab_);
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

    public void PlayerDied()
    {
        playerNeedsRespawn_ = true;
        playerDeathTime_ = Time.time;
    }

    void SpawnPlayer()
    {
        Instantiate(playerPrefab_,
            new Vector3(0.0f, Random.Range(playerScript.minVerticalMovementLimit_, playerScript.maxVerticalMovementLimit_), spawnZCoord_),
            Quaternion.identity);
        playerNeedsRespawn_ = false;
    }
}
