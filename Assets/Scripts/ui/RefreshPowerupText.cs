/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshPowerupText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
    public class RefreshPowerupText : MonoBehaviour
    {
        private Text _powerupText;
        private Player _playerScript;

        private void Start()
        {
            _powerupText = gameObject.GetComponent<Text>();
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void Update()
        {
            _powerupText.text = _playerScript.GetSpeedUpGunAmmo().ToString();
        }
    }
}
