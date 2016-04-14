/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshDifficultyText.cs
 */

using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace ui
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
	        float avgDifficulty = 0.0f;
	        for (DifficultyParameter curParam = DifficultyParameter.DpShipFireRate; curParam < DifficultyParameter.DpCount; ++curParam)
	        {
	            avgDifficulty += _difficultyManagerScript.DifficultyCoefs[curParam];
	        }
	        avgDifficulty /= (int)DifficultyParameter.DpCount;
			_difficultyText.text = avgDifficulty.ToString(CultureInfo.InvariantCulture);
		}
	}
}
