/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * DifficultyManager.cs
 * Checks regularly for difficulty changing situations and adjusts difficulty parameters
 */

using UnityEngine;

namespace Assets.Scripts
{
    public class DifficultyManager : MonoBehaviour
    {
        public float DifficultyMultiplier { get; private set; }

        float _lastDifficultyAdjustmentTime;
        const float DifficultyAdjustmentInterval = 5.0f;

        GameObject _playerGameObject;
        Player _playerScript;

        int _adjustmentStepCount;

        int _previousWavePlayerHealth;
        int _previousWavePlayerScore;

        float _totalPlayerHealthDiff;
        float _totalPlayerScoreDiff;

        void Start()
        {
            // higher difficulty multiplier equals a more challenging game
            DifficultyMultiplier = 1.0f;
            _lastDifficultyAdjustmentTime = Time.time;

            _playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (_playerGameObject)
            {
                _playerScript = _playerGameObject.GetComponent<Player>();
            }

            _adjustmentStepCount = 0;

            _previousWavePlayerHealth = GameConstants.PlayerInitialHealth;
            _previousWavePlayerScore = 0;

            _totalPlayerHealthDiff = 0.0f;
            _totalPlayerScoreDiff = 0.0f;
        }

        void Update()
        {
            if (Time.time - _lastDifficultyAdjustmentTime > DifficultyAdjustmentInterval)
            {
                _adjustmentStepCount++;

                //float scoreDiffSinceLastAdjustment = (Player.PlayerScore - _previousWavePlayerScore);
                float hpDiffSinceLastAdjustment = (_playerScript.PlayerHealth - _previousWavePlayerHealth);

                //print("Score Diff Since Adjustment: " + scoreDiffSinceLastAdjustment);
                //print("HP Diff Since Adjustment: " + hpDiffSinceLastAdjustment);

                //_totalPlayerScoreDiff += scoreDiffSinceLastAdjustment;
                //_totalPlayerHealthDiff += hpDiffSinceLastAdjustment;

                //print("Avg Score Difference: " + (_totalPlayerScoreDiff / _adjustmentStepCount));
                //print("Avg Health Difference: " + (_totalPlayerHealthDiff / _adjustmentStepCount));

                if (hpDiffSinceLastAdjustment < 0.0f)
                {
                    // player lost hp during last 5 seconds, drop difficulty
                    
                    DecreaseDifficulty(0.2f);
                    EventLogger.PrintToLog("Difficulty Decreased: " + DifficultyMultiplier);
                }
                else if (_adjustmentStepCount % 6 == 0 && _playerScript.PlayerHealth > 1)
                {
                    // increase difficulty every 30 seconds if player is not struggling
                    
                    IncreaseDifficulty(0.2f);
                    EventLogger.PrintToLog("Difficulty Increased: " + DifficultyMultiplier);
                }

                _previousWavePlayerHealth = _playerScript.PlayerHealth;
                //_previousWavePlayerScore = Player.PlayerScore;

                _lastDifficultyAdjustmentTime = Time.time;
            }
        }

        public void IncreaseDifficulty(float value)
        {
            DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier + value, 
                GameConstants.MinDifficultyMultiplier, GameConstants.MaxDifficultyMultiplier);
        }

        public void DecreaseDifficulty(float value)
        {
            DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier - value,
                GameConstants.MinDifficultyMultiplier, GameConstants.MaxDifficultyMultiplier);
        }
    }
}
