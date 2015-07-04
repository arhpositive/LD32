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
        Text _endScoreText;

        void Start()
        {
            _endScoreText = gameObject.GetComponent<Text>();
        }

        void Update()
        {
            _endScoreText.text = "Your Score: " + Player.PlayerScore.ToString();
        }

        public void SetTextVisible()
        {
            _endScoreText.enabled = true;
        }
    }
}
