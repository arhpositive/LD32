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
        BtTeleport,
        BtKiller
    }

    public class Bullet : MonoBehaviour
    {
        public BulletType CurrentBulletType;
        public AudioClip BulletHitClip;
        public bool ShotByPlayer;
        bool _hasCollided;
        bool _destroyedByCollision;
        Player _playerScript;

        void Start()
        {
            _hasCollided = false;
            _destroyedByCollision = false;
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
                _playerScript = playerObject.GetComponent<Player>();
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
	        if (_hasCollided)
	        {
		        return;
	        }

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
	            _destroyedByCollision = true;
		        AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
		        Destroy(gameObject);
	        }
	        else if (other.gameObject.tag == "Player")
	        {
		        switch (CurrentBulletType)
		        {
			        case BulletType.BtKiller:
				        bool playerGotHit = _playerScript.PlayerGotHit();
		                if (playerGotHit)
		                {
                            EventLogger.PrintToLog("Bullet Collision v Player");
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
                            EventLogger.PrintToLog("Bullet Collision v Shield");
					        _hasCollided = true;
					        AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
					        Destroy(gameObject);
				        }
				        break;
		        }
	        }
        }

        void OnDestroy()
        {
            if (CurrentBulletType == BulletType.BtTeleport)
            {
                AudioSource.PlayClipAtPoint(BulletHitClip, transform.position);
            }
            else if (ShotByPlayer)
            {
                _playerScript.OnBulletDestruction(_destroyedByCollision);
            }
        }
    }
}
