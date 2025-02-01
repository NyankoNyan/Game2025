using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace NN
{
    /// <summary>
    /// ���������� ��������, ����������� ��������� �� ��������� ����� ������.
    /// </summary>
    public class ShootController : MonoBehaviour
    {
        [Tooltip( "���������� ������ ��� ������/�������." )]
        [SerializeField]
        private int _skillSlotsCount = 2;

        [Tooltip( "�����, �� ������� ���������� ��������." )]
        [SerializeField]
        private Transform _shootingPoint;

        [Tooltip( "����, �� ������� ����� �������� �������." )]
        [SerializeField]
        private LayerMask _bulletsHitLayers = ~0;

        [Tooltip( "����� ���� �����. ����� ��� ��������� �� ����� ���������� �� ����� �������� �����." )]
        [SerializeField]
        private LayerMask _groundLayer;

        [Tooltip( "������� ������ ������� � ������� ����� ��������." )]
        [SerializeField]
        private float _bulletTargetHeight = 1.5f;

        [Tooltip( "������������ ��������� �������� �����." )]
        [SerializeField]
        private float _groundTestDistance = 50f;

        [Header( "Controls" )]
        [Tooltip( "������ InputSystem ��� �������." )]
        [SerializeField]
        private InputActionReference[] _skillActions;

        [Tooltip( "���� InputSystem ��� ������� ����." )]
        [SerializeField]
        private InputActionReference _mousePositionAction;

        [Header( "Initialize" )]
        [Tooltip( "�������������� ������� �� ��������� �� ������." )]
        [SerializeField]
        private string[] _defaultIds = new[] { "RIFLE" };

        private WeaponPreset[] _currentPresets;
        private bool[] _performedSkills;
        private bool _initialized;

        /// <summary>
        /// ������������� ����������� ��������.
        /// </summary>
        public void Init()
        {
            _initialized = true;

            for (int i = 0; i < _skillSlotsCount; i++)
            {
                var skillAction = _skillActions[i];
                skillAction.action.Enable();
                int locI = i;
                skillAction.action.performed += (_) => { _performedSkills[locI] = true; };
            }

            _currentPresets = new WeaponPreset[_skillSlotsCount];
            _performedSkills = new bool[_skillSlotsCount];

            InitDefaults();
        }

        /// <summary>
        /// ��������� �������������
        /// </summary>
        private void Start()
        {
            ValidateFields();

            if (!_shootingPoint)
            {
                _shootingPoint = transform;
                Debug.LogWarning( "Missing shooting point" );
            }

            for (int i = 0; i < _skillSlotsCount; i++)
            {
                var skillAction = _skillActions[i];
                Assert.IsNotNull( skillAction );
            }
        }

        /// <summary>
        /// �������� ����� �� ������������.
        /// </summary>
        private void ValidateFields()
        {
            Assert.IsNotNull( _mousePositionAction, "Mouse position action is not assigned." );
            Assert.AreEqual( _skillSlotsCount, _skillActions.Length, "Skill actions count does not match skill slots count." );
            Assert.IsTrue( _skillSlotsCount >= _defaultIds.Length, "Skill slots count is less than default IDs count." );
        }

        /// <summary>
        /// ������������� �������� ������� �� ���������.
        /// </summary>
        private void InitDefaults()
        {
            for (int i = 0; i < _defaultIds.Length; i++)
            {
                string defaultId = _defaultIds[i];
                if (!string.IsNullOrWhiteSpace( defaultId ))
                {
                    _currentPresets[i] = SkillsAndWeapons.Instance.GetPreset( defaultId );
                }
            }
        }

        /// <summary>
        /// ���������� ��������� �����������.
        /// </summary>
        private void Update()
        {
            if (_initialized)
            {
                UpdateShooting();
            }
        }

        /// <summary>
        /// ���������� ��������� ��������.
        /// </summary>
        private void UpdateShooting()
        {
            for (int i = 0; i < _performedSkills.Length; i++)
            {
                if (!_performedSkills[i])
                    continue;

                _performedSkills[i] = false;

                var weaponPreset = _currentPresets[i];

                if (weaponPreset == null)
                    continue;

                HandleWeaponShooting( weaponPreset );
            }
        }

        /// <summary>
        /// ��������� �������� � ����������� �� ���� ������.
        /// </summary>
        /// <param name="weaponPreset">������ ������.</param>
        private void HandleWeaponShooting(WeaponPreset weaponPreset)
        {
            switch (weaponPreset.weaponType)
            {
                case WeaponPreset.WeaponType.Ray:
                    ShootRayWeapon( weaponPreset );
                    break;

                default:
                    throw new System.Exception( $"Unknown weapon type {weaponPreset.weaponType}" );
            }
        }

        /// <summary>
        /// �������� �� ������ ���� Ray.
        /// </summary>
        /// <param name="weaponPreset">������ ������.</param>
        private void ShootRayWeapon(WeaponPreset weaponPreset)
        {
            Vector3 target = GetShootingTarget( Vector3.up );
            var shootResult = GetShootingHit( target, weaponPreset.maxDistance );

            ShootRaySpawner.Instance.SpawnRay( _shootingPoint.position, shootResult.hitPoint, weaponPreset.id );

            NetworkObject networkObject = null;
            if (shootResult.success)
            {
                networkObject = shootResult.hitTarget.GetComponentInParent<NetworkObject>();
            }

            NetworkActionsReceiver.Instance.ShootRay( new ShootRayInfo()
            {
                weaponPresetId = weaponPreset.id,
                localFrom = _shootingPoint.position,
                localTo = shootResult.hitPoint,
                hitTargetNetId = networkObject?.NetworkObjectId ?? 0,
                successHit = networkObject
            } );
        }

        

        /// <summary>
        /// ��������� ������� ������� ��� �������� � ����������� �� ��������� ���������.
        /// </summary>
        /// <param name="up">����������� �����.</param>
        /// <returns>���������� ����.</returns>
        private Vector3 GetShootingTarget(Vector3 up)
        {
            Vector2 mousePosition = _mousePositionAction.action.ReadValue<Vector2>();
            Ray castRay = Camera.main.ScreenPointToRay( mousePosition );
            var hits = Physics.RaycastAll( castRay, 1000f, _bulletsHitLayers );
            Vector3 hitTarget;
            if (hits.Length > 0)
            {
                hitTarget = hits.OrderBy( h => h.distance ).First().point;
            } else
            {
                if (new Plane( up, transform.position ).Raycast( castRay, out float enter ))
                {
                    hitTarget = castRay.GetPoint( enter );
                } else
                {
                    hitTarget = _shootingPoint.transform.position + _shootingPoint.forward;
                }
            }

            var groundHits = Physics.RaycastAll( hitTarget + up, -up, _groundTestDistance, _groundLayer );
            if (groundHits.Length > 0)
            {
                return groundHits.OrderBy( h => h.distance ).First().point + up * _bulletTargetHeight;
            } else
            {
                return hitTarget;
            }
        }

        /// <summary>
        /// ��������� ���������� � ��������� ��� ��������.
        /// </summary>
        /// <param name="target">���� ��������.</param>
        /// <param name="distance">������������ ��������� ��������.</param>
        /// <returns>��������� ��������.</returns>
        private RayShootingResult GetShootingHit(Vector3 target, float distance)
        {
            Vector3 direction = (target - _shootingPoint.position).normalized;
            var hits = Physics.RaycastAll( _shootingPoint.position, direction, distance, _bulletsHitLayers );
            if (hits.Length > 0)
            {
                var hit = hits.OrderBy( h => h.distance ).First();
                return new RayShootingResult
                {
                    success = true,
                    hitTarget = hit.transform,
                    hitDistance = hit.distance,
                    hitPoint = hit.point
                };
            } else
            {
                return new RayShootingResult
                {
                    hitDistance = distance,
                    hitPoint = _shootingPoint.position + direction * distance
                };
            }
        }

        /// <summary>
        /// ��������� ��� �������� ���������� �������� ���� Ray.
        /// </summary>
        private struct RayShootingResult
        {
            public bool success;
            public Transform hitTarget;
            public float hitDistance;
            public Vector3 hitPoint;
        }
    }
}