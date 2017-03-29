/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshHealthText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace ui
{
	public class RefreshHealthText : MonoBehaviour
	{
		private Text _healthText;
		private Player _playerScript;

		private void Start()
		{
			_healthText = gameObject.GetComponent<Text>();
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
				_healthText.text = _playerScript.PlayerHealth.ToString();
			}
			else
			{
				_healthText.text = "Dead";
			}
		}
	}
}
