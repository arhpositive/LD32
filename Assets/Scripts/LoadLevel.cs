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
	
	public static bool TutorialToggleValue;
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
	            _isInitialized = true;
	        }
        }
	}

	public void LoadSceneWithIndex(int levelIndex)
	{
		SceneManager.LoadScene(levelIndex);
	}

	public void OnTutorialButtonClick(bool newValue)
	{
		TutorialToggleValue = newValue;
	}
}