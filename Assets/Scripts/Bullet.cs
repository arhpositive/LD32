/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Bullet.cs
 * Handles bullets which have a speed and a direction.
 */

using UnityEngine;

namespace Assets.Scripts
{
    public enum BulletType
    {
        BtStun,
        BtSpeedup,
        BtKiller
    }

    public class Bullet : MonoBehaviour
    {
        public BulletType CurrentBulletType;
        public AudioClip BulletHitClip;
        bool _hasCollided;

        void Start()
        {
            _hasCollided = false;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!_hasCollided)
            {
                if (other.gameObject.tag == "Enemy")
                {
                    BasicEnemy enemyScript = other.gameObject.GetComponent<BasicEnemy>();

                    switch (CurrentBulletType)
                    {
                        case BulletType.BtStun:
                            enemyScript.TriggerStun();
                            break;
                        case BulletType.BtSpeedup:
                            enemyScript.TriggerSpeedBoost();
                            break;
                    }
                    _hasCollided = true;
                    AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
                    Destroy(gameObject);
                }
                else if (other.gameObject.tag == "Player")
                {
                    Player playerScript = other.gameObject.GetComponent<Player>();

                    switch (CurrentBulletType)
                    {
                        case BulletType.BtKiller:
                            bool playerGotHit = playerScript.PlayerGotHit();
                            if (playerGotHit)
                            {
                                _hasCollided = true;
                                AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
                                Destroy(gameObject);
                            }
                            break;
                    }
                }
                else if (other.gameObject.tag == "Shield")
                {
                    Player playerScript = other.gameObject.GetComponentInParent<Player>();

                    switch (CurrentBulletType)
                    {
                        case BulletType.BtKiller:
                            bool shieldGotHit = playerScript.ShieldGotHit();
                            if (shieldGotHit)
                            {
                                _hasCollided = true;
                                AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
                                Destroy(gameObject);
                            }
                            break;
                    }
                }
            }
        }
    }
}
