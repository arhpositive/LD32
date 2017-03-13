/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * PlayerStats.cs
 * Handles player stats
 */

public class PlayerStats
{
    //TODO we should hold several statistics such as 
    // how much health player lost over a certain period of time
    // how often does the player manouver
    // what types of weapons does the player use often

    //TODO flesh it out, but first, carry accuracy here
    public float PlayerAccuracy;

    private int _hitBulletCount;
    private int _shotBulletCount;

    public PlayerStats()
    {
        _hitBulletCount = 0;
        _shotBulletCount = 0;
        PlayerAccuracy = 0.0f;
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
}
