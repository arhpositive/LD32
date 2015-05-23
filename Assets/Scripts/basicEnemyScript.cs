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
    public float stunDuration_;
    public float speedBoostPercentage_;
    public bool canShoot_;
    public float shootingInterval_;
    public bool randomShot_;
    public GameObject bulletPrefab_;
    public AudioClip explosionClip_;    

    bool isStunned_;
    float stunTime_;    
    bool speedBoostActive_;
    float lastShotTime_;
    float nextShootingInterval_;

    GameObject playerObject_;
    playerScript scriptPlayer_;
    spawnScript scriptSpawn_;
    BasicMove MoveScript;

    public float displacementAmount_; //used for scoring

	// Use this for initialization
	void Start () 
    {
        MoveScript = gameObject.GetComponent<BasicMove>();

        isStunned_ = false;
        stunTime_ = 0.0f;
        speedBoostActive_ = false;
        lastShotTime_ = Time.time;
        displacementAmount_ = 0.0f;
        playerObject_ = GameObject.FindGameObjectWithTag("Player");
        if (playerObject_)
        {
            scriptPlayer_ = playerObject_.GetComponent<playerScript>();
        }
        scriptSpawn_ = Camera.main.GetComponent<spawnScript>();
        nextShootingInterval_ = (Random.Range(shootingInterval_ - 1.0f, shootingInterval_ + 1.0f) / scriptSpawn_.getDifficultyMultiplier()) * 0.5f;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isStunned_ && Time.time - stunTime_ > stunDuration_)
        {
            isStunned_ = false;
            lastShotTime_ = Time.time;
            MoveScript.DoesMove = true;
            displacementAmount_ += stunDuration_ * MoveScript.MoveSpeed;
        }

        if (!isStunned_)
        {
            if (canShoot_ && Time.time - lastShotTime_ > nextShootingInterval_)
            {
                FireGun();
            }

            if (speedBoostActive_)
            {
                // TODO something is wrong
                displacementAmount_ -= stunDuration_ * MoveScript.MoveSpeed;
            }

            if (transform.position.x < GameConstants.HorizontalMinCoord)
            {
                //cash in the points
                if (scriptPlayer_)
                {
                    scriptPlayer_.triggerEnemyDisplacement((int)Mathf.Abs(displacementAmount_));
                }

                Destroy(gameObject);
            }
        }
	}

    public void triggerStunCondition()
    {
        isStunned_ = true;
        stunTime_ = Time.time;
        MoveScript.DoesMove = false;
        //TODO consider changing material color
    }

    public void triggerSpeedBoost()
    {
        if (!speedBoostActive_)
        {
            speedBoostActive_ = true;
            MoveScript.MoveSpeed *= speedBoostPercentage_;
        }
        //TODO consider changing material color
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if ((other.gameObject.tag == "Player" && !other.gameObject.GetComponent<playerScript>().isInvulnerable() && 
            !other.gameObject.GetComponent<playerScript>().isDead()) || other.gameObject.tag == "Enemy")
        {
            AudioSource.PlayClipAtPoint(explosionClip_, transform.position);
            Destroy(gameObject);
        }
    }

    void FireGun()
    {
        lastShotTime_ = Time.time;
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Instantiate(bulletPrefab_, transform.GetChild(i).position, Quaternion.identity);
            }            
        }
        nextShootingInterval_ = Random.Range(shootingInterval_ - 1.0f, shootingInterval_ + 1.0f) / scriptSpawn_.getDifficultyMultiplier();
    }
}
