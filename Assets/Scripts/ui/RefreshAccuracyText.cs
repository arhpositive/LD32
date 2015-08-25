/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshAccuracyText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
    public class RefreshAccuracyText : MonoBehaviour
    {
        Text _accuracyText;
        Player _playerScript;

        void Start()
        {
            _accuracyText = gameObject.GetComponent<Text>();
            _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            _accuracyText.text = _playerScript.PlayerAccuracy.ToString();
        }
    }
}


