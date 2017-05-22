/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshAccuracyText.cs
 */

using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace ui
{
	public class RefreshAccuracyText : MonoBehaviour
	{
		private Text _accuracyText;
		private StatsManager _statsManagerScript;

		private void Start()
		{
			_accuracyText = gameObject.GetComponent<Text>();
			_statsManagerScript = Camera.main.GetComponent<StatsManager>();
		}

		private void Update()
		{
			if (_statsManagerScript)
			{
				_accuracyText.text = _statsManagerScript.GetAllTimeStats().PlayerAccuracy.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
