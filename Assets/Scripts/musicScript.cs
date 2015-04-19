/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * musicScript.cs
 * Handles music in battle scene.
 */

using UnityEngine;
using System.Collections;

public class musicScript : MonoBehaviour {

    public AudioClip[] musicList_;
    AudioSource audioSource_;

	void Awake () {
        int randomIndex = Random.Range(0, musicList_.Length);

        audioSource_ = gameObject.GetComponent<AudioSource>();

        audioSource_.clip = musicList_[randomIndex];
        audioSource_.loop = true;
        audioSource_.Play();
	}
}
