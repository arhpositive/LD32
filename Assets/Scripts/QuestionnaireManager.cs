/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * QuestionnaireManager.cs
 * Handles questionnaire flow
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//TODO LATER might get moved completely to ui namespace
public class QuestionnaireManager : MonoBehaviour
{
	public static float DeterminedInitialDifficultyCoef;

	public Transform QuestionnaireToggleGroupTransform;
	public Text QuestionText;
	public Text QuestionCountText;
	
	private List<Question> _questions;
	private int _currentQuestionIndex;
	private ToggleGroup _questionnaireToggleGroup;
	private List<GameObject> _questionnaireAnswerGameObjects;
	private List<Text> _questionnaireAnswerTexts;
	
	private void Awake ()
	{
		_questionnaireToggleGroup = QuestionnaireToggleGroupTransform.GetComponent<ToggleGroup>();
		_questionnaireAnswerGameObjects = new List<GameObject>();
		_questionnaireAnswerTexts = new List<Text>();

		foreach (Transform child in QuestionnaireToggleGroupTransform)
		{
			_questionnaireAnswerGameObjects.Add(child.gameObject);
			_questionnaireAnswerTexts.Add(child.gameObject.GetComponentInChildren<Text>());
		}
		
		_questions = new List<Question>
		{
			//we will take the average weight of given responses and start the game with a difficulty level
			//that takes this average weight into account
			//TODO LATER later on these questionnaires will give their own answer as scores of players will change weighting
			
			new Question("What is your age range?", 
				new QuestionAnswer("0-17", 2.0f), 
				new QuestionAnswer("18-26", 3.0f), 
				new QuestionAnswer("27-44", 2.0f), 
				new QuestionAnswer("45+", 1.0f)),
			new Question("How many hours in a week are you spending playing video games?", 
				new QuestionAnswer("Less than 2", 1.0f), 
				new QuestionAnswer("Between 2 and 10", 2.0f),
				new QuestionAnswer("Between 10 and 20", 3.0f), 
				new QuestionAnswer("More than 20", 4.0f)),
			new Question("Have you played shoot'em up games before?", 
				new QuestionAnswer("No, I have not.", 1.0f), 
				new QuestionAnswer("Only a few.", 2.0f), 
				new QuestionAnswer("I play them a lot.", 4.0f))
		};
	}

	private void OnEnable()
	{
		DeterminedInitialDifficultyCoef = GameConstants.StartDifficulty;
		_currentQuestionIndex = -1;
		DisplayNextQuestion();
	}

	private void DisplayNextQuestion()
	{
		//increment question index
		++_currentQuestionIndex;

		//reset toggles
		_questionnaireToggleGroup.SetAllTogglesOff();
		foreach (GameObject go in _questionnaireAnswerGameObjects)
		{
			go.SetActive(true);
		}

		//display next question in ui
		Question nextQuestion = _questions[_currentQuestionIndex];
		int optionCount = nextQuestion.GetOptionCount();
		QuestionCountText.text = "Question " + (_currentQuestionIndex + 1) + " / " + _questions.Count;
		QuestionText.text = nextQuestion.GetQuestionText();

		for (int i = 0; i < _questionnaireAnswerTexts.Count; ++i)
		{
			if (i >= optionCount)
			{
				_questionnaireAnswerGameObjects[i].SetActive(false);
			}
			else
			{
				
				_questionnaireAnswerTexts[i].text = nextQuestion.GetOptionText(i);
			}
		}
	}
	
	public void AnswerCurrentQuestion()
	{
		Toggle selectedToggle = _questionnaireToggleGroup.ActiveToggles().FirstOrDefault();
		if (selectedToggle)
		{
			//set question answer
			_questions[_currentQuestionIndex].AnswerQuestion(selectedToggle.gameObject.GetComponent<QuestionnaireToggle>().ToggleIndex);

			//skip to next question if an answer is selected
			if (_currentQuestionIndex + 1 < _questions.Count)
			{
				DisplayNextQuestion();
			}
			else
			{
				//the questionnaire is finished, start the game after processing the answers
				DetermineInitialGameDifficulty();
				LoadLevel.LoadSceneWithIndex(1);
			}
		}
	}

	private float GetMaxQuestionnaireDifficultyWeight()
	{
		float result = 0.0f;
		foreach (Question q in _questions)
		{
			//TODO LATER right now we're adding, see if we want to multiply instead
			result += q.GetMaxAnswerDifficultyWeight();
		}
		return result;
	}

	private float GetSelectedQuestionnaireDifficultyWeight()
	{
		float result = 0.0f;
		foreach (Question q in _questions)
		{
			result += q.GetSelectedAnswerDifficultyWeight();
		}
		return result;
	}

	private void DetermineInitialGameDifficulty()
	{
		float maxWeight = GetMaxQuestionnaireDifficultyWeight();
		float selectedWeight = GetSelectedQuestionnaireDifficultyWeight();
		
		float difficultyBoundsDiff = (GameConstants.MaxDifficulty - GameConstants.MinDifficulty) * 0.8f;

		//TODO LATER right now we're just assigning default difficulty values to options
		//we should determine these difficulty values from the choices past players have made
		DeterminedInitialDifficultyCoef = GameConstants.MinDifficulty + (selectedWeight / maxWeight) * difficultyBoundsDiff;
	}
}
