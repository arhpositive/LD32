/* 
 * Ludum Dare 32 Game
 * Author: Arhan Bakan
 * 
 * basicEnemyScript.cs
 * Handles basic enemy behaviour
 */

using UnityEngine;
using System.Collections;

public class basicEnemyScript : MonoBehaviour 
{
    public float speed_;
    public Vector3 direction_;

    public static float enemyHorizontalExitCoord_ = -1.0f;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Translate(direction_ * speed_ * Time.deltaTime);

        if (transform.position.x < enemyHorizontalExitCoord_)
        {
            Destroy(gameObject);
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}
