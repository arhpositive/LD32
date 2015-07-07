/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using UnityEngine;
using Assets.Scripts.ui;

namespace Assets.Scripts
{
    struct Gun
    {
        public GameObject BulletPrefab { get; private set; }
        public float Cooldown { get; private set; }
        public float LastFireTime;
        public int AmmoCount;

        public Gun(GameObject bulletPrefab, float cooldown, float lastFireTime, int ammoCount)
            : this()
        {
            BulletPrefab = bulletPrefab;
            Cooldown = cooldown;
            LastFireTime = lastFireTime;
            AmmoCount = ammoCount;
        }
    }

    public class Player : MonoBehaviour
    {
        public static int PlayerScore = 0;

        public GameObject StunBulletPrefab;
        public GameObject SpeedUpBulletPrefab;
        public float PlayerSpeedLimit;
        public float PlayerAcceleration;

        public AudioClip FireStunGunClip;
        public AudioClip FireSpeedUpGunClip;
        public AudioClip ExplosionClip;

        public int PlayerHealth { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public bool IsShielded { get; private set; }

        RefreshEndScoreText _endGameScoreText;

        GameObject _playerShield;

        float _currentHorizontalSpeed;
        float _currentVerticalSpeed;

        const float DeathDuration = 2.0f;
        float _deathTime;

        const float InvulnerabilityDuration = 2.0f;
        float _invulnerabilityStartTime;

        SpriteRenderer _spriteRenderer;
        SpriteRenderer[] _childRenderers;

        Gun _stunGun;
        Gun _speedUpGun;

        void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            _endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();
        }

        void Start()
        {
            PlayerScore = 0;
            PlayerHealth = GameConstants.PlayerInitialHealth;
            IsDead = false;
            IsShielded = false;
            IsInvulnerable = true;
            _invulnerabilityStartTime = Time.time;
            _currentHorizontalSpeed = 0.0f;
            _currentVerticalSpeed = 0.0f;

            _stunGun = new Gun(StunBulletPrefab, 0.3f, 0.0f, -1);
            _speedUpGun = new Gun(SpeedUpBulletPrefab, 0.5f, 0.0f, 3);

            //find shield object in children
            foreach (Transform tr in transform)
            {
                if (tr.tag == "Shield")
                {
                    _playerShield = tr.gameObject;
                    break;
                }
            }
        }

        void Update()
        {
            if (IsDead)
            {
                if (Time.time - _deathTime > DeathDuration)
                {
                    //rise from dead
                    IsDead = false;
                    _spriteRenderer.enabled = true;
                    SetChildRenderers(true);

                    //spawn
                    transform.position = new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit,
                        GameConstants.MaxVerticalMovementLimit));
                    IsInvulnerable = true;
                    _invulnerabilityStartTime = Time.time;
                }
                else
                {
                    return;
                }
            }

            //invulnerability
            if (IsInvulnerable)
            {
                if ((Time.time - _invulnerabilityStartTime) % 0.5f > 0.25f)
                {
                    _spriteRenderer.enabled = false;
                    SetChildRenderers(false);
                }
                else
                {
                    _spriteRenderer.enabled = true;
                    SetChildRenderers(true);
                }
            }

            //end of invulnerability
            if (Time.time - _invulnerabilityStartTime > InvulnerabilityDuration)
            {
                IsInvulnerable = false;
                _spriteRenderer.enabled = true;
                SetChildRenderers(true);
            }

            //shooting
            if (Input.GetKey(KeyCode.Space) && Time.time - _stunGun.LastFireTime > _stunGun.Cooldown)
            {
                FireStunGun();
            }
            else if (Input.GetKey(KeyCode.C) &&
                Time.time - _speedUpGun.LastFireTime > _speedUpGun.Cooldown &&
                _speedUpGun.AmmoCount > 0)
            {
                FireSpeedUpGun();
                //TODO consider giving info on why player can't shoot
                //TODO consider displaying cooldown on ui
            }

            //movement
            Vector2 inputDir = GetMoveDirFromInput();

            if (inputDir.x > Mathf.Epsilon && _currentHorizontalSpeed >= 0.0f)
            {
                _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed + PlayerAcceleration, 0.0f, 1.0f);
            }
            else if (inputDir.x < -Mathf.Epsilon && _currentHorizontalSpeed <= 0.0f)
            {
                _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed - PlayerAcceleration, -1.0f, 0.0f);
            }
            else
            {
                _currentHorizontalSpeed = 0.0f;
            }

            if (inputDir.y > Mathf.Epsilon && _currentVerticalSpeed >= 0.0f)
            {
                _currentVerticalSpeed = Mathf.Clamp(_currentVerticalSpeed + PlayerAcceleration, 0.0f, 1.0f);
            }
            else if (inputDir.y < -Mathf.Epsilon && _currentVerticalSpeed <= 0.0f)
            {
                _currentVerticalSpeed = Mathf.Clamp(_currentVerticalSpeed - PlayerAcceleration, -1.0f, 0.0f);
            }
            else
            {
                _currentVerticalSpeed = 0.0f;
            }

            Vector2 movementDir = Vector2.ClampMagnitude(new Vector2(_currentHorizontalSpeed, _currentVerticalSpeed), 1.0f);
            movementDir *= PlayerSpeedLimit * Time.deltaTime;
            transform.Translate(movementDir, Space.World);

            if (transform.position.x < GameConstants.MinHorizontalMovementLimit ||
                transform.position.x > GameConstants.MaxHorizontalMovementLimit)
            {
                _currentHorizontalSpeed = 0.0f;
            }
            if (transform.position.y < GameConstants.MinVerticalMovementLimit ||
                transform.position.y > GameConstants.MaxVerticalMovementLimit)
            {
                _currentVerticalSpeed = 0.0f;
            }

            transform.position = new Vector3(Mathf.Clamp(transform.position.x, GameConstants.MinHorizontalMovementLimit,
                GameConstants.MaxHorizontalMovementLimit), Mathf.Clamp(transform.position.y, GameConstants.MinVerticalMovementLimit,
                GameConstants.MaxVerticalMovementLimit), transform.position.z);
        }

        Vector2 GetMoveDirFromInput()
        {
            float horizontalMoveDir = Input.GetAxisRaw("Horizontal");
            float verticalMoveDir = Input.GetAxisRaw("Vertical");

            Vector2 moveDir = new Vector2(horizontalMoveDir, verticalMoveDir);
            moveDir.Normalize();

            return moveDir;
        }

        public bool PlayerGotHit()
        {
            if (IsInvulnerable || IsDead)
            {
                // bullet goes right through player
                return false;
            }
            else
            {
                PlayerHealth--;

                if (PlayerHealth == 0)
                {
                    _endGameScoreText.SetTextVisible();
                    Destroy(gameObject);
                }
                else
                {
                    if (IsShielded)
                    {
                        ShieldGotHit();
                    }
                    AudioSource.PlayClipAtPoint(ExplosionClip, transform.position);
                    _deathTime = Time.time;
                    IsDead = true;
                    _spriteRenderer.enabled = false;
                    SetChildRenderers(false);
                }
                return true;
            }
        }

        public bool ShieldGotHit()
        {
            if (IsInvulnerable || IsDead)
            {
                return false;
            }

            IsShielded = false;
            _playerShield.SetActive(false);
            return true;
        }

        public void TriggerEnemyDestruction()
        {
            PlayerScore -= 100;
        }

        public void TriggerEnemyDisplacement(int scoreAddition)
        {
            PlayerScore += scoreAddition;
        }

        public void TriggerHealthPickup()
        {
            PlayerHealth++;
        }

        public void TriggerSpeedUpPickup()
        {
            _speedUpGun.AmmoCount++;
        }

        public void TriggerResearchPickup()
        {
            PlayerScore += 50;
        }

        public void TriggerShieldPickup()
        {
            if (!IsShielded)
            {
                IsShielded = true;
                _playerShield.SetActive(true);
                //TODO play shield sound effect
            }
            else
            {
                PlayerScore += 10;
            }
        }

        public int GetSpeedUpGunAmmo()
        {
            return _speedUpGun.AmmoCount;
        }

        void SetChildRenderers(bool value)
        {
            foreach (SpriteRenderer r in _childRenderers)
            {
                if (r.gameObject.tag == "Shield")
                {
                    if (IsShielded)
                    {
                        r.enabled = value;
                    }
                }
                else
                {
                    r.enabled = value;
                }
            }
        }

        void FireStunGun()
        {
            _stunGun.LastFireTime = Time.time;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("BulletStart"))
                {
                    Vector3 bulletStartPoint = transform.GetChild(i).position;
                    Instantiate(_stunGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
                    AudioSource.PlayClipAtPoint(FireStunGunClip, transform.GetChild(i).position);
                }
            }
        }

        void FireSpeedUpGun()
        {
            _speedUpGun.LastFireTime = Time.time;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("BulletStart"))
                {
                    Vector3 bulletStartPoint = transform.GetChild(i).position;
                    Instantiate(_speedUpGun.BulletPrefab, bulletStartPoint, Quaternion.identity);
                    _speedUpGun.AmmoCount--;
                    AudioSource.PlayClipAtPoint(FireSpeedUpGunClip, bulletStartPoint);
                }
            }
        }
    }
}
