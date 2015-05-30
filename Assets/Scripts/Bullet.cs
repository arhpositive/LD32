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
            BasicEnemy EnemyScript = other.gameObject.GetComponent<BasicEnemy>();

            switch(CurrentBulletType)
            {
                case BulletType.bt_stun:
                    EnemyScript.TriggerStun();
                    break;
                case BulletType.bt_speedup:
                    EnemyScript.TriggerSpeedBoost();
                    break;
                default:
                    break;
            }
            AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
            Destroy(gameObject);
        }
        else if (other.gameObject.tag == "Player")
        {
            Player PlayerScript = other.gameObject.GetComponent<Player>();
            
            if (!PlayerScript.IsInvulnerable && !PlayerScript.IsDead)
            {
                switch (CurrentBulletType)
                {
                    case BulletType.bt_killer:
                        PlayerScript.TriggerGettingShot();
                        break;
                    default:
                        break;
                }
                AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
                Destroy(gameObject);
            }
        }
    }
}
