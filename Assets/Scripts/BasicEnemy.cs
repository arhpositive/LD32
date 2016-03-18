/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicEnemy.cs
 * Handles basic enemy behaviour
 */

using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts
{
    public class BasicEnemy : MonoBehaviour
    {
        public float StunDuration;
        public float SpeedBoostCoef;
        public bool CanShoot;
        public float MinFiringInterval;
        public GameObject BulletPrefab;
        public AudioClip ExplosionClip;

        public const float MoveSpeed = 1.2f;

        private GameObject _playerGameObject;
        private Player _playerScript;
        private DifficultyManager _difficultyManagerScript;
        private BasicMove _basicMoveScript;

        private bool _hasCollided;
        private bool _isStunned;
        private float _lastStunTime;
        private bool _speedBoostIsActive;
        private float _lastFireTime;
        private float _nextFiringInterval;
        private float _displacementLength; //used for scoring

        private void Start()
        {
            _playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (_playerGameObject)
            {
                _playerScript = _playerGameObject.GetComponent<Player>();
            }
            _difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
            _basicMoveScript = gameObject.GetComponent<BasicMove>();
            _basicMoveScript.MoveSpeed = MoveSpeed;

            _hasCollided = false;
            _isStunned = false;
            _lastStunTime = 0.0f;
            _speedBoostIsActive = false;
            _lastFireTime = Time.time;
            SetNextFiringInterval();
            _displacementLength = 0.0f;
        }

        private void Update()
        {
            // if stun timer expired
            if (_isStunned && Time.time - _lastStunTime > StunDuration)
            {
                RemoveStun();
            }

            if (!_isStunned)
            {
                // if fire timer expired
                if (CanShoot && Time.time - _lastFireTime > _nextFiringInterval)
                {
                    FireGun();
                }

                if (_speedBoostIsActive)
                {
                    _displacementLength -= Time.deltaTime * _basicMoveScript.MoveSpeed * (_basicMoveScript.SpeedCoef - 1.0f);
                }

                if (transform.position.x < GameConstants.HorizontalMinCoord)
                {
                    //cash in the points
                    if (_playerScript)
                    {
                        int scoreAddition = (int) (Mathf.Abs(_displacementLength) * 10.0f);
                        EventLogger.PrintToLog("Enemy Scored: " + scoreAddition);
                        _playerScript.TriggerEnemyDisplacement(scoreAddition);
                    }

                    Destroy(gameObject);
                }
            }
        }

        public void TriggerStun()
        {
            _isStunned = true;
            _lastStunTime = Time.time;
            _basicMoveScript.DoesMove = false;
        }

        private void RemoveStun()
        {
            _isStunned = false;
            _lastFireTime = Time.time;
            _basicMoveScript.DoesMove = true;
            _displacementLength += StunDuration * _basicMoveScript.MoveSpeed;
        }

        public void TriggerSpeedBoost()
        {
            EventLogger.PrintToLog("Enemy Gains Speedup");
            if (!_speedBoostIsActive)
            {
                _speedBoostIsActive = true;
                _basicMoveScript.SpeedCoef = SpeedBoostCoef;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_hasCollided)
            {
                return;
            }

            if (other.gameObject.tag == "Player")
            {
                Assert.IsNotNull(_playerScript);
                bool playerGotHit = _playerScript.PlayerGotHit();
                if (playerGotHit)
                {
                    EventLogger.PrintToLog("Enemy Collision v Player");
                    Explode();
                }
            }
            else if (other.gameObject.tag == "Shield")
            {
                Assert.IsNotNull(_playerScript);
                bool shieldGotHit = _playerScript.ShieldGotHit();
                if (shieldGotHit)
                {
                    EventLogger.PrintToLog("Enemy Collision v Shield");
                    Explode();
                }
            }
            else if (other.gameObject.tag == "Enemy")
            {
                EventLogger.PrintToLog("Enemy Collision v Enemy");
                Explode();
            }
        }

        private void FireGun()
        {
            _lastFireTime = Time.time;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("BulletStart"))
                {
                    Instantiate(BulletPrefab, transform.GetChild(i).position, Quaternion.identity);
                }
            }
            SetNextFiringInterval();
        }

        private void SetNextFiringInterval()
        {
            float randomIntervalCoef = Random.Range(MinFiringInterval, 2 * MinFiringInterval);
            _nextFiringInterval = randomIntervalCoef / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
        }

        private void Explode()
        {
            if (_playerScript)
            {
                //player might not be alive, game might have ended, do not score negative points in this case
                _playerScript.TriggerEnemyDestruction();
            }
            _hasCollided = true;
            AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
            Destroy(gameObject);
        }
    }
}
