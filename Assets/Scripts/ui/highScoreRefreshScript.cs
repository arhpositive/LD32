using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class highScoreRefreshScript : MonoBehaviour {

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
        highScoreInfo_.text = playerScript.playerScore_.ToString();
    }
}
