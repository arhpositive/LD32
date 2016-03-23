/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * Destroyable.cs
 * Handles objects that will be destroyed upon animation finish such as an explosion object.
 */

using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public void DestroyMe() { Destroy(gameObject); }
}