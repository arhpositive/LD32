using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class endScoreRefreshText : MonoBehaviour {
    Text highScoreInfo_;
    playerScript scriptPlayer_;

    // Use this for initialization
    void Start()
    {
        highScoreInfo_ = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        highScoreInfo_.text = "Your Score: " + playerScript.playerScore_.ToString();
    }

    public void SetTextVisible()
    {
        highScoreInfo_.enabled = true;
    }
}
