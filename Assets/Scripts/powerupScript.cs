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
    pt_health = 0,
    pt_speedup = 1,
    pt_research = 2
}

public class powerupScript : MonoBehaviour 
{
    public PowerupType powerupType_;
    public AudioClip powerupGainClip_;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<playerScript>().isDead())
        {
            switch (powerupType_)
            {
                case PowerupType.pt_health:
                    other.gameObject.GetComponent<playerScript>().triggerHealthPickup();
                    break;
                case PowerupType.pt_speedup:
                    other.gameObject.GetComponent<playerScript>().triggerSpeedUpPickup();
                    break;
                case PowerupType.pt_research:
                    other.gameObject.GetComponent<playerScript>().triggerResearchPickup();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(powerupGainClip_, transform.position);
            Destroy(gameObject);
        }
    }
}
