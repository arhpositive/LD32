
//this class will hold a single question related to the questionnaire
public class Question
{
	private string _question;
	private string[] _answers;
	private int _selectedAnswer;

	public Question(string question, params string[] answers)
	{
		_question = question;
		_answers = answers;
		_selectedAnswer = -1;
	}

	public string GetOptionText(int optionIndex)
	{
		return _answers[optionIndex];
	}

	public void AnswerQuestion(int selectedOptionIndex)
	{
		_selectedAnswer = selectedOptionIndex;
	}

	public int GetSelectedAnswer()
	{
		return _selectedAnswer;
	}
}
