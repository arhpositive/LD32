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
        Text _powerupText;
        Player _playerScript;

        void Start()
        {
            _powerupText = gameObject.GetComponent<Text>();
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            _powerupText.text = _playerScript.GetSpeedUpGunAmmo().ToString();
        }
    }
}
