using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class healthRefreshScript : MonoBehaviour {

    Text healthInfo_;
    playerScript scriptPlayer_;

	// Use this for initialization
	void Start () {
        healthInfo_ = gameObject.GetComponent<Text>();
        scriptPlayer_ = GameObject.FindGameObjectWithTag("Player").GetComponent<playerScript>();
	}
	
	// Update is called once per frame
	void Update () {
        healthInfo_.text = scriptPlayer_.getHealth().ToString();
	}
}
