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
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void Update()
        {
            _healthText.text = _playerScript.PlayerHealth.ToString();
        }
    }
}
