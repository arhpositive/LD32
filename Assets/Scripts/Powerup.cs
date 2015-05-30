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
        if (other.gameObject.tag == "Player")
        {
            Player PlayerScript = other.gameObject.GetComponent<Player>();

            if (!PlayerScript.IsDead)
            {
                switch (PowerupType)
                {
                    case PowerupType.pt_health:
                        PlayerScript.TriggerHealthPickup();
                        break;
                    case PowerupType.pt_speedup:
                        PlayerScript.TriggerSpeedUpPickup();
                        break;
                    case PowerupType.pt_research:
                        PlayerScript.TriggerResearchPickup();
                        break;
                    default:
                        break;
                }
                AudioSource.PlayClipAtPoint(GainPowerupClip, transform.position);
                Destroy(gameObject);
            }
        }
    }
}
