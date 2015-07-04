/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * MusicManager.cs
 * Handles music in battle scene.
 */

using UnityEngine;

namespace Assets.Scripts
{
    public class MusicManager : MonoBehaviour
    {
        public AudioClip[] MusicList;
        AudioSource _currentAudioSource;

        void Awake()
        {
            int randomIndex = Random.Range(0, MusicList.Length);

            _currentAudioSource = gameObject.GetComponent<AudioSource>();

            _currentAudioSource.clip = MusicList[randomIndex];
            _currentAudioSource.loop = true;
            _currentAudioSource.Play();
        }
    }
}
