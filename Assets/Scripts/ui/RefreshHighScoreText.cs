/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshHighScoreText.cs
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RefreshHighScoreText : MonoBehaviour 
{
    Text HighScoreText;

    void Start()
    {
        HighScoreText = gameObject.GetComponent<Text>();
    }

    void Update()
    {
        HighScoreText.text = Player.PlayerScore.ToString();
    }
}
