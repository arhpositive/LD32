/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Powerup.cs
 * Handles powerup movement
 */

using UnityEngine;

namespace Assets.Scripts
{
    public enum PowerupType
    {
        PtHealth,
        PtSpeedup,
        PtResearch,
        PtShield
    }

    public class Powerup : MonoBehaviour
    {
        public PowerupType PowerupType;
        public AudioClip GainPowerupClip;
        bool _hasCollided;

        void Start()
        {
            _hasCollided = false;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!_hasCollided)
            {
                if (other.gameObject.tag == "Player")
                {
                    Player playerScript = other.gameObject.GetComponent<Player>();

                    if (!playerScript.IsDead)
                    {
                        switch (PowerupType)
                        {
                            case PowerupType.PtHealth:
                                playerScript.TriggerHealthPickup();
                                break;
                            case PowerupType.PtSpeedup:
                                playerScript.TriggerSpeedUpPickup();
                                break;
                            case PowerupType.PtResearch:
                                playerScript.TriggerResearchPickup();
                                break;
                            case PowerupType.PtShield:
                                playerScript.TriggerShieldPickup();
                                break;
                        }
                        _hasCollided = true;
                        AudioSource.PlayClipAtPoint(GainPowerupClip, transform.position);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
