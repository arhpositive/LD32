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
    public bool canShoot_;
    public float shootingInterval_;
    public GameObject bulletPrefab_;

    bool isStunned_;
    float stunTime_;    
    bool speedBoostActive_;
    float lastShotTime_;

	// Use this for initialization
	void Start () 
    {
        direction_.Normalize();
        isStunned_ = false;
        stunTime_ = 0.0f;
        speedBoostActive_ = false;
        lastShotTime_ = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isStunned_ && Time.time - stunTime_ > stunDuration_)
        {
            isStunned_ = false;
            lastShotTime_ = Time.time;
        }

        if (!isStunned_)
        {
            if (canShoot_ && Time.time - lastShotTime_ > shootingInterval_)
            {
                FireGun();
            }

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
        if (!speedBoostActive_)
        {
            speedBoostActive_ = true;
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

    void FireGun()
    {
        lastShotTime_ = Time.time;
        for(int i = 0; i < transform.childCount; i++)
        {
            Vector3 bulletStartPoint = transform.GetChild(i).position;
            GameObject bullet = Instantiate(bulletPrefab_, bulletStartPoint, Quaternion.identity) as GameObject;
            bullet.GetComponent<bulletScript>().SetDirection(new Vector2(-1.0f, (i % 2 == 1 ? 0.1f : -0.1f)));
        }
    }
}
