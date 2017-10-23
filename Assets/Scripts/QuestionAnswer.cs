/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * QuestionAnswer.cs
 * A single question answer related to the questionnaire
 */

public class QuestionAnswer
{
	private string _answerText;
	private float _difficultyWeight;

	public QuestionAnswer(string answerText, float difficultyWeight)
	{
		_answerText = answerText;
		_difficultyWeight = difficultyWeight;
	}

	public string GetQuestionAnswerText()
	{
		return _answerText;
	}

	public float GetDifficultyWeight()
	{
		return _difficultyWeight;
	}
}
