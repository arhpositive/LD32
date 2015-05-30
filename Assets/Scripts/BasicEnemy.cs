/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicEnemy.cs
 * Handles basic enemy behaviour
 */

using UnityEngine;
using System.Collections;

public class BasicEnemy : MonoBehaviour 
{
    public float StunDuration;
    public float SpeedBoostCoef;
    public bool CanShoot;
    public float FiringInterval;
    public GameObject BulletPrefab;
    public AudioClip ExplosionClip;

    GameObject PlayerGameObject;
    Player PlayerScript;
    SpawnManager SpawnManagerScript;
    BasicMove BasicMoveScript;

    bool IsStunned;
    float LastStunTime;    
    bool SpeedBoostIsActive;
    float LastFireTime;
    float NextFiringInterval;
    float DisplacementLength; //used for scoring

	void Start () 
    {
        PlayerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (PlayerGameObject)
        {
            PlayerScript = PlayerGameObject.GetComponent<Player>();
        }
        SpawnManagerScript = Camera.main.GetComponent<SpawnManager>();
        BasicMoveScript = gameObject.GetComponent<BasicMove>();

        IsStunned = false;
        LastStunTime = 0.0f;
        SpeedBoostIsActive = false;
        LastFireTime = Time.time;
        NextFiringInterval = (Random.Range(FiringInterval - 1.0f, FiringInterval + 1.0f) / SpawnManagerScript.DifficultyMultiplier) * 0.5f;
        DisplacementLength = 0.0f;
	}
	
	void Update () 
    {
        // if stun timer expired
        if (IsStunned && Time.time - LastStunTime > StunDuration)
        {
            RemoveStun();
        }

        if (!IsStunned)
        {
            // if fire timer expired
            if (CanShoot && Time.time - LastFireTime > NextFiringInterval)
            {
                FireGun();
            }

            if (SpeedBoostIsActive)
            {
                DisplacementLength -= Time.deltaTime * BasicMoveScript.MoveSpeed * (BasicMoveScript.SpeedCoef - 1.0f);
            }

            if (transform.position.x < GameConstants.HorizontalMinCoord)
            {
                //cash in the points
                if (PlayerScript)
                {
                    PlayerScript.TriggerEnemyDisplacement((int)Mathf.Abs(DisplacementLength));
                }

                Destroy(gameObject);
            }
        }
	}

    public void TriggerStun()
    {
        IsStunned = true;
        LastStunTime = Time.time;
        BasicMoveScript.DoesMove = false;
    }

    void RemoveStun()
    {
        IsStunned = false;
        LastFireTime = Time.time;
        BasicMoveScript.DoesMove = true;
        DisplacementLength += StunDuration * BasicMoveScript.MoveSpeed;
    }

    public void TriggerSpeedBoost()
    {
        if (!SpeedBoostIsActive)
        {
            SpeedBoostIsActive = true;
            BasicMoveScript.SpeedCoef = SpeedBoostCoef;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if ((other.gameObject.tag == "Player" && !other.gameObject.GetComponent<Player>().IsInvulnerable && 
            !other.gameObject.GetComponent<Player>().IsDead) || other.gameObject.tag == "Enemy")
        {
            AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
            Destroy(gameObject);
        }
    }

    void FireGun()
    {
        LastFireTime = Time.time;
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("BulletStart"))
            {
                Instantiate(BulletPrefab, transform.GetChild(i).position, Quaternion.identity);
            }            
        }
        NextFiringInterval = Random.Range(FiringInterval - 1.0f, FiringInterval + 1.0f) / SpawnManagerScript.DifficultyMultiplier;
    }
}
