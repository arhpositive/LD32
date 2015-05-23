/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Bullet.cs
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

public class Bullet : MonoBehaviour 
{
    public BulletType CurrentBulletType;
    public AudioClip BulletHitClip;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            switch(CurrentBulletType)
            {
                case BulletType.bt_stun:
                    other.gameObject.GetComponent<BasicEnemy>().TriggerStun();
                    break;
                case BulletType.bt_speedup:
                    other.gameObject.GetComponent<BasicEnemy>().TriggerSpeedBoost();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
            Destroy(gameObject);
        }
        else if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<Player>().GetIsInvulnerable() &&
            !other.gameObject.GetComponent<Player>().GetIsDead())
        {
            switch(CurrentBulletType)
            {
                case BulletType.bt_killer:
                    other.gameObject.GetComponent<Player>().TriggerGettingShot();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
            Destroy(gameObject);
        }
    }
}
