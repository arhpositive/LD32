/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * bulletScript.cs
 * Handles bullets which have a speed and a direction.
 */

using UnityEngine;
using System.Collections;

public enum BulletType
{
    bt_stun,
    bt_speedup,
    bt_killer
}

public class bulletScript : MonoBehaviour 
{
    public BulletType bulletType_;
    public AudioClip bulletHitClip_;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            switch(bulletType_)
            {
                case BulletType.bt_stun:
                    other.gameObject.GetComponent<basicEnemyScript>().triggerStunCondition();
                    break;
                case BulletType.bt_speedup:
                    other.gameObject.GetComponent<basicEnemyScript>().triggerSpeedBoost();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(bulletHitClip_, transform.position);
            Destroy(gameObject);
        }
        else if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<playerScript>().isInvulnerable() &&
            !other.gameObject.GetComponent<playerScript>().isDead())
        {
            switch(bulletType_)
            {
                case BulletType.bt_killer:
                    other.gameObject.GetComponent<playerScript>().triggerGettingShot();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(bulletHitClip_, transform.position);
            Destroy(gameObject);
        }
    }
}
