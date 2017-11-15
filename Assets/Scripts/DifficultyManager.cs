/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * DifficultyManager.cs
 * Checks regularly for difficulty changing situations and adjusts difficulty parameters
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public enum DifficultyParameter
{
	DpShipFireRateIncrease,
	DpWaveSpawnRateIncrease, 
	DpHugeEnemySpawnRateIncrease,
    DpHugeEnemyIntrusionSize,
	DpWaveHasNoExitCoef,
	DpPosPowerupSpawnRateDecrease,
	DpNegPowerupSpawnRateIncrease,
	DpEnemyShipStrength,
    DpMinimumEnemyCountCoef,
	DpCount
}

public class DifficultyManager : MonoBehaviour
{
	//TODO LEARN player will be placed in one of the preset player models after the questionnaire ends

	// these difficulty multipliers will be given their initial value via our learning program
	// we'll give the players a small questionnaire at the beginning of the game
	// based on the answers to these questions, our learning program will place players into a preset model
	// this preset model will set the initial difficulty parameters of the game

	// at the very beginning of the project, these multipliers can be set to 1.0 * (base_difficulty_level_coef)
	// base level coef can be 0.6f for easy, 1.0 for normal, 1.4f for hard (this is just an example)

	public Dictionary<DifficultyParameter, int> DifficultyCoefs { get; private set; }

	// difficulty adjustment steps will be at regular intervals
	private float _lastDifficultyAdjustmentTime;
	private const float DifficultyAdjustmentInterval = 5.0f;

	//TODO LEARN we need to define a time interval to measure the effectiveness of our last difficulty adjustment

	//TODO LEARN difficulty adjustment coefficients will be determined by learning algorithm for every parameter separately, 
	// and they'll change for each adjustment depending on what size of a step the learning algorithm wants to take

	private GameObject _playerGameObject;
	private Player _playerScript;

	private int _adjustmentStepCount;
	private int _previousWavePlayerHealth;
	private bool _tutorialSequenceIsActive;

	private void Start()
	{
		DetermineInitialDifficultyLevels();

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
		//TODO LEARN measure the effectiveness of last difficulty adjustment

		if (!_tutorialSequenceIsActive)
		{
			//check if we need a new difficulty adjustment step
			if (Time.time - _lastDifficultyAdjustmentTime > DifficultyAdjustmentInterval)
			{
				_adjustmentStepCount++;
				RequestNewDifficultyAdjustment();
				_lastDifficultyAdjustmentTime = Time.time;
			}
		}
	}

	public void ChangeTutorialSequenceState(bool newState)
	{
		_tutorialSequenceIsActive = newState;

		//reset timer after tutorial ends
		if (!_tutorialSequenceIsActive)
		{
			_lastDifficultyAdjustmentTime = Time.time;
		}
	}

	private void DetermineInitialDifficultyLevels()
	{
		//pull avg difficulty from questionnaire
		float initialDifficultyGoal = QuestionnaireManager.DeterminedInitialDifficultyCoef;

		//TODO LATER temporarily, we're automatically trying to match starting difficulty level
		//with the determined initial difficulty
		//by increasing difficulty coefficients until we're close enough to the initial difficulty

		const int diffParamCount = (int)DifficultyParameter.DpCount;
		int[] difficultyWeights = Enumerable.Repeat(GameConstants.MinDifficulty, diffParamCount).ToArray();
		int currentWeightIndex = 0;

		float achievedDifficultyCoef = GameConstants.MinDifficulty;
		int requiredIncrementCount = 0;
		while (achievedDifficultyCoef < initialDifficultyGoal)
		{
			requiredIncrementCount += GameConstants.DifficultyStep;
			difficultyWeights[currentWeightIndex] += GameConstants.DifficultyStep;
			currentWeightIndex = (currentWeightIndex + 1) % diffParamCount;
			achievedDifficultyCoef = (float)(GameConstants.MinDifficulty * diffParamCount + requiredIncrementCount) / diffParamCount;
		}
		print("Target: " + initialDifficultyGoal + ", Achieved: " + achievedDifficultyCoef);

		//shuffle the array
		new System.Random().Shuffle(difficultyWeights);

		DifficultyCoefs = new Dictionary<DifficultyParameter, int>(diffParamCount);
		for (DifficultyParameter curParam = 0; curParam < DifficultyParameter.DpCount; ++curParam)
		{
			//TODO LEARN these multipliers have to be pulled out from our learning data (from existing player models)
			DifficultyCoefs.Add(curParam, difficultyWeights[(int)curParam]);
			print("Param: " + curParam + " Weight: " + DifficultyCoefs[curParam]);
		}
		print("Average Difficulty: " + GetAverageDifficultyLevel());
	}

	private void RequestNewDifficultyAdjustment()
	{
		//TODO LEARN this method will send the learning system a message to notify that we're on a new adjustment step
		// for now we'll just directly connect this to the response as if the system has replied with a proper answer

		//you'll have all kinds of raw statistics the game can supply you with, here
		//you'll make decisions depending on the game state to slightly alter the difficulty level

		//for now, lets put up some examples
		//do not consider difficulty numbers going off the limits here, they won't be of any problem once ML system is online

		// we hold several statistics such as 
		// how much health player lost over a certain period of time
		// how often does the player manouver
		// what types of weapons does the player use often

		if (_playerScript)
		{
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
	}

	private void RandomDiffAdjustment(bool isIncrement)
	{
		//TODO LEARN remove below code after ML is implemented
		//start
		int numRetries = 3;
		for (int i = 0; i < numRetries; ++i)
		{
		    DifficultyParameter selectedDifficultyParameter = (DifficultyParameter)Random.Range(0, (int)DifficultyParameter.DpCount);
		    int oldValue = DifficultyCoefs[selectedDifficultyParameter];

			if (isIncrement ? oldValue < GameConstants.MaxDifficulty : oldValue > GameConstants.MinDifficulty)
			{
				ChangeDifficultyParameter(selectedDifficultyParameter, oldValue + GameConstants.DifficultyStep * (isIncrement ? 1 : -1));
				break;
			}
		}
		//end
	}

	private void ChangeDifficultyParameter(DifficultyParameter difficultyParameter, int newValue)
	{
		Assert.IsTrue(newValue >= GameConstants.MinDifficulty && newValue <= GameConstants.MaxDifficulty);
		DifficultyCoefs[difficultyParameter] = newValue;
		EventLogger.PrintToLog("Difficulty Changed: " + difficultyParameter + " Value: " + newValue);
		print("Difficulty Changed: " + difficultyParameter + " Value: " + newValue);
	}

	public float GetAverageDifficultyLevel()
	{
		float avgDifficulty = 0.0f;
		for (DifficultyParameter curParam = 0; curParam < DifficultyParameter.DpCount; ++curParam)
		{
			avgDifficulty += DifficultyCoefs[curParam];
		}
		avgDifficulty /= (int)DifficultyParameter.DpCount;

		return avgDifficulty;
	}

	public float GetDifficultyMultiplier(DifficultyParameter difficultyParameter)
	{
        // returned values for parameters between 1 to 5 are the following:
        // 0.44 | 0.67 | 1 | 1.5 | 2.25

		int difficultyDifference = GameConstants.MidDifficulty - DifficultyCoefs[difficultyParameter];
		float difficultyMultiplier = Mathf.Pow(GameConstants.DifficultyCoef, difficultyDifference);
		return difficultyMultiplier;
	}
}