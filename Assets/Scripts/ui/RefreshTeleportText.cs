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
    public class RefreshTeleportText : MonoBehaviour
    {
        Text _teleportText;
        Player _playerScript;

        void Start()
        {
            _teleportText = gameObject.GetComponent<Text>();
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            _teleportText.text = _playerScript.GetTeleportGunAmmo().ToString();
        }
    }
}
