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
        Text _highScoreText;

        void Start()
        {
            _highScoreText = gameObject.GetComponent<Text>();
        }

        void Update()
        {
            _highScoreText.text = Player.PlayerScore.ToString();
        }
    }
}
