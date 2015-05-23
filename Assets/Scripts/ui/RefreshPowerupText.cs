/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * RefreshPowerupText.cs
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RefreshPowerupText : MonoBehaviour
{
    Text PowerupText;
    Player PlayerScript;

    void Start()
    {
        PowerupText = gameObject.GetComponent<Text>();
        PlayerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        PowerupText.text = PlayerScript.GetSpeedUpGunAmmo().ToString();
    }
}
