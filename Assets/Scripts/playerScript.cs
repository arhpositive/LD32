/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * playerScript.cs
 * Handles player movement and actions via given input
 */

using UnityEngine;
using System.Collections;

public class playerScript : MonoBehaviour 
{
    public float playerSpeedLimit_;
    public float playerAcceleration_; //TODO_ARHAN adjust acceleration values

    float currentHorizontalSpeed_;
    float currentVerticalSpeed_;
    public static float minHorizontalMovementLimit_ = -0.15f;
    public static float maxHorizontalMovementLimit_ = 3.25f;
    public static float minVerticalMovementLimit_ = 0.45f;
    public static float maxVerticalMovementLimit_ = 5.15f;

    bool isInvulnerable_;
    float invulnerabilityDuration_ = 2.0f;
    float invulnerabilityStart_;
    SpriteRenderer sprRenderer_;

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

        //movement
        Vector2 inputDir = getMoveDirFromInput();

        if (inputDir.x > 0.0f && currentHorizontalSpeed_ >= 0.0f)
        {
            currentHorizontalSpeed_ = Mathf.Clamp(currentHorizontalSpeed_ + playerAcceleration_, 0.0f, 1.0f);
        }
        else if (inputDir.x < 0.0f && currentHorizontalSpeed_ <= 0.0f)
        {
            currentHorizontalSpeed_ = Mathf.Clamp(currentHorizontalSpeed_ - playerAcceleration_, -1.0f, 0.0f);
        }
        else
        {
            currentHorizontalSpeed_ = 0.0f;
        }

        if (inputDir.y > 0.0f && currentVerticalSpeed_ >= 0.0f)
        {
            currentVerticalSpeed_ = Mathf.Clamp(currentVerticalSpeed_ + playerAcceleration_, 0.0f, 1.0f);
        }
        else if (inputDir.y < 0.0f && currentVerticalSpeed_ <= 0.0f)
        {
            currentVerticalSpeed_ = Mathf.Clamp(currentVerticalSpeed_ - playerAcceleration_, -1.0f, 0.0f);
        }
        else
        {
            currentVerticalSpeed_ = 0.0f;
        }

        Vector2 movementDir = Vector2.ClampMagnitude(new Vector2(currentHorizontalSpeed_, currentVerticalSpeed_), 1.0f);
        movementDir *= playerSpeedLimit_ * Time.deltaTime;
        transform.Translate(movementDir);

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInvulnerable_ && other.gameObject.tag == "Enemy")
        {
            scriptSpawn_.PlayerDied();
            Destroy(gameObject);
        }
    }
}
