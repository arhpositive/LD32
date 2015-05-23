/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshEndScoreText.cs
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RefreshEndScoreText : MonoBehaviour 
{
    Text EndScoreText;

    void Start()
    {
        EndScoreText = gameObject.GetComponent<Text>();
    }

    void Update()
    {
        EndScoreText.text = "Your Score: " + Player.PlayerScore.ToString();
    }

    public void SetTextVisible()
    {
        EndScoreText.enabled = true;
    }
}
