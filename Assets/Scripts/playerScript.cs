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

    public GameObject stunBulletPrefab_;
    public GameObject speedUpBulletPrefab_;
    public float playerSpeedLimit_;
    public float playerAcceleration_; //TODO_ARHAN adjust acceleration values
    
    float currentHorizontalSpeed_;
    float currentVerticalSpeed_;

    bool isInvulnerable_;
    float invulnerabilityDuration_ = 2.0f;
    float invulnerabilityStart_;
    SpriteRenderer sprRenderer_;

    Gun stunGun_;
    Gun speedUpGun_;

    spawnScript scriptSpawn_;

    void Awake()
    {
        scriptSpawn_ = Camera.main.gameObject.GetComponent<spawnScript>();
        sprRenderer_ = gameObject.GetComponent<SpriteRenderer>();
    }

	// Use this for initialization
	void Start () 
    {
        isInvulnerable_ = true;
        invulnerabilityStart_ = Time.time;
        currentHorizontalSpeed_ = 0.0f;
        currentVerticalSpeed_ = 0.0f;

        stunGun_ = new Gun(stunBulletPrefab_, 0.5f, 0.0f, -1);
        speedUpGun_ = new Gun(speedUpBulletPrefab_, 0.5f, 0.0f, 3);
	}
	
	// Update is called once per frame
	void Update () 
    {
        //invulnerability
        if (isInvulnerable_)
        {
            if ((Time.time - invulnerabilityStart_) % 0.5f > 0.25f)
            {
                sprRenderer_.enabled = false;
            }
            else
            {
                sprRenderer_.enabled = true;
            }
        }

        if (Time.time - invulnerabilityStart_ > invulnerabilityDuration_)
        {
            isInvulnerable_ = false;
            sprRenderer_.enabled = true;
        }

        //shooting
        if (Input.GetAxisRaw("Fire1") > Mathf.Epsilon && Time.time - stunGun_.shootTime_ > stunGun_.cooldown_)
        {
            FireStunGun();
        }
        else if (Input.GetAxisRaw("Fire2") > Mathf.Epsilon && 
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
        if (!isInvulnerable_ && other.gameObject.tag == "Enemy")
        {
            scriptSpawn_.PlayerDied();
            Destroy(gameObject);
        }
    }

    public bool isInvulnerable()
    {
        return isInvulnerable_;
    }

    void FireStunGun()
    {
        stunGun_.shootTime_ = Time.time;
        Vector3 bulletStartPoint = transform.GetChild(0).position;
        Instantiate(stunGun_.bulletPrefab_, bulletStartPoint, Quaternion.identity);
    }

    void FireSpeedUpGun()
    {
        speedUpGun_.shootTime_ = Time.time;
        Vector3 bulletStartPoint = transform.GetChild(0).position;
        Instantiate(speedUpGun_.bulletPrefab_, bulletStartPoint, Quaternion.identity);
        speedUpGun_.ammoCount_--;
    }
}
