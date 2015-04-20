using UnityEngine;
using System.Collections;

public class loadLevelScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LoadMenuScene()
    {
        Application.LoadLevel(0);
    }

    public void LoadGameScene()
    {
        Application.LoadLevel(1);
    }
}
