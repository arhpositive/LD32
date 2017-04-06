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
	public class RefreshPowerupText : MonoBehaviour
	{
		public GunType GunKind;

		private Text _powerupText;
		private Player _playerScript;

		private void Start()
		{
			_powerupText = gameObject.GetComponent<Text>();
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
				_powerupText.text = _playerScript.GetGunAmmo(GunKind).ToString();
			}
		}
	}
}
