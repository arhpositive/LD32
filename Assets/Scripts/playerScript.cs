/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * playerScript.cs
 * Handles player movement and actions via given input
 */

using UnityEngine;
using System.Collections;

struct Gun
{
    public GameObject bulletPrefab_;
    public float cooldown_;
    public float shootTime_;
    public int ammoCount_;

    public Gun (GameObject bulletPrefab, float cooldown, float shootTime, int ammoCount)
    {
        bulletPrefab_ = bulletPrefab;
        cooldown_ = cooldown;
        shootTime_ = shootTime;
        ammoCount_ = ammoCount;
    }
}

public class playerScript : MonoBehaviour 
{
    public static float minHorizontalMovementLimit_ = -0.15f;
    public static float maxHorizontalMovementLimit_ = 3.25f;
    public static float minVerticalMovementLimit_ = 0.45f;
    public static float maxVerticalMovementLimit_ = 5.15f;

    public static int playerScore_ = 0;

    public GameObject explosionPrefab_;
    public GameObject stunBulletPrefab_;
    public GameObject speedUpBulletPrefab_;
    public float playerSpeedLimit_;
    public float playerAcceleration_; //TODO_ARHAN adjust acceleration values

    public AudioClip fireStunGunClip_;
    public AudioClip fireSpeedUpGunClip_;
    public AudioClip explosionClip_;

    endScoreRefreshText endGameScoreDisplay_;
    
    float currentHorizontalSpeed_;
    float currentVerticalSpeed_;

    int playerHealth_;

    bool isDead_;
    float deathDuration_ = 2.0f;
    float deathStart_;

    bool isInvulnerable_;
    float invulnerabilityDuration_ = 2.0f;
    float invulnerabilityStart_;
    
    SpriteRenderer sprRenderer_;
    SpriteRenderer[] childRenderers_;

    Gun stunGun_;
    Gun speedUpGun_;

    void Awake()
    {
        sprRenderer_ = gameObject.GetComponent<SpriteRenderer>();
        childRenderers_ = gameObject.GetComponentsInChildren<SpriteRenderer>();
        endGameScoreDisplay_ = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<endScoreRefreshText>();
    }

	// Use this for initialization
	void Start () 
    {
        playerScore_ = 0;
        playerHealth_ = 3;
        isDead_ = false;
        isInvulnerable_ = true;
        invulnerabilityStart_ = Time.time;
        currentHorizontalSpeed_ = 0.0f;
        currentVerticalSpeed_ = 0.0f;

        stunGun_ = new Gun(stunBulletPrefab_, 0.3f, 0.0f, -1);
        speedUpGun_ = new Gun(speedUpBulletPrefab_, 0.5f, 0.0f, 3);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isDead_)
        {
            if (Time.time - deathStart_ > deathDuration_)
            {
                //rise from dead
                isDead_ = false;
                sprRenderer_.enabled = true;
                foreach (SpriteRenderer r in childRenderers_)
                {
                    r.enabled = true;
                }

                //spawn
                transform.position = new Vector3(0.0f, Random.Range(minVerticalMovementLimit_, maxVerticalMovementLimit_), spawnScript.spawnZCoord_);
                isInvulnerable_ = true;
                invulnerabilityStart_ = Time.time;
            }
            else
            {
                return;
            }
        }

        //invulnerability
        if (isInvulnerable_)
        {
            if ((Time.time - invulnerabilityStart_) % 0.5f > 0.25f)
            {
                sprRenderer_.enabled = false;
                foreach (SpriteRenderer r in childRenderers_)
                {
                    r.enabled = false;
                }
            }
            else
            {
                sprRenderer_.enabled = true;
                foreach (SpriteRenderer r in childRenderers_)
                {
                    r.enabled = true;
                }
            }
        }

        if (Time.time - invulnerabilityStart_ > invulnerabilityDuration_)
        {
            isInvulnerable_ = false;
            sprRenderer_.enabled = true;
        }

        //shooting
        if (Input.GetKey(KeyCode.Space) && Time.time - stunGun_.shootTime_ > stunGun_.cooldown_)
        {
            FireStunGun();
        }
        else if (Input.GetKey(KeyCode.C) && 
            Time.time - speedUpGun_.shootTime_ > speedUpGun_.cooldown_ &&
            speedUpGun_.ammoCount_ > 0)
        {
            FireSpeedUpGun();
            //TODO consider giving info on why player can't shoot
            //TODO consider displaying cooldown on ui
        }

        //movement
        Vector2 inputDir = getMoveDirFromInput();

        if (inputDir.x > Mathf.Epsilon && currentHorizontalSpeed_ >= 0.0f)
        {
            currentHorizontalSpeed_ = Mathf.Clamp(currentHorizontalSpeed_ + playerAcceleration_, 0.0f, 1.0f);
        }
        else if (inputDir.x < -Mathf.Epsilon && currentHorizontalSpeed_ <= 0.0f)
        {
            currentHorizontalSpeed_ = Mathf.Clamp(currentHorizontalSpeed_ - playerAcceleration_, -1.0f, 0.0f);
        }
        else
        {
            currentHorizontalSpeed_ = 0.0f;
        }

        if (inputDir.y > Mathf.Epsilon && currentVerticalSpeed_ >= 0.0f)
        {
            currentVerticalSpeed_ = Mathf.Clamp(currentVerticalSpeed_ + playerAcceleration_, 0.0f, 1.0f);
        }
        else if (inputDir.y < -Mathf.Epsilon && currentVerticalSpeed_ <= 0.0f)
        {
            currentVerticalSpeed_ = Mathf.Clamp(currentVerticalSpeed_ - playerAcceleration_, -1.0f, 0.0f);
        }
        else
        {
            currentVerticalSpeed_ = 0.0f;
        }

        Vector2 movementDir = Vector2.ClampMagnitude(new Vector2(currentHorizontalSpeed_, currentVerticalSpeed_), 1.0f);
        movementDir *= playerSpeedLimit_ * Time.deltaTime;
        transform.Translate(movementDir, Space.World);

        if (transform.position.x < minHorizontalMovementLimit_ || transform.position.x > maxHorizontalMovementLimit_)
        {
            currentHorizontalSpeed_ = 0.0f;
        }
        if (transform.position.y < minVerticalMovementLimit_ || transform.position.y > maxVerticalMovementLimit_)
        {
            currentVerticalSpeed_ = 0.0f;
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minHorizontalMovementLimit_, maxHorizontalMovementLimit_),
            Mathf.Clamp(transform.position.y, minVerticalMovementLimit_, maxVerticalMovementLimit_), transform.position.z);
	}

    Vector2 getMoveDirFromInput()
    {
        Vector2 moveDir = Vector2.zero;

        float horizontalMoveDir = Input.GetAxisRaw("Horizontal");
        float verticalMoveDir = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(horizontalMoveDir, verticalMoveDir);
        moveDir.Normalize();

        return moveDir;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDead_ && !isInvulnerable_ && other.gameObject.tag == "Enemy")
        {
            playerGotHit();
        }
    }

    void playerGotHit()
    {
        playerHealth_--;

        if (playerHealth_ == 0)
        {
            endGameScoreDisplay_.SetTextVisible();
            Destroy(gameObject);
        }
        else
        {
            AudioSource.PlayClipAtPoint(explosionClip_, transform.position);
            deathStart_ = Time.time;
            isDead_ = true;
            sprRenderer_.enabled = false;
            foreach(SpriteRenderer r in childRenderers_)
            {
                r.enabled = false;
            }
        }
    }

    public int getPlayerHealth()
    {
        return playerHealth_;
    }

    public bool isInvulnerable()
    {
        return isInvulnerable_;
    }

    public bool isDead()
    {
        return isDead_;
    }

    public void triggerEnemyDestruction()
    {
        playerScore_ -= 100;
    }

    public void triggerEnemyDisplacement(int scoreAddition)
    {
        playerScore_ += scoreAddition;
    }

    public void triggerHealthPickup()
    {
        playerHealth_++;
    }

    public void triggerSpeedUpPickup()
    {
        speedUpGun_.ammoCount_++;
    }

    public void triggerResearchPickup()
    {
        playerScore_ += 50;
    }

    public void triggerGettingShot()
    {
        playerGotHit();
    }

    public int getHealth()
    {
        return playerHealth_;
    }

    public int getSpeedUpGunAmmo()
    {
        return speedUpGun_.ammoCount_;
    }

    void FireStunGun()
    {
        stunGun_.shootTime_ = Time.time;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Vector3 bulletStartPoint = transform.GetChild(i).position;
                Instantiate(stunGun_.bulletPrefab_, bulletStartPoint, Quaternion.identity);
                AudioSource.PlayClipAtPoint(fireStunGunClip_, transform.GetChild(i).position);
            }
        }        
    }

    void FireSpeedUpGun()
    {
        speedUpGun_.shootTime_ = Time.time;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Vector3 bulletStartPoint = transform.GetChild(i).position;
                Instantiate(speedUpGun_.bulletPrefab_, bulletStartPoint, Quaternion.identity);
                speedUpGun_.ammoCount_--;
                AudioSource.PlayClipAtPoint(fireSpeedUpGunClip_, bulletStartPoint);
            }
        }
    }
}
