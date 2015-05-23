/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Powerup.cs
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

public class Powerup : MonoBehaviour 
{
    public PowerupType PowerupType;
    public AudioClip GainPowerupClip;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<Player>().GetIsDead())
        {
            switch (PowerupType)
            {
                case PowerupType.pt_health:
                    other.gameObject.GetComponent<Player>().TriggerHealthPickup();
                    break;
                case PowerupType.pt_speedup:
                    other.gameObject.GetComponent<Player>().TriggerSpeedUpPickup();
                    break;
                case PowerupType.pt_research:
                    other.gameObject.GetComponent<Player>().TriggerResearchPickup();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(GainPowerupClip, transform.position);
            Destroy(gameObject);
        }
    }
}
