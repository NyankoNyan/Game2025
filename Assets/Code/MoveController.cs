using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace NN
{
    public class MoveController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _moveAction;

        [SerializeField]
        private InputActionReference _mousePositionAction;

        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private MoveSettings _moveSettings = new()
        {
            speedLimit = 5f,
            acceleration = 10f,
            deacceleration = 20f,
        };

        [SerializeField]
        private GroundSettings _groundSettings = new()
        {
            castHeight = 1f,
            castDistance = 2f,
            testSphereRadius = .3f
        };

        [Serializable]
        private struct MoveSettings
        {
            public float speedLimit;
            public float acceleration;
            public float deacceleration;
        }

        [Serializable]
        public struct GroundSettings
        {
            public float castHeight;
            public float castDistance;
            public float testSphereRadius;
            public LayerMask groundLayer;
        }

        public bool MoveActive { get; set; }

        private class MoveModel
        {
            public Vector3 Speed;
        }

        private MoveModel _moveModel = new();
        private bool _initialized;

        public void Init()
        {
            _initialized = true;
            _moveAction.action.Enable();
            _mousePositionAction.action.Enable();
            MoveActive = true;
        }

        private void Start()
        {
            Assert.IsNotNull( _moveAction );
            Assert.IsNotNull( _mousePositionAction );

            Assert.IsNotNull( _characterController );
        }

        private void Update()
        {
            if (_initialized)
            {
                if(MoveActive)
                    UpdateMove( Time.deltaTime );
            }
        }

        private void UpdateMove(float deltaTime)
        {
            Vector2 moveInput = _moveAction.action.ReadValue<Vector2>();
            float magnitude = moveInput.magnitude;
            Vector3 moveAdjusted = AdjustInputToCamera( Vector3.up, moveInput );

            bool hasMove = magnitude > 0;

            if (hasMove)
            {
                _moveModel.Speed = moveAdjusted.normalized
                    * Mathf.Min( _moveSettings.speedLimit, _moveModel.Speed.magnitude + deltaTime * _moveSettings.acceleration );
            } else
            {
                _moveModel.Speed = _moveModel.Speed.normalized * Mathf.Max( 0, _moveModel.Speed.magnitude - _moveSettings.deacceleration * deltaTime );
            }

            Vector3 flatDelta = _moveModel.Speed * deltaTime;

            Vector3 groundDelta = TestGround( flatDelta + transform.position, Vector3.up ) * -Vector3.up;

            transform.rotation = GetRotation( flatDelta + transform.position, Vector3.up );

            _characterController.Move( flatDelta + groundDelta );
        }

        private Vector3 AdjustInputToCamera(Vector3 up, Vector2 input)
        {
            Vector3 forward = Camera.main.transform.forward;
            if (Vector3.Cross( up, forward ).sqrMagnitude < .01f)
            {
                forward = Camera.main.transform.up;
            }
            Vector3 right = Vector3.Cross( up, forward ).normalized;
            return forward * input.y + right * input.x;
        }

        private float TestGround(Vector3 from, Vector3 up)
        {
            var hits = Physics.SphereCastAll( from, _groundSettings.testSphereRadius, -up, _groundSettings.castDistance, _groundSettings.groundLayer );
            foreach (var hit in hits.OrderBy( x => x.distance ))
            {
                return hit.distance - _groundSettings.castHeight + _groundSettings.testSphereRadius;
            }
            return 0;
        }

        private Quaternion GetRotation(Vector3 from, Vector3 up)
        {
            Vector2 mousePosition = _mousePositionAction.action.ReadValue<Vector2>();
            Ray castRay = Camera.main.ScreenPointToRay( mousePosition );
            var hits = Physics.RaycastAll( castRay, 1000f, _groundSettings.groundLayer ).OrderBy( x => x.distance );
            foreach (var hit in hits)
            {
                return Quaternion.LookRotation( (hit.point - from).ProjectOntoPlane( up ).normalized, up );
            }
            if (new Plane( up, transform.position ).Raycast( castRay, out float enter ))
            {
                return Quaternion.LookRotation( (castRay.GetPoint( enter ) - from).ProjectOntoPlane( up ).normalized, up );
            }
            return transform.rotation;
        }
    }
}