/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * LoadLevel.cs
 * Switches between scenes
 */

using UnityEngine;

namespace Assets.Scripts
{
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
}
