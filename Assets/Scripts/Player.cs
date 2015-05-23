/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using UnityEngine;
using System.Collections;

struct Gun
{
    public GameObject BulletPrefab;
    public float Cooldown;
    public float LastFireTime;
    public int AmmoCount;

    public Gun (GameObject bulletPrefab, float cooldown, float lastFireTime, int ammoCount)
    {
        BulletPrefab = bulletPrefab;
        Cooldown = cooldown;
        LastFireTime = lastFireTime;
        AmmoCount = ammoCount;
    }
}

public class Player : MonoBehaviour 
{
    public static int PlayerScore = 0;

    public GameObject StunBulletPrefab;
    public GameObject SpeedUpBulletPrefab;
    public float PlayerSpeedLimit;
    public float PlayerAcceleration;

    public AudioClip FireStunGunClip;
    public AudioClip FireSpeedUpGunClip;
    public AudioClip ExplosionClip;

    RefreshEndScoreText EndGameScoreText;
    
    float CurrentHorizontalSpeed;
    float CurrentVerticalSpeed;

    int PlayerHealth;

    bool IsDead;
    float DeathDuration = 2.0f;
    float DeathTime;

    bool IsInvulnerable;
    float InvulnerabilityDuration = 2.0f;
    float InvulnerabilityStartTime;
    
    SpriteRenderer SprRenderer;
    SpriteRenderer[] ChildRenderers;

    Gun StunGun;
    Gun SpeedUpGun;

    void Awake()
    {
        SprRenderer = gameObject.GetComponent<SpriteRenderer>();
        ChildRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        EndGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();
    }

	// Use this for initialization
	void Start () 
    {
        PlayerScore = 0;
        PlayerHealth = 3;
        IsDead = false;
        IsInvulnerable = true;
        InvulnerabilityStartTime = Time.time;
        CurrentHorizontalSpeed = 0.0f;
        CurrentVerticalSpeed = 0.0f;

        StunGun = new Gun(StunBulletPrefab, 0.3f, 0.0f, -1);
        SpeedUpGun = new Gun(SpeedUpBulletPrefab, 0.5f, 0.0f, 3);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (IsDead)
        {
            if (Time.time - DeathTime > DeathDuration)
            {
                //rise from dead
                IsDead = false;
                SprRenderer.enabled = true;
                foreach (SpriteRenderer r in ChildRenderers)
                {
                    r.enabled = true;
                }

                //spawn
                transform.position = new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit, GameConstants.MaxVerticalMovementLimit));
                IsInvulnerable = true;
                InvulnerabilityStartTime = Time.time;
            }
            else
            {
                return;
            }
        }

        //invulnerability
        if (IsInvulnerable)
        {
            if ((Time.time - InvulnerabilityStartTime) % 0.5f > 0.25f)
            {
                SprRenderer.enabled = false;
                foreach (SpriteRenderer r in ChildRenderers)
                {
                    r.enabled = false;
                }
            }
            else
            {
                SprRenderer.enabled = true;
                foreach (SpriteRenderer r in ChildRenderers)
                {
                    r.enabled = true;
                }
            }
        }

        if (Time.time - InvulnerabilityStartTime > InvulnerabilityDuration)
        {
            IsInvulnerable = false;
            SprRenderer.enabled = true;
        }

        //shooting
        if (Input.GetKey(KeyCode.Space) && Time.time - StunGun.LastFireTime > StunGun.Cooldown)
        {
            FireStunGun();
        }
        else if (Input.GetKey(KeyCode.C) && 
            Time.time - SpeedUpGun.LastFireTime > SpeedUpGun.Cooldown &&
            SpeedUpGun.AmmoCount > 0)
        {
            FireSpeedUpGun();
            //TODO consider giving info on why player can't shoot
            //TODO consider displaying cooldown on ui
        }

        //movement
        Vector2 inputDir = getMoveDirFromInput();

        if (inputDir.x > Mathf.Epsilon && CurrentHorizontalSpeed >= 0.0f)
        {
            CurrentHorizontalSpeed = Mathf.Clamp(CurrentHorizontalSpeed + PlayerAcceleration, 0.0f, 1.0f);
        }
        else if (inputDir.x < -Mathf.Epsilon && CurrentHorizontalSpeed <= 0.0f)
        {
            CurrentHorizontalSpeed = Mathf.Clamp(CurrentHorizontalSpeed - PlayerAcceleration, -1.0f, 0.0f);
        }
        else
        {
            CurrentHorizontalSpeed = 0.0f;
        }

        if (inputDir.y > Mathf.Epsilon && CurrentVerticalSpeed >= 0.0f)
        {
            CurrentVerticalSpeed = Mathf.Clamp(CurrentVerticalSpeed + PlayerAcceleration, 0.0f, 1.0f);
        }
        else if (inputDir.y < -Mathf.Epsilon && CurrentVerticalSpeed <= 0.0f)
        {
            CurrentVerticalSpeed = Mathf.Clamp(CurrentVerticalSpeed - PlayerAcceleration, -1.0f, 0.0f);
        }
        else
        {
            CurrentVerticalSpeed = 0.0f;
        }

        Vector2 movementDir = Vector2.ClampMagnitude(new Vector2(CurrentHorizontalSpeed, CurrentVerticalSpeed), 1.0f);
        movementDir *= PlayerSpeedLimit * Time.deltaTime;
        transform.Translate(movementDir, Space.World);

        if (transform.position.x < GameConstants.MinHorizontalMovementLimit || transform.position.x > GameConstants.MaxHorizontalMovementLimit)
        {
            CurrentHorizontalSpeed = 0.0f;
        }
        if (transform.position.y < GameConstants.MinVerticalMovementLimit || transform.position.y > GameConstants.MaxVerticalMovementLimit)
        {
            CurrentVerticalSpeed = 0.0f;
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, GameConstants.MinHorizontalMovementLimit, 
            GameConstants.MaxHorizontalMovementLimit), Mathf.Clamp(transform.position.y, GameConstants.MinVerticalMovementLimit, 
            GameConstants.MaxVerticalMovementLimit), transform.position.z);
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
        if (!IsDead && !IsInvulnerable && other.gameObject.tag == "Enemy")
        {
            playerGotHit();
        }
    }

    void playerGotHit()
    {
        PlayerHealth--;

        if (PlayerHealth == 0)
        {
            EndGameScoreText.SetTextVisible();
            Destroy(gameObject);
        }
        else
        {
            AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
            DeathTime = Time.time;
            IsDead = true;
            SprRenderer.enabled = false;
            foreach(SpriteRenderer r in ChildRenderers)
            {
                r.enabled = false;
            }
        }
    }

    public int GetPlayerHealth()
    {
        return PlayerHealth;
    }

    public bool GetIsInvulnerable()
    {
        return IsInvulnerable;
    }

    public bool GetIsDead()
    {
        return IsDead;
    }

    public void TriggerEnemyDestruction()
    {
        PlayerScore -= 100;
    }

    public void TriggerEnemyDisplacement(int scoreAddition)
    {
        PlayerScore += scoreAddition;
    }

    public void TriggerHealthPickup()
    {
        PlayerHealth++;
    }

    public void TriggerSpeedUpPickup()
    {
        SpeedUpGun.AmmoCount++;
    }

    public void TriggerResearchPickup()
    {
        PlayerScore += 50;
    }

    public void TriggerGettingShot()
    {
        playerGotHit();
    }

    public int GetSpeedUpGunAmmo()
    {
        return SpeedUpGun.AmmoCount;
    }

    void FireStunGun()
    {
        StunGun.LastFireTime = Time.time;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Vector3 bulletStartPoint = transform.GetChild(i).position;
                Instantiate(StunGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
                AudioSource.PlayClipAtPoint(FireStunGunClip, transform.GetChild(i).position);
            }
        }        
    }

    void FireSpeedUpGun()
    {
        SpeedUpGun.LastFireTime = Time.time;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Vector3 bulletStartPoint = transform.GetChild(i).position;
                Instantiate(SpeedUpGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
                SpeedUpGun.AmmoCount--;
                AudioSource.PlayClipAtPoint(FireSpeedUpGunClip, bulletStartPoint);
            }
        }
    }
}
