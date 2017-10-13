using System.Collections.Generic;
using UnityEngine;

public class QuestionnaireManager : MonoBehaviour
{
	private LoadLevel _levelManager;
	private List<Question> _questions;

	// Use this for initialization
	private void Start ()
	{
		_levelManager = Camera.main.gameObject.GetComponent<LoadLevel>();
		print("aand opeen!");
		//TODO NEXT write questions and link them together, put all your shit together, in a box, get your shit together, all your shit, together

		_questions.Add(new Question("What is your age range?", "0-17", "18-26", "27-44", "45+"));
		_questions.Add(new Question("How many hours in a week are you spending playing video games?", "Less than 2", "Between 2 and 10", "Between 10 and 20", "More than 20"));
		_questions.Add(new Question("Have you played shoot'em up games before?", "No, I have not.", "Only a few.", "I play them a lot."));
	}
	
	// Update is called once per frame
	private void Update ()
	{
		
	}

	private void OnEnable()
	{
		print("enabling comm link...");
	}

	public void CancelQuestionnaire()
	{
		//TODO NEXT do stuff related to questionnaire being cancelled, rollback player model, erase answers, etc.
		//if you can't find anything to do, move this function entirely to loadLevel
		_levelManager.SwapActiveLeftPanel();
	}
}
