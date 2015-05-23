/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshHealthText.cs
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RefreshHealthText : MonoBehaviour 
{
    Text HealthText;
    Player PlayerScript;

	void Start () 
    {
        HealthText = gameObject.GetComponent<Text>();
        PlayerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}
	
	void Update () 
    {
        HealthText.text = PlayerScript.GetPlayerHealth().ToString();
	}
}
