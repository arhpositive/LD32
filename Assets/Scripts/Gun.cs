using UnityEngine;
using UnityEngine.Assertions;

public enum GunType
{
    GtStun,
    GtSpeedUp,
    GtTeleport,
    GtCount
}

public class Gun
{
    public float Cooldown { get; private set; }
    public float LastFireTime { get; private set; }
    public int CurrentAmmoCount { get; private set; }
    public GameObject LastBullet { get; private set; }
    public bool CanBeFired { get; private set; }

    private GameObject _bulletPrefab;
    private AudioClip _gunAudioClip;
    private bool _initiallyUsesAmmo;
    private bool _currentlyUsesAmmo;
    private int _initialAmmoCount;

    public Gun(GameObject bulletPrefab, AudioClip gunAudioClip, float cooldown, bool usesAmmo = false, int initialAmmoCount = -1)
    {
        Cooldown = cooldown;
        LastFireTime = -cooldown;
        CurrentAmmoCount = initialAmmoCount;
        LastBullet = null;
        CanBeFired = true;

        _bulletPrefab = bulletPrefab;
        _gunAudioClip = gunAudioClip;
        _initiallyUsesAmmo = usesAmmo;
        _currentlyUsesAmmo = usesAmmo;
        _initialAmmoCount = initialAmmoCount;
    }

    public void Fire(Vector3 bulletStartPoint)
    {
        Assert.IsTrue(CanBeFired);
        LastFireTime = Time.time;
        LastBullet = Object.Instantiate(_bulletPrefab, bulletStartPoint, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_gunAudioClip, bulletStartPoint);
        if (_currentlyUsesAmmo)
        {
            --CurrentAmmoCount;
        }
    }

    public void PickupAmmo()
    {
        ++CurrentAmmoCount;
    }

    public void DepleteAmmo()
    {
        if (_currentlyUsesAmmo)
        {
            CurrentAmmoCount = 0;
        }
    }

    public float GetCooldownPercentage()
    {
        return (Time.time - LastFireTime) / Cooldown;
    }

    public void SetCanBeFired(bool value)
    {
        CanBeFired = value;
    }

    public void ResetLastBullet()
    {
        Object.Destroy(LastBullet.gameObject);
        LastBullet = null;
    }

    public void SetAmmoUsage(bool value)
    {
        _currentlyUsesAmmo = value;
    }

    public void ResetAmmoUsage()
    {
        _currentlyUsesAmmo = _initiallyUsesAmmo;
    }

    public void ResetAmmoCount()
    {
        CurrentAmmoCount = _initialAmmoCount;
    }
}
