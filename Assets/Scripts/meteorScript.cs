using UnityEngine;
using System.Collections;

public class meteorScript : MonoBehaviour {

    public float speed_;
    Vector2 direction_;
    float rotation_;

	// Use this for initialization
	void Start () {
        direction_ = new Vector2(-1.0f, 0.0f);
        direction_.Normalize();
        rotation_ = Random.Range(-1.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(direction_ * speed_ * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward, rotation_, Space.World);

        if (transform.position.x < spawnScript.horizontalExitCoord_ || transform.position.x > spawnScript.horizontalEnterCoord_ ||
            transform.position.y > playerScript.maxVerticalMovementLimit_ + 1.0f || transform.position.y < playerScript.minVerticalMovementLimit_ - 1.0f)
        {
            Destroy(gameObject);
        }
	}

    public void setDirection(Vector2 direction)
    {
        direction_ = direction;
        direction_.Normalize();
    }
}
