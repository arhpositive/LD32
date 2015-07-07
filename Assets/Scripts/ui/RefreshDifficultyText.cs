/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshDifficultyText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
	public class RefreshDifficultyText : MonoBehaviour
	{
		Text _difficultyText;
		DifficultyManager _difficultyManagerScript;

		void Start ()
		{
			_difficultyText = gameObject.GetComponent<Text>();
			_difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
		}
	
		void Update ()
		{
			_difficultyText.text = _difficultyManagerScript.DifficultyMultiplier.ToString();
		}
	}
}
