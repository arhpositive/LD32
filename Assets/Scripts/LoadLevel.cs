/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * LoadLevel.cs
 * Switches between scenes
 */

using UnityEngine;
using System.Collections;

public class LoadLevel : MonoBehaviour 
{
    public void LoadMenuScene()
    {
        Application.LoadLevel(0);
    }

    public void LoadGameScene()
    {
        Application.LoadLevel(1);
    }
}
