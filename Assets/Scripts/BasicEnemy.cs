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

    bool IsStunned;
    float LastStunTime;    
    bool SpeedBoostIsActive;
    float LastFireTime;
    float NextFiringInterval;
    float DisplacementLength; //used for scoring

    GameObject PlayerGameObject;
    Player PlayerScript;
    SpawnManager SpawnManagerScript;
    BasicMove BasicMoveScript;

	// Use this for initialization
	void Start () 
    {
        BasicMoveScript = gameObject.GetComponent<BasicMove>();

        IsStunned = false;
        LastStunTime = 0.0f;
        SpeedBoostIsActive = false;
        LastFireTime = Time.time;
        DisplacementLength = 0.0f;
        PlayerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (PlayerGameObject)
        {
            PlayerScript = PlayerGameObject.GetComponent<Player>();
        }
        SpawnManagerScript = Camera.main.GetComponent<SpawnManager>();
        NextFiringInterval = (Random.Range(FiringInterval - 1.0f, FiringInterval + 1.0f) / SpawnManagerScript.GetDifficultyMultiplier()) * 0.5f;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (IsStunned && Time.time - LastStunTime > StunDuration)
        {
            IsStunned = false;
            LastFireTime = Time.time;
            BasicMoveScript.DoesMove = true;
            DisplacementLength += StunDuration * BasicMoveScript.MoveSpeed;
        }

        if (!IsStunned)
        {
            if (CanShoot && Time.time - LastFireTime > NextFiringInterval)
            {
                FireGun();
            }

            if (SpeedBoostIsActive)
            {
                // TODO something is wrong
                DisplacementLength -= StunDuration * BasicMoveScript.MoveSpeed;
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
        //TODO consider changing material color
    }

    public void TriggerSpeedBoost()
    {
        if (!SpeedBoostIsActive)
        {
            SpeedBoostIsActive = true;
            BasicMoveScript.MoveSpeed *= SpeedBoostCoef;
        }
        //TODO consider changing material color
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if ((other.gameObject.tag == "Player" && !other.gameObject.GetComponent<Player>().GetIsInvulnerable() && 
            !other.gameObject.GetComponent<Player>().GetIsDead()) || other.gameObject.tag == "Enemy")
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
        NextFiringInterval = Random.Range(FiringInterval - 1.0f, FiringInterval + 1.0f) / SpawnManagerScript.GetDifficultyMultiplier();
    }
}
