/* 
 * Game: Dislocator
 * Author: Arhan Bakan
 * 
 * BasicMove.cs
 * Should be attached to every moving, uncontrollable entity
 * Contains speed and direction
 */

using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts
{
    public class BasicMove : MonoBehaviour
    {
        public bool DoesMove;
        public float MoveSpeed;
        public float SpeedCoef { get; set; }
        Vector2 _moveDir;

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
        public bool DestroyOnEarlyHorizontalLimits;

        float _minVerticalBounceLimit;
        float _maxVerticalBounceLimit;

        void Awake()
        {
            _minVerticalBounceLimit = GameConstants.MinVerticalMovementLimit;
            _maxVerticalBounceLimit = GameConstants.MaxVerticalMovementLimit;

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

        void Update()
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
                if (transform.position.x < GameConstants.HorizontalMinCoord || transform.position.x > GameConstants.HorizontalEarlyMaxCoord)
                {
                    Destroy(gameObject);
                }
            }
            else if (DestroyOnHorizontalLimits)
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
                if (transform.position.y < _minVerticalBounceLimit && _moveDir.y < -float.Epsilon ||
                    transform.position.y > _maxVerticalBounceLimit && _moveDir.y > float.Epsilon)
                {
                    _moveDir.y = -_moveDir.y;
                    _moveDir.Normalize();
                }
            }
        }

        public void SetMoveDir(Vector2 newMoveDir)
        {
            _moveDir = newMoveDir;
        }

        public void SetBounceLimits(float minLimit, float maxLimit)
        {
            Assert.IsTrue(minLimit >= GameConstants.MinVerticalMovementLimit);
            Assert.IsTrue(maxLimit <= GameConstants.MaxVerticalMovementLimit);

            _minVerticalBounceLimit = minLimit;
            _maxVerticalBounceLimit = maxLimit;
        }
    }
}
