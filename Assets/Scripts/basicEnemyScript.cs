/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * basicEnemyScript.cs
 * Handles basic enemy behaviour
 */

using UnityEngine;
using System.Collections;

public class basicEnemyScript : MonoBehaviour 
{
    public static float enemyHorizontalEnterCoord_ = 8.0f;
    public static float enemyHorizontalExitCoord_ = -1.0f;

    public float speed_;
    public Vector2 direction_;
    public float stunDuration_;
    public float speedBoostPercentage_;

    bool isStunned_;
    float stunTime_;    
    bool speedBoostActive;

	// Use this for initialization
	void Start () 
    {
        isStunned_ = false;
        stunTime_ = 0.0f;
        speedBoostActive = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isStunned_ && Time.time - stunTime_ > stunDuration_)
        {
            isStunned_ = false;
        }

        if (!isStunned_)
        {
            transform.Translate(direction_ * speed_ * Time.deltaTime, Space.World);

            if (transform.position.x < spawnScript.horizontalExitCoord_ || transform.position.x > spawnScript.horizontalEnterCoord_)
            {
                Destroy(gameObject);
            }
        }
	}

    public void triggerStunCondition()
    {
        isStunned_ = true;
        stunTime_ = Time.time;
        //TODO consider changing material color
    }

    public void triggerSpeedBoost()
    {
        if (!speedBoostActive)
        {
            speedBoostActive = true;
            speed_ *= speedBoostPercentage_;
        }
        //TODO consider changing material color
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<playerScript>().isInvulnerable() ||
            other.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}
