/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Player.cs
 * Handles player movement and actions via given input
 */

using UnityEngine;
using CnControls;
using Assets.Scripts.ui;

namespace Assets.Scripts
{
    struct Gun
    {
        public GameObject BulletPrefab { get; private set; }
        public float Cooldown { get; private set; }
        public float LastFireTime;
        public int AmmoCount;
        public GameObject LastBullet;

        public Gun(GameObject bulletPrefab, float cooldown, int ammoCount)
            : this()
        {
            BulletPrefab = bulletPrefab;
            Cooldown = cooldown;
            AmmoCount = ammoCount;
            LastFireTime = 0.0f;
        }
    }

    public class Player : MonoBehaviour
    {
        public static int PlayerScore = 0;

        public bool UseTouchControls;

        public GameObject StunBulletPrefab;
        public GameObject SpeedUpBulletPrefab;
        public GameObject TeleportBulletPrefab;
        public float PlayerSpeedLimit;

        public AudioClip FireStunGunClip;
        public AudioClip FireSpeedUpGunClip;
        public AudioClip FireTeleportGunClip;
        public AudioClip ExplosionClip;

        public int PlayerHealth { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public bool IsShielded { get; private set; }
        public float PlayerAccuracy { get; private set; }

        public const int PlayerInitialHealth = 3;

        RefreshEndScoreText _endGameScoreText;

        GameObject _playerShield;
        
        int _hitBulletCount;
        int _shotBulletCount;

        const float DeathDuration = 2.0f;
        float _deathTime;

        const float InvulnerabilityDuration = 2.0f;
        float _invulnerabilityStartTime;

        SpriteRenderer _spriteRenderer;
        SpriteRenderer[] _childRenderers;

        Gun _stunGun;
        Gun _speedUpGun;
        Gun _teleportGun;

        void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _childRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            _endGameScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<RefreshEndScoreText>();
        }

        void Start()
        {
            PlayerScore = 0;
            PlayerHealth = PlayerInitialHealth;
            IsDead = false;
            IsShielded = false;
            IsInvulnerable = true;
            _invulnerabilityStartTime = Time.time;

            PlayerAccuracy = 0.0f;
            _hitBulletCount = 0;
            _shotBulletCount = 0;

            _stunGun = new Gun(StunBulletPrefab, 0.3f, -1);
            _speedUpGun = new Gun(SpeedUpBulletPrefab, 0.5f, 3);
            _teleportGun = new Gun(TeleportBulletPrefab, 1.0f, 10);

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
                if (Time.time - _deathTime <= DeathDuration)
                {
                    return;
                }

                EventLogger.PrintToLog("Player Respawns");

                //rise from dead
                IsDead = false;
                _spriteRenderer.enabled = true;
                SetChildRenderers(true);

                EventLogger.PrintToLog("Invulnerability Start");

                //spawn
                transform.position = new Vector2(0.0f, Random.Range(GameConstants.MinVerticalMovementLimit,
                    GameConstants.MaxVerticalMovementLimit));
                IsInvulnerable = true;
                _invulnerabilityStartTime = Time.time;
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
            if (Time.time - _invulnerabilityStartTime > InvulnerabilityDuration && IsInvulnerable)
            {
                EventLogger.PrintToLog("Invulnerability End");

                IsInvulnerable = false;
                _spriteRenderer.enabled = true;
                SetChildRenderers(true);
            }

            //shooting
            bool fireInputGiven;
            bool speedUpInputGiven;
            bool teleportInputGiven; //teleport device will teleport our ship to wherever the beacon is

            if (UseTouchControls)
            {
                fireInputGiven = CnInputManager.GetButton("TouchFire");
                speedUpInputGiven = CnInputManager.GetButtonDown("TouchSpeedUp");
                teleportInputGiven = false; //TODO LATER touch control for third weapon
            }
            else
            {
                fireInputGiven = Input.GetKey(KeyCode.Z);
                speedUpInputGiven = Input.GetKeyDown(KeyCode.X);
                teleportInputGiven = Input.GetKeyDown(KeyCode.C);
            }

            if (fireInputGiven && Time.time - _stunGun.LastFireTime > _stunGun.Cooldown)
            {
                FireStunGun();
            }

            if (speedUpInputGiven &&
                Time.time - _speedUpGun.LastFireTime > _speedUpGun.Cooldown &&
                _speedUpGun.AmmoCount > 0)
            {
                FireSpeedUpGun();
                //TODO LATER display weapon cooldowns on UI
                //cooldown can be represented by a bar filling up on the area that shows the weapon type
            }

            if (teleportInputGiven)
            {
                if (_teleportGun.LastBullet)
                {
                    TriggerTeleportBullet();
                }
                else if (Time.time - _teleportGun.LastFireTime > _teleportGun.Cooldown &&
                         _teleportGun.AmmoCount > 0)
                {
                    FireTeleportGun();
                }
            }

            DoMovement();
        }

        Vector2 GetMoveDirFromInput()
        {
            float horizontalMoveDir;
            float verticalMoveDir;
            if (UseTouchControls)
            {
                horizontalMoveDir = CnInputManager.GetAxis("Horizontal");
                horizontalMoveDir = Mathf.Abs(horizontalMoveDir) < GameConstants.JoystickDeadZoneCoef ? 
                    0.0f : Mathf.Sign(horizontalMoveDir);

                verticalMoveDir = CnInputManager.GetAxis("Vertical");
                verticalMoveDir = Mathf.Abs(verticalMoveDir) < GameConstants.JoystickDeadZoneCoef ?
                    0.0f : Mathf.Sign(verticalMoveDir);
            }
            else
            {
                horizontalMoveDir = Input.GetAxisRaw("Horizontal");
                verticalMoveDir = Input.GetAxisRaw("Vertical"); 
            }

            Vector2 moveDir = new Vector2(horizontalMoveDir, verticalMoveDir);
            moveDir.Normalize();
            return moveDir;
        }

        void DoMovement()
        {
            //movement
            Vector2 inputDir = GetMoveDirFromInput();

            Vector2 movementDir = Vector2.ClampMagnitude(inputDir, 1.0f);
            movementDir *= PlayerSpeedLimit * Time.deltaTime;
            transform.Translate(movementDir, Space.World);

            transform.position = new Vector3(Mathf.Clamp(transform.position.x, GameConstants.MinHorizontalMovementLimit,
                GameConstants.MaxHorizontalMovementLimit), Mathf.Clamp(transform.position.y, GameConstants.MinVerticalMovementLimit,
                GameConstants.MaxVerticalMovementLimit), transform.position.z);
        }

        public bool PlayerGotHit()
        {
            if (IsInvulnerable || IsDead)
            {
                // bullet goes right through player
                return false;
            }

            PlayerHealth--;

            if (PlayerHealth == 0)
            {
                EventLogger.PrintToLog("Player Dies: Game Over");
                _endGameScoreText.SetTextVisible();
                Destroy(gameObject);
            }
            else
            {
                EventLogger.PrintToLog("Player Loses Health");
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

        public bool ShieldGotHit()
        {
            if (IsInvulnerable || IsDead)
            {
                return false;
            }

            EventLogger.PrintToLog("Player Loses Shield");

            IsShielded = false;
            _playerShield.SetActive(false);
            return true;
        }

        public void OnBulletDestruction(bool bulletHitEnemy)
        {
            _shotBulletCount++;
            if (bulletHitEnemy)
            {
                _hitBulletCount++;
            }

            PlayerAccuracy = (float)_hitBulletCount / _shotBulletCount;
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
            EventLogger.PrintToLog("Player Gains Health");
            PlayerHealth++;
        }

        public void TriggerSpeedUpPickup()
        {
            EventLogger.PrintToLog("Player Gains Speedup Powerup");
            _speedUpGun.AmmoCount++;
        }

        public void TriggerResearchPickup()
        {
            EventLogger.PrintToLog("Player Gains Research Powerup");
            PlayerScore += 50;
        }

        public void TriggerShieldPickup()
        {
            EventLogger.PrintToLog("Player Gains Shield");
            if (!IsShielded)
            {
                IsShielded = true;
                _playerShield.SetActive(true);
            }
            else
            {
                PlayerScore += 10;
            }
        }

        public void TriggerTeleportPickup()
        {
            EventLogger.PrintToLog("Player Gains Teleport Powerup");
            _teleportGun.AmmoCount++;
        }

        public int GetSpeedUpGunAmmo()
        {
            return _speedUpGun.AmmoCount;
        }

        public int GetTeleportGunAmmo()
        {
            return _teleportGun.AmmoCount;
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
                if (transform.GetChild(i).CompareTag("BulletStart")) //TODO LATER experiment with multiple gun slots
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

        void FireTeleportGun()
        {
            _teleportGun.LastFireTime = Time.time;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).CompareTag("BulletStart"))
                {
                    Vector3 bulletStartPoint = transform.GetChild(i).position;
                    _teleportGun.LastBullet =
                        Instantiate(_teleportGun.BulletPrefab, bulletStartPoint, Quaternion.identity) as GameObject;
                    _teleportGun.AmmoCount--;
                    AudioSource.PlayClipAtPoint(FireTeleportGunClip, bulletStartPoint);
                }
            }
        }

        void TriggerTeleportBullet()
        {
            if (_teleportGun.LastBullet)
            {
                EventLogger.PrintToLog("Player Triggers Teleport");
                transform.position = _teleportGun.LastBullet.transform.position;
                AudioSource.PlayClipAtPoint(_teleportGun.LastBullet.GetComponent<Bullet>().BulletHitClip, transform.position);
                
                Destroy(_teleportGun.LastBullet.gameObject);
                _teleportGun.LastBullet = null;
            }
        }
    }
}
