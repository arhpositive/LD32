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
	public bool RespawnOnDestroy;
	public float[] VerticalLimits;
	public float[] HorizontalLimits;

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

		if (DestroyOnHorizontalLimits)
		{
			if (transform.position.x < HorizontalLimits[0] || transform.position.x > HorizontalLimits[1])
			{
				OnDestroyTrigger();
			}
		}

		if (DestroyOnVerticalLimits)
		{
			if (transform.position.y < VerticalLimits[0] || transform.position.y > VerticalLimits[1])
			{
				OnDestroyTrigger();
			}
		}
	}

	public void SetMoveDir(Vector2 newMoveDir, bool keepRandomizations = false)
	{
		_moveDir = newMoveDir;

		if (keepRandomizations)
		{
			RandomlyAlterMoveDirection();
		}
	}

	private void Initialize()
	{
		_moveDir = new Vector2(MoveDirX, MoveDirY);
		SpeedCoef = 1.0f;

		RandomlyAlterMoveDirection();

		if (RandomizeRotationSpeed)
		{
			float range = Random.Range(0.0f, 1.0f) * RandomizeRotationSpeedCoef;
			RotationSpeed = Random.Range(-range, range);
		}
	}

	private void OnDestroyTrigger()
	{
		if (RespawnOnDestroy)
		{
			Vector2 respawnPos = new Vector2(Random.Range(HorizontalLimits[1] - 0.1f, HorizontalLimits[1]),
				Random.Range(VerticalLimits[0], VerticalLimits[1]));
			transform.position = respawnPos;
			Initialize();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void RandomlyAlterMoveDirection()
	{
		if (RandomizeMoveDirX)
		{
			float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirXCoef;
			_moveDir.x += Random.Range(-range, range);
		}

		if (RandomizeMoveDirY)
		{
			float range = Random.Range(0.0f, 1.0f) * RandomizeMoveDirYCoef;
			_moveDir.y += Random.Range(-range, range);
		}

		_moveDir.Normalize();
	}
}