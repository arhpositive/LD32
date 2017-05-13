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

	private void Start()
	{
		if (TutorialToggleObject)
		{
			TutorialToggleValue = TutorialToggleObject.GetComponent<Toggle>().isOn;
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