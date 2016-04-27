﻿/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * DifficultyManager.cs
 * Checks regularly for difficulty changing situations and adjusts difficulty parameters
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum DifficultyParameter
{
    DpShipFireRateIncrease,
    DpWaveSpawnRateIncrease,
    DpPosPowerupSpawnRateDecrease,
    DpNegPowerupSpawnRateIncrease,
    DpEnemyShipStrength,
    DpCount
}

public class DifficultyManager : MonoBehaviour
{
    // TODO NEXT we have to define lots of difficulty multipliers here

    // these difficulty multipliers will be given their initial value via our learning program
        // we'll give the players a small questionnaire at the beginning of the game
        // based on the answers to these questions, our learning program will place players into a preset model
        // this preset model will set the initial difficulty parameters of the game

    // at the very beginning of the project, these multipliers can be set to 1.0 * (base_difficulty_level_coef)
    // base level coef can be 0.6f for easy, 1.0 for normal, 1.4f for hard (this is just an example)

    public Dictionary<DifficultyParameter, float> DifficultyCoefs { get; private set; }

    // difficulty adjustment steps will be at regular intervals
    private float _lastDifficultyAdjustmentTime;
    private const float DifficultyAdjustmentInterval = 5.0f; //TODO set this constant on par with what wave spawns will have for their initial value

    //TODO we also need to define a time interval to measure the effectiveness of our last difficulty adjustment

    //TODO difficulty adjustment coefficients will be determined by learning algorithm for every parameter separately, 
    // and they'll change for each adjustment depending on what size of a step the learning algorithm wants to take

    private GameObject _playerGameObject;
    private Player _playerScript;

    private int _adjustmentStepCount;
    private int _previousWavePlayerHealth;

    private void Start()
    {
        //TODO later on, these multipliers have to be pulled out from our learning data
        float avgDifficulty = (GameConstants.MaxDifficultyMultiplier + GameConstants.MinDifficultyMultiplier)/2.0f;
        DifficultyCoefs = new Dictionary<DifficultyParameter, float>((int)DifficultyParameter.DpCount);
        for (DifficultyParameter curParam = DifficultyParameter.DpShipFireRateIncrease; curParam < DifficultyParameter.DpCount; ++curParam)
        {
            DifficultyCoefs.Add(curParam, avgDifficulty);
        }

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
        //TODO measure the effectiveness of last difficulty adjustment

        //check if we need a new difficulty adjustment step
        if (Time.time - _lastDifficultyAdjustmentTime > DifficultyAdjustmentInterval)
        {
            _adjustmentStepCount++;
            RequestNewDifficultyAdjustment();
            _lastDifficultyAdjustmentTime = Time.time;
        }
    }

    private void RequestNewDifficultyAdjustment()
    {
        //TODO this method will send the learning system a message to notify that we're on a new adjustment step
        // for now we'll just directly connect this to the response as if the system has replied with a proper answer

        //you'll have all kinds of raw statistics the game can supply you with, here
        //you'll make decisions depending on the game state to slightly alter the difficulty level

        //for now, lets put up some examples
        //do not consider difficulty numbers going off the limits here, they won't be of any problem once ML system is online

        float hpDiffSinceLastAdjustment = _playerScript.PlayerHealth - _previousWavePlayerHealth;

        if (hpDiffSinceLastAdjustment < 0.0f)
        {
            // player lost hp during last 5 seconds, drop difficulty
            RandomDiffAdjustment(false);
        }

        if (_adjustmentStepCount % 6 == 0 && _playerScript.PlayerHealth > 1)
        {
            // increase difficulty every 30 seconds if player is not struggling
            RandomDiffAdjustment(true);
        }

        _previousWavePlayerHealth = _playerScript.PlayerHealth;

    }

    private void RandomDiffAdjustment(bool isIncrement)
    {
        DifficultyParameter selectedDifficultyParameter = (DifficultyParameter)Random.Range((int)DifficultyParameter.DpShipFireRateIncrease,
                        (int)DifficultyParameter.DpCount);
        float oldValue = DifficultyCoefs[selectedDifficultyParameter];

        //TODO remove below code after ML is implemented
        //start
        int numRetries = 3;
        for (int i = 0; i < numRetries || (isIncrement ? (oldValue <= GameConstants.MinDifficultyMultiplier) : (oldValue >= GameConstants.MaxDifficultyMultiplier)); ++i)
        {
            selectedDifficultyParameter = (DifficultyParameter)Random.Range((int)DifficultyParameter.DpShipFireRateIncrease,
                    (int)DifficultyParameter.DpCount);
            oldValue = DifficultyCoefs[selectedDifficultyParameter];
        }
        //end

        ChangeDifficultyParameter(selectedDifficultyParameter, oldValue + (0.1f * (isIncrement ? 1 : -1)));
    }

    private void ChangeDifficultyParameter(DifficultyParameter difficultyParameter, float newValue)
    {
        //Assert.IsTrue(newValue >= GameConstants.MinDifficultyMultiplier && newValue <= GameConstants.MaxDifficultyMultiplier);
        DifficultyCoefs[difficultyParameter] = Mathf.Clamp(newValue, GameConstants.MinDifficultyMultiplier, GameConstants.MaxDifficultyMultiplier);
        EventLogger.PrintToLog("Difficulty Changed: " + difficultyParameter + " Value: " + newValue);
    }

    public float GetAverageDifficultyLevel()
    {
        float avgDifficulty = 0.0f;
        for (DifficultyParameter curParam = DifficultyParameter.DpShipFireRateIncrease; curParam < DifficultyParameter.DpCount; ++curParam)
        {
            avgDifficulty += DifficultyCoefs[curParam];
        }
        avgDifficulty /= (int)DifficultyParameter.DpCount;

        return avgDifficulty;
    }
}