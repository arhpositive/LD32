/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshHighScoreText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
    public class RefreshHighScoreText : MonoBehaviour
    {
        private Text _highScoreText;

        private void Start()
        {
            _highScoreText = gameObject.GetComponent<Text>();
        }

        private void Update()
        {
            _highScoreText.text = Player.PlayerScore.ToString();
        }
    }
}
