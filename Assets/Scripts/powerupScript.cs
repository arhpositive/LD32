/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * powerupScript.cs
 * Handles powerup movement
 */

using UnityEngine;
using System.Collections;

public enum PowerupType
{
    pt_health,
    pt_speedup,
    pt_coin
}

public class powerupScript : MonoBehaviour 
{

    public float speed_;
    public Vector2 direction_;
    public PowerupType powerupType_;

	// Use this for initialization
	void Start () 
    {
        direction_.Normalize();
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Translate(direction_ * speed_ * Time.deltaTime, Space.World);

        if  (transform.position.y < playerScript.minVerticalMovementLimit_ || transform.position.y > playerScript.maxVerticalMovementLimit_)
        {
            direction_.y = -direction_.y;
        }

        if (transform.position.x < spawnScript.horizontalExitCoord_ || transform.position.x > spawnScript.horizontalEnterCoord_)
        {
            Destroy(gameObject);
        }
	}

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            switch (powerupType_)
            {
                case PowerupType.pt_health:
                    other.gameObject.GetComponent<playerScript>().triggerHealthPickup();
                    break;
                case PowerupType.pt_speedup:
                    other.gameObject.GetComponent<playerScript>().triggerSpeedUpPickup();
                    break;
                case PowerupType.pt_coin:
                    other.gameObject.GetComponent<playerScript>().triggerCoinPickup();
                    break;
                default:
                    break;
            }


            Destroy(gameObject);
        }
    }

    public void setDirection(Vector2 direction)
    {
        direction_ = direction;
        direction_.Normalize();
    }
}
