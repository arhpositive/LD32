/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicEnemy.cs
 * Handles basic enemy behaviour
 */

using UnityEngine;

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

        GameObject _playerGameObject;
        Player _playerScript;
        DifficultyManager _difficultyManagerScript;
        BasicMove _basicMoveScript;

        bool _hasCollided;
        bool _isStunned;
        float _lastStunTime;
        bool _speedBoostIsActive;
        float _lastFireTime;
        float _nextFiringInterval;
        float _displacementLength; //used for scoring

        void Start()
        {
            _playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (_playerGameObject)
            {
                _playerScript = _playerGameObject.GetComponent<Player>();
            }
            _difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
            _basicMoveScript = gameObject.GetComponent<BasicMove>();

            _hasCollided = false;
            _isStunned = false;
            _lastStunTime = 0.0f;
            _speedBoostIsActive = false;
            _lastFireTime = Time.time;
            _nextFiringInterval = Random.Range(MinFiringInterval, 2 * MinFiringInterval) / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
            _displacementLength = 0.0f;
        }

        void Update()
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
                        _playerScript.TriggerEnemyDisplacement((int)(Mathf.Abs(_displacementLength) * 10.0f));
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

        void RemoveStun()
        {
            _isStunned = false;
            _lastFireTime = Time.time;
            _basicMoveScript.DoesMove = true;
            _displacementLength += StunDuration * _basicMoveScript.MoveSpeed;
        }

        public void TriggerSpeedBoost()
        {
            if (!_speedBoostIsActive)
            {
                _speedBoostIsActive = true;
                _basicMoveScript.SpeedCoef = SpeedBoostCoef;
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!_hasCollided)
            {
                if (other.gameObject.tag == "Player")
                {
                    Player playerScript = other.gameObject.GetComponent<Player>();

                    bool playerGotHit = playerScript.PlayerGotHit();
                    if (playerGotHit)
                    {
                        _hasCollided = true;
                        AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
                        Destroy(gameObject);
                    }
                }
                else if (other.gameObject.tag == "Shield")
                {
                    Player playerScript = other.gameObject.GetComponentInParent<Player>();

                    bool shieldGotHit = playerScript.ShieldGotHit();
                    if (shieldGotHit)
                    {
                        _hasCollided = true;
                        AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
                        Destroy(gameObject);
                    }
                }
                else if (other.gameObject.tag == "Enemy")
                {
                    _hasCollided = true;
                    AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
                    Destroy(gameObject);
                }
            }
        }

        void FireGun()
        {
            _lastFireTime = Time.time;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("BulletStart"))
                {
                    Instantiate(BulletPrefab, transform.GetChild(i).position, Quaternion.identity);
                }
            }
            float randomIntervalCoef = Random.Range(MinFiringInterval, 3 * MinFiringInterval);
            _nextFiringInterval = randomIntervalCoef / Mathf.Sqrt(_difficultyManagerScript.DifficultyMultiplier);
        }
    }
}
