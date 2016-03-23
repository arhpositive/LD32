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
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void Update()
        {
            _teleportText.text = _playerScript.GetTeleportGunAmmo().ToString();
        }
    }
}
