using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//TODO NEXT might get moved completely to ui namespace
public class QuestionnaireManager : MonoBehaviour
{
	public Transform QuestionnaireToggleGroupTransform;
	public Text _questionText;
	public Text _questionCountText;

	private LoadLevel _levelManager;
	private List<Question> _questions;
	private int _currentQuestionIndex;
	private ToggleGroup _questionnaireToggleGroup;
	private List<GameObject> _questionnaireAnswerGameObjects;
	private List<Text> _questionnaireAnswerTexts;
	
	private void Awake ()
	{
		_levelManager = Camera.main.gameObject.GetComponent<LoadLevel>();
		_questionnaireToggleGroup = QuestionnaireToggleGroupTransform.GetComponent<ToggleGroup>();
		_questionnaireAnswerGameObjects = new List<GameObject>();
		_questionnaireAnswerTexts = new List<Text>();

		foreach (Transform child in QuestionnaireToggleGroupTransform)
		{
			_questionnaireAnswerGameObjects.Add(child.gameObject);
			_questionnaireAnswerTexts.Add(child.gameObject.GetComponentInChildren<Text>());
		}

		//TODO NEXT write questions and link them together, put all your shit together, in a box, get your shit together, all your shit, together
		_questions = new List<Question>
		{
			new Question("What is your age range?", "0-17", "18-26", "27-44", "45+"),
			new Question("How many hours in a week are you spending playing video games?", "Less than 2", "Between 2 and 10",
				"Between 10 and 20", "More than 20"),
			new Question("Have you played shoot'em up games before?", "No, I have not.", "Only a few.", "I play them a lot.")
		};
	}

	private void OnEnable()
	{
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
		_questionCountText.text = "Question " + (_currentQuestionIndex + 1) + " / " + _questions.Count;
		_questionText.text = nextQuestion.GetQuestionText();

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
				_levelManager.LoadSceneWithIndex(1);
			}
		}
	}

	public void CancelQuestionnaire()
	{
		//TODO NEXT do stuff related to questionnaire being cancelled, rollback player model, erase answers, etc.
		//if you can't find anything to do, move this function entirely to loadLevel
		_levelManager.SwapActiveLeftPanel();
	}
}
