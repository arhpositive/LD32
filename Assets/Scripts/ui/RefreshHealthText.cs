/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshHealthText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
    public class RefreshHealthText : MonoBehaviour
    {
        Text _healthText;
        Player _playerScript;

        void Start()
        {
            _healthText = gameObject.GetComponent<Text>();
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            _healthText.text = _playerScript.PlayerHealth.ToString();
        }
    }
}
