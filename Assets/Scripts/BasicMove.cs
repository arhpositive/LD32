/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicMove.cs
 * Should be attached to every moving, uncontrollable entity
 * Contains speed and direction
 */

using UnityEngine;

public class BasicMove : MonoBehaviour
{
    public bool DoesMove;
    public float MoveSpeed;
    public float SpeedCoef { get; set; }
    private Vector2 _moveDir;

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
    
    public bool DestroyOnVerticalLimits;
    public bool DestroyOnHorizontalLimits;
    public bool DestroyOnEarlyHorizontalLimits;

    public bool RespawnOnVerticalLimits;
    public bool RespawnOnHorizontalLimits;

    private void Awake()
    {
        Initialize();
    }
    
    private void Update()
    {
        if (DoesMove)
        {
            transform.Translate(_moveDir * MoveSpeed * SpeedCoef * Time.deltaTime, Space.World);
        }

        if (DoesRotate)
        {
            transform.Rotate(Vector3.forward * RotationSpeed * Time.deltaTime, Space.World);
        }

        if (DestroyOnEarlyHorizontalLimits)
        {
            if (transform.position.x < GameConstants.HorizontalMinCoord || 
                transform.position.x > GameConstants.HorizontalEarlyMaxCoord)
            {
                Destroy(gameObject);
            }
        }
        else if (DestroyOnHorizontalLimits)
        {
            if (transform.position.x < GameConstants.HorizontalMinCoord || 
                transform.position.x > GameConstants.HorizontalMaxCoord)
            {
                Destroy(gameObject);
            }
        }

        if (DestroyOnVerticalLimits)
        {
            if (transform.position.y < GameConstants.VerticalMinCoord || 
                transform.position.y > GameConstants.VerticalMaxCoord)
            {
                Destroy(gameObject);
            }
        }

        if (RespawnOnHorizontalLimits)
        {
            if (transform.position.x < GameConstants.HorizontalMinCoord ||
                transform.position.x > GameConstants.HorizontalMaxCoord)
            {
                transform.position = SpawnManager.GetRespawnPos();
                Initialize();
            }
        }
        else if (RespawnOnVerticalLimits)
        {
            if (transform.position.y < GameConstants.VerticalMinCoord ||
                transform.position.y > GameConstants.VerticalMaxCoord)
            {
                transform.position = SpawnManager.GetRespawnPos();
                Initialize();
            }
        }
    }

    public void SetMoveDir(Vector2 newMoveDir)
    {
        _moveDir = newMoveDir;
    }

    private void Initialize()
    {
        _moveDir = new Vector2(MoveDirX, MoveDirY);
        SpeedCoef = 1.0f;

        if (RandomizeMoveDirX)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirXCoef;
            _moveDir.x = Random.Range(-range, range);
        }

        if (RandomizeMoveDirY)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirYCoef;
            _moveDir.y = Random.Range(-range, range);
        }

        _moveDir.Normalize();

        if (RandomizeRotationSpeed)
        {
            float range = Random.Range(0.0f, 1.0f) * RandomizeRotationSpeedCoef;
            RotationSpeed = Random.Range(-range, range);
        }
    }
}