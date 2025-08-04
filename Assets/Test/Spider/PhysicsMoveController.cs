using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NN
{
    /// <summary>
    /// Контроллер движения персонажа с использованием физики.
    /// Управляет перемещением, рывком и взаимодействием с поверхностями.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsMoveController : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _moveAction;

        [SerializeField]
        private InputActionReference _dashAction;

        [SerializeField]
        private InputActionReference _mousePositionAction;

        [SerializeField]
        private MoveSettings _moveSettings = new()
        {
            SpeedLimit = 5f,
            Acceleration = 10f,
            Deceleration = 20f,
            UphillMultiplier = 2f
        };

        [SerializeField]
        private DashSettings _dashSettings = new()
        {
            DashDuration = 0.5f,
            DashForce = 20f
        };

        [SerializeField]
        private GroundSettings _groundSettings = new()
        {
            CastHeight = 1f,
            CastDistance = 2f,
            TestSphereRadius = 0.3f,
            GroundLayer = ~0,
            StickGroundForce = 10f
        };

        [Serializable]
        private struct MoveSettings
        {
            /// <summary>Максимальная скорость движения персонажа.</summary>
            public float SpeedLimit;

            /// <summary>Ускорение персонажа при движении.</summary>
            public float Acceleration;

            /// <summary>Замедление персонажа при остановке.</summary>
            public float Deceleration;

            /// <summary>Множитель скорости при движении в гору.</summary>
            public float UphillMultiplier;
        }

        [Serializable]
        private struct DashSettings
        {
            /// <summary>Продолжительность рывка в секундах.</summary>
            public float DashDuration;

            /// <summary>Сила рывка.</summary>
            public float DashForce;
        }

        [Serializable]
        private struct GroundSettings
        {
            /// <summary>Высота, с которой начинается проверка поверхности под персонажем.</summary>
            public float CastHeight;

            /// <summary>Максимальная дистанция проверки поверхности под персонажем.</summary>
            public float CastDistance;

            /// <summary>Радиус сферы для проверки поверхности.</summary>
            public float TestSphereRadius;

            /// <summary>Слой, который считается землей.</summary>
            public LayerMask GroundLayer;

            public float StickGroundForce;
        }

        private enum CharacterStatus
        {
            Move,
            Dash
        }

        private Rigidbody _rigidbody;
        private Vector3 _inputDirection;
        private bool _isGrounded;
        private Vector3 _groundNormal;
        private float _groundDistance;
        private CharacterStatus _currentStatus;
        private float _dashActionTimer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _moveAction.action.Enable();
            _dashAction.action.Enable();
            _mousePositionAction.action.Enable();
        }

        private void Update()
        {
            if (_currentStatus == CharacterStatus.Move)
            {
                UpdateInput();
            }

            if (_dashAction.action.triggered && _currentStatus != CharacterStatus.Dash)
            {
                StartDash();
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundState();

            if (_currentStatus == CharacterStatus.Dash)
            {
                UpdateDash();
            }
            else if (_currentStatus == CharacterStatus.Move)
            {
                UpdateMovement();
                UpdateRotation();
            }
        }

        /// <summary>
        /// Обновляет пользовательский ввод для движения.
        /// </summary>
        private void UpdateInput()
        {
            Vector2 moveInput = _moveAction.action.ReadValue<Vector2>();
            _inputDirection = AdjustInputToCamera(Vector3.up, moveInput).normalized;
        }

        /// <summary>
        /// Обновляет движение персонажа, учитывая наклон поверхности и пользовательский ввод.
        /// </summary>
        private void UpdateMovement()
        {
            if (_isGrounded)
            {
                if (_inputDirection.sqrMagnitude > 0)
                {
                    Vector3 targetVelocity = _inputDirection * _moveSettings.SpeedLimit;
                    Vector3 velocityChange = targetVelocity - _rigidbody.linearVelocity;

                    // Учет угла поверхности
                    float slopeFactor = Mathf.Clamp01(Vector3.Dot(_groundNormal, Vector3.up));
                    velocityChange = Vector3.ProjectOnPlane(velocityChange, _groundNormal);
                    velocityChange *= Mathf.Lerp(1f, _moveSettings.UphillMultiplier, 1f - slopeFactor);

                    velocityChange = Vector3.ClampMagnitude(velocityChange, _moveSettings.Acceleration * Time.fixedDeltaTime);
                    _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                }
                else
                {
                    Vector3 velocity = _rigidbody.linearVelocity;
                    velocity = Vector3.Lerp(velocity, Vector3.zero, _moveSettings.Deceleration * Time.fixedDeltaTime);
                    _rigidbody.linearVelocity = velocity;
                }

                // Прилипание к земле
                _rigidbody.AddForce(Vector3.down * _groundSettings.StickGroundForce, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Обновляет состояние рывка, уменьшая таймер и применяя силу рывка.
        /// </summary>
        private void UpdateDash()
        {
            _dashActionTimer -= Time.fixedDeltaTime;
            if (_dashActionTimer <= 0)
            {
                EndDash();
                return;
            }

            _rigidbody.linearVelocity = _inputDirection * _dashSettings.DashForce;
        }

        /// <summary>
        /// Начинает рывок, устанавливая статус и таймер.
        /// </summary>
        private void StartDash()
        {
            _currentStatus = CharacterStatus.Dash;
            _dashActionTimer = _dashSettings.DashDuration;
            _rigidbody.isKinematic = true;
        }

        /// <summary>
        /// Завершает рывок, возвращая персонажа в состояние движения.
        /// </summary>
        private void EndDash()
        {
            _currentStatus = CharacterStatus.Move;
            _rigidbody.isKinematic = false;
        }

        /// <summary>
        /// Обновляет состояние земли под персонажем, проверяя наличие поверхности.
        /// </summary>
        private void UpdateGroundState()
        {
            Vector3 origin = transform.position + Vector3.up * _groundSettings.CastHeight;
            _isGrounded = Physics.SphereCast(
                origin,
                _groundSettings.TestSphereRadius,
                Vector3.down,
                out RaycastHit hit,
                _groundSettings.CastDistance,
                _groundSettings.GroundLayer
            );

            if (_isGrounded)
            {
                _groundNormal = hit.normal;
                _groundDistance = hit.distance;
            }
            else
            {
                _groundNormal = Vector3.up;
                _groundDistance = 0f;
            }
        }

        private void UpdateRotation()
        {
            transform.rotation = GetRotation(transform.position, Vector3.up);
        }

        private Quaternion GetRotation(Vector3 from, Vector3 up)
        {
            Vector2 mousePosition = _mousePositionAction.action.ReadValue<Vector2>();
            Ray castRay = Camera.main.ScreenPointToRay(mousePosition);
            var hits = Physics.RaycastAll(castRay, 1000f, _groundSettings.GroundLayer).OrderBy(x => x.distance);
            foreach (var hit in hits)
            {
                return Quaternion.LookRotation((hit.point - from).ProjectOntoPlane(up).normalized, up);
            }
            if (new Plane(up, transform.position).Raycast(castRay, out float enter))
            {
                return Quaternion.LookRotation((castRay.GetPoint(enter) - from).ProjectOntoPlane(up).normalized, up);
            }
            return transform.rotation;
        }

        /// <summary>
        /// Преобразует ввод в зависимости от положения камеры.
        /// </summary>
        /// <param name="up">Направление вверх в мировых координатах.</param>
        /// <param name="input">Ввод пользователя в виде вектора.</param>
        /// <returns>Скорректированный вектор ввода.</returns>
        private Vector3 AdjustInputToCamera(Vector3 up, Vector2 input)
        {
            Vector3 forward = Camera.main.transform.forward;
            if (Vector3.Cross(up, forward).sqrMagnitude < 0.01f)
            {
                forward = Camera.main.transform.up;
            }
            forward = forward.ProjectOntoPlane(up).normalized;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            return forward * input.y + right * input.x;
        }
    }
}
