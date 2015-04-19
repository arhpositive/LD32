/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * powerupRefreshScript.cs
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class powerupRefreshScript : MonoBehaviour
{

    Text powerupInfo_;
    playerScript scriptPlayer_;

    // Use this for initialization
    void Start()
    {
        powerupInfo_ = gameObject.GetComponent<Text>();
        scriptPlayer_ = GameObject.FindGameObjectWithTag("Player").GetComponent<playerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        powerupInfo_.text = scriptPlayer_.getSpeedUpGunAmmo().ToString();
    }
}
