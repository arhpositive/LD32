using UnityEngine;
using UnityEngine.UI;

namespace ui
{
	public class PowerupCooldownBar : MonoBehaviour
	{
		public GunType GunKind;

		private Slider _powerupCooldownSlider;
		private Player _playerScript;

		private void Start()
		{
			_powerupCooldownSlider = gameObject.GetComponent<Slider>();
			GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
			if (playerGameObject)
			{
				_playerScript = playerGameObject.GetComponent<Player>();
			}
		}

		// Update is called once per frame
		void Update()
		{
			_powerupCooldownSlider.value = _playerScript ? _playerScript.GetGunCooldownPercentage(GunKind) : 0.0f;
		}
	}
}
