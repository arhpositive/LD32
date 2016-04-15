﻿/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Powerup.cs
 * Handles powerup specific features
 */

using UnityEngine;

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

    private BasicObject _basicObjectScript;
    private SpriteRenderer _spriteRenderer;
    private Color _rendererColor;
    private bool _hasCollided;
    private float _lastBlinkTime;
    private const float BlinkSpeed = 0.5f;

    private void Start()
    {
        _basicObjectScript = gameObject.GetComponent<BasicObject>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _rendererColor = _spriteRenderer.color;
        _hasCollided = false;
    }

    private void Update()
    {
        if (IsNegativePowerup && Time.time - _lastBlinkTime > BlinkSpeed)
        {
            //negative powerups blink
            _spriteRenderer.color = _spriteRenderer.color == _rendererColor ? NegativePowerupBlinkColor : _rendererColor;

            _lastBlinkTime = Time.time;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
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
                        _basicObjectScript.OnDestruction();
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}