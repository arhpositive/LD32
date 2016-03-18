/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshEndScoreText.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ui
{
    public class RefreshEndScoreText : MonoBehaviour
    {
        private Text _endScoreText;

        private void Start()
        {
            _endScoreText = gameObject.GetComponent<Text>();
        }

        private void Update()
        {
            _endScoreText.text = "Your Score: " + Player.PlayerScore;
        }

        public void SetTextVisible()
        {
            _endScoreText.enabled = true;
        }
    }
}
