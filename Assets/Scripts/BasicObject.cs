/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicObject.cs
 * Handles the common features of the interactive objects in the game
 * These objects are ships, bullets and powerups
 */

using UnityEngine;
using UnityEngine.Assertions;

public class BasicObject : MonoBehaviour
{
    public GameObject DestructionPrefab;
    public AudioClip DestructionAudioClip;

    public void OnDestruction()
    {
        //animate explosion and emit sound
        if (DestructionPrefab != null)
        {
            GameObject explosion = Instantiate(DestructionPrefab, transform.position, Quaternion.identity) as GameObject;
            Assert.IsNotNull(explosion);
            explosion.GetComponent<SpriteRenderer>().material.color = gameObject.GetComponent<SpriteRenderer>().color;
        }
        if (DestructionAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(DestructionAudioClip, transform.position);
        }
    }
}