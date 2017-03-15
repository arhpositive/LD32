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
		private Player _playerScript;

		private void Start()
		{
			_accuracyText = gameObject.GetComponent<Text>();
			_playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		}

		private void Update()
		{
			_accuracyText.text = _playerScript.AllPlayerStats[0].PlayerAccuracy.ToString(CultureInfo.InvariantCulture);
			
		}
	}
}
