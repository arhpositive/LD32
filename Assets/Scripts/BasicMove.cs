/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicMove.cs
 * Should be attached to every moving, uncontrollable entity
 * Contains speed and direction
 */

using UnityEngine;
using System.Collections;

public class BasicMove : MonoBehaviour 
{
    public bool DoesMove;
    public float MoveSpeed;
    Vector2 MoveDir;

    [Range(-1.0f, 1.0f)]
    public float MoveDirX;
    public bool RandomizeMoveDirX;
    [Range(0.0f, 1.0f)]
    public float RandomizeMoveDirXCoef;
    [Range(-1.0f, 1.0f)]
    public float MoveDirY;
    public bool RandomizeMoveDirY;
    [Range(0.0f, 1.0f)]
    public float RandomizeMoveDirYCoef;

    public bool DoesRotate;
    public float RotationSpeed;
    public bool RandomizeRotationSpeed;
    public float RandomizeRotationSpeedCoef;

    public bool BounceOnHorizontalLimits;
    public bool DestroyOnVerticalLimits;
    public bool DestroyOnHorizontalLimits;

	// Use this for initialization
	void Start () 
    {
        MoveDir = new Vector2(MoveDirX, MoveDirY);

        if (RandomizeMoveDirX)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirXCoef;
            MoveDir.x = Random.Range(-range, range);
        }

        if (RandomizeMoveDirY)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirYCoef;
            MoveDir.y = Random.Range(-range, range);
        }

        MoveDir.Normalize();
	
        if (RandomizeRotationSpeed)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeRotationSpeedCoef;
            RotationSpeed = Random.Range(-range, range);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (DoesMove)
        {
            transform.Translate(MoveDir * MoveSpeed * Time.deltaTime, Space.World);
        }

        if (DoesRotate)
        {
            transform.Rotate(Vector3.forward * RotationSpeed * Time.deltaTime, Space.World);
        }

        if (DestroyOnHorizontalLimits)
        {
            if (transform.position.x < GameConstants.HorizontalMinCoord || transform.position.x > GameConstants.HorizontalMaxCoord)
            {
                Destroy(gameObject);
            }
        }

        if (DestroyOnVerticalLimits)
        {
            if (transform.position.y < GameConstants.VerticalMinCoord || transform.position.y > GameConstants.VerticalMaxCoord)
            {
                Destroy(gameObject);
            }
        }

        if (BounceOnHorizontalLimits)
        {
            if (transform.position.y < GameConstants.MinVerticalMovementLimit && MoveDir.y < -float.Epsilon ||
                transform.position.y > GameConstants.MaxVerticalMovementLimit && MoveDir.y > float.Epsilon)
            {
                MoveDir.y = -MoveDir.y;
                MoveDir.Normalize();
            }
        }
	}
}
