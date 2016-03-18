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

        private float _lastDifficultyAdjustmentTime;
        private const float DifficultyAdjustmentInterval = 5.0f;
        private const float DifficultyCoefficient = 1.2f;

        private GameObject _playerGameObject;
        private Player _playerScript;

        private int _adjustmentStepCount;

        private int _previousWavePlayerHealth;

        private void Start()
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

            _previousWavePlayerHealth = Player.PlayerInitialHealth;
        }

        private void Update()
        {
            if (Time.time - _lastDifficultyAdjustmentTime > DifficultyAdjustmentInterval)
            {
                _adjustmentStepCount++;
                float hpDiffSinceLastAdjustment = _playerScript.PlayerHealth - _previousWavePlayerHealth;

                if (hpDiffSinceLastAdjustment < 0.0f)
                {
                    // player lost hp during last 5 seconds, drop difficulty
                    DecreaseDifficulty();
                }

                if (_adjustmentStepCount % 6 == 0 && _playerScript.PlayerHealth > 1)
                {
                    // increase difficulty every 30 seconds if player is not struggling
                    IncreaseDifficulty();
                }

                _previousWavePlayerHealth = _playerScript.PlayerHealth;

                _lastDifficultyAdjustmentTime = Time.time;
            }
        }

        private void IncreaseDifficulty()
        {
            float newDifficultyMultiplier = DifficultyMultiplier * DifficultyCoefficient;

            if (newDifficultyMultiplier <= GameConstants.MaxDifficultyMultiplier)
            {
                DifficultyMultiplier = newDifficultyMultiplier;
                EventLogger.PrintToLog("Difficulty Increased: " + DifficultyMultiplier);
            }
        }

        private void DecreaseDifficulty()
        {
            float newDifficultyMultiplier = DifficultyMultiplier / DifficultyCoefficient;

            if (newDifficultyMultiplier >= GameConstants.MinDifficultyMultiplier)
            {
                DifficultyMultiplier = newDifficultyMultiplier;
                EventLogger.PrintToLog("Difficulty Decreased: " + DifficultyMultiplier);
            }
        }
    }
}
