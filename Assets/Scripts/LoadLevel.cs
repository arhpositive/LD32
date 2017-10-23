/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * LoadLevel.cs
 * Switches between scenes
 */
 
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadLevel : MonoBehaviour
{
	public GameObject StartGamePanel;
	public GameObject QuestionnairePanel;

	public GameObject TutorialToggleObject;
	public GameObject QuestionnaireToggleObject;
	
	public static bool TutorialToggleValue;

	private static bool _questionnaireToggleValue;
    // ReSharper disable once RedundantDefaultMemberInitializer
    private static bool _isInitialized = false;

	private void Start()
	{
		//initiate toggle parameters for the menu items

	    if (TutorialToggleObject)
	    {
		    Assert.IsTrue(TutorialToggleObject.activeSelf);
			Toggle toggleComponent = TutorialToggleObject.GetComponent<Toggle>();
	        if (_isInitialized)
	        {
	            toggleComponent.isOn = TutorialToggleValue;
	        }
	        else
	        {
	            TutorialToggleValue = toggleComponent.isOn;
	        }
        }
		
		if (QuestionnaireToggleObject)
		{
			Assert.IsTrue(QuestionnaireToggleObject.activeSelf);
			Toggle toggleComponent = QuestionnaireToggleObject.GetComponent<Toggle>();
			if (_isInitialized)
			{
				toggleComponent.isOn = _questionnaireToggleValue;
			}
			else
			{
				_questionnaireToggleValue = toggleComponent.isOn;
			}
		}

		_isInitialized = true;
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public void SwapActiveLeftPanel()
	{
		if (StartGamePanel.activeSelf)
		{
			Assert.IsTrue(!QuestionnairePanel.activeSelf);
			StartGamePanel.SetActive(false);
			QuestionnairePanel.SetActive(true);
		}
		else
		{
			Assert.IsTrue(!StartGamePanel.activeSelf && QuestionnairePanel.activeSelf);
			StartGamePanel.SetActive(true);
			QuestionnairePanel.SetActive(false);
		}
	}

	public void StartGame(int levelIndex)
	{
		//clicking play button on main menu

		if (_questionnaireToggleValue)
		{
			//if questionnaire is open, the player has to answer some questions
			SwapActiveLeftPanel();
		}
		else
		{
			//otherwise, just start the game
			SceneManager.LoadScene(levelIndex);
		}
	}

	public void CancelQuestionnaire()
	{
		SwapActiveLeftPanel();
	}

	public static void LoadSceneWithIndex(int levelIndex)
	{
		SceneManager.LoadScene(levelIndex);
	}

	public void OnTutorialButtonClick(bool newValue)
	{
		TutorialToggleValue = newValue;
	}

	public void OnQuestionnaireButtonClick(bool newValue)
	{
		_questionnaireToggleValue = newValue;
	}
}