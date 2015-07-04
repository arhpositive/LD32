﻿/* 
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

        void Start()
        {
            DifficultyMultiplier = 1.0f; //the bigger the harder
        }

        void Update()
        {

        }

        public void IncreaseDifficulty(float value)
        {
            DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier + value, 0.1f, 2.0f);
        }

        public void DecreaseDifficulty(float value)
        {
            DifficultyMultiplier = Mathf.Clamp(DifficultyMultiplier - value, 0.1f, 2.0f);
        }
    }
}