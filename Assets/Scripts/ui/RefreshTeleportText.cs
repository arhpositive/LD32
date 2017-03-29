/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshPowerupText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace ui
{
	public class RefreshTeleportText : MonoBehaviour
	{
		private Text _teleportText;
		private Player _playerScript;

		private void Start()
		{
			_teleportText = gameObject.GetComponent<Text>();
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
				_teleportText.text = _playerScript.GetTeleportGunAmmo().ToString();
			}
		}
	}
}
