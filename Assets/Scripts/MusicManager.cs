/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * MusicManager.cs
 * Handles music in battle scene.
 */

using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
    public AudioClip[] MusicList;
    AudioSource CurrentAudioSource;

	void Awake () {
        int randomIndex = Random.Range(0, MusicList.Length);

        CurrentAudioSource = gameObject.GetComponent<AudioSource>();

        CurrentAudioSource.clip = MusicList[randomIndex];
        CurrentAudioSource.loop = true;
        CurrentAudioSource.Play();
	}
}
