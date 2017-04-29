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
			GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
			if (playerGameObject)
			{
				_playerScript = playerGameObject.GetComponent<Player>();
			}
		}

		private void Update()
		{
			if (_playerScript)
			{
				_accuracyText.text = _playerScript.GetAllTimeStats().PlayerAccuracy.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
