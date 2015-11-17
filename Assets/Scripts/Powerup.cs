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
        PtShield,
        PtTeleport,
        PtBomb,
        PtCount
    }

    public class Powerup : MonoBehaviour
    {
        public PowerupType PowerupType;
        [Range(0, 10)]
        public int PowerupOccurence;
        public bool IsNegativePowerup;
        public Color NegativePowerupBlinkColor;
        public AudioClip GainPowerupClip;

        SpriteRenderer _spriteRenderer;
        Color _rendererColor;
        bool _hasCollided;
        float _lastBlinkTime;
        const float _blinkSpeed = 0.5f;

        void Start()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _rendererColor = _spriteRenderer.color;
            _hasCollided = false;
        }

        void Update()
        {
            if (IsNegativePowerup && Time.time - _lastBlinkTime > _blinkSpeed)
            {
                //negative powerups blink
                if (_spriteRenderer.color == _rendererColor)
                {
                    _spriteRenderer.color = NegativePowerupBlinkColor;
                }
                else
                {
                    _spriteRenderer.color = _rendererColor;
                }

                _lastBlinkTime = Time.time;
            }
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
                        bool gotPickedUp = true;
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
                            case PowerupType.PtTeleport:
                                playerScript.TriggerTeleportPickup();
                                break;
                            case PowerupType.PtBomb:
                                gotPickedUp = playerScript.PlayerGotHit();
                                if (gotPickedUp)
                                {
                                    EventLogger.PrintToLog("Bomb Collides v Player");
                                }
                                break;
                        }

                        if (gotPickedUp)
                        {
                            _hasCollided = true;
                            AudioSource.PlayClipAtPoint(GainPowerupClip, transform.position);
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}
