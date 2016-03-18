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
	    private Text _difficultyText;
	    private DifficultyManager _difficultyManagerScript;

	    private void Start ()
		{
			_difficultyText = gameObject.GetComponent<Text>();
			_difficultyManagerScript = Camera.main.GetComponent<DifficultyManager>();
		}

	    private void Update ()
		{
			_difficultyText.text = _difficultyManagerScript.DifficultyMultiplier.ToString();
		}
	}
}
