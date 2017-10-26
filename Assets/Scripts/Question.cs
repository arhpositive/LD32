/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Question.cs
 * A single question related to the questionnaire
 */

public class Question
{
	private string _question;
	private QuestionAnswer[] _answers;
	private int _selectedAnswer;
	private float _maxAnswerDifficultyWeight;

	public Question(string question, params QuestionAnswer[] answers)
	{
		_question = question;
		_answers = answers;
		_selectedAnswer = -1;

		_maxAnswerDifficultyWeight = 0.0f;
		foreach (QuestionAnswer answer in _answers)
		{
			float curDifficultyWeight = answer.GetDifficultyWeight();
			if (curDifficultyWeight > _maxAnswerDifficultyWeight)
			{
				_maxAnswerDifficultyWeight = curDifficultyWeight;
			}
		}
	}

	public string GetQuestionText()
	{
		return _question;
	}

	public string GetOptionText(int optionIndex)
	{
		return _answers[optionIndex].GetQuestionAnswerText();
	}

	public float GetMaxAnswerDifficultyWeight()
	{
		return _maxAnswerDifficultyWeight;
	}

	public float GetSelectedAnswerDifficultyWeight()
	{
		return _answers[_selectedAnswer].GetDifficultyWeight();
	}

	public void AnswerQuestion(int selectedOptionIndex)
	{
		_selectedAnswer = selectedOptionIndex;
	}

	public int GetSelectedAnswerIndex()
	{
		return _selectedAnswer;
	}

	public int GetOptionCount()
	{
		return _answers.Length;
	}
}
