/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * LoadLevel.cs
 * Switches between scenes
 */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LoadLevel : MonoBehaviour
    {
        public void LoadMenuScene()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadGameScene()
        {
            SceneManager.LoadScene(1);
        }
    }
}
