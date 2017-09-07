/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * LoadLevel.cs
 * Switches between scenes
 */
 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadLevel : MonoBehaviour
{
	public GameObject TutorialToggleObject;
	public GameObject QuestionnaireToggleObject;
	
	public static bool TutorialToggleValue;
	public static bool QuestionnaireToggleValue;
    // ReSharper disable once RedundantDefaultMemberInitializer
    private static bool _isInitialized = false;

	private void Start()
	{
	    if (TutorialToggleObject)
	    {
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
			Toggle toggleComponent = QuestionnaireToggleObject.GetComponent<Toggle>();
			if (_isInitialized)
			{
				toggleComponent.isOn = QuestionnaireToggleValue;
			}
			else
			{
				QuestionnaireToggleValue = toggleComponent.isOn;
			}
		}

		_isInitialized = true;
	}

	public void LoadSceneWithIndex(int levelIndex)
	{
		SceneManager.LoadScene(levelIndex);
	}

	public void OnTutorialButtonClick(bool newValue)
	{
		TutorialToggleValue = newValue;
	}

	public void OnQuestionnaireButtonClick(bool newValue)
	{
		QuestionnaireToggleValue = newValue;
	}
}