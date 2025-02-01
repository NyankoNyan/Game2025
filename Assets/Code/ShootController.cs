using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace NN
{
    /// <summary>
    /// Контроллер стрельбы, управляющий стрельбой из различных видов оружия.
    /// </summary>
    public class ShootController : MonoBehaviour
    {
        [Tooltip( "Количество слотов для оружия/навыков." )]
        [SerializeField]
        private int _skillSlotsCount = 2;

        [Tooltip( "Точка, из которой происходит стрельба." )]
        [SerializeField]
        private Transform _shootingPoint;

        [Tooltip( "Слои, по которым могут попадать снаряды." )]
        [SerializeField]
        private LayerMask _bulletsHitLayers = ~0;

        [Tooltip( "Маска слой земли. Нужен для понимания на какое расстояние от земли стреляет игрок." )]
        [SerializeField]
        private LayerMask _groundLayer;

        [Tooltip( "Целевая высота снаряда в целевом месте стрельбы." )]
        [SerializeField]
        private float _bulletTargetHeight = 1.5f;

        [Tooltip( "Максимальная дистанция проверки земли." )]
        [SerializeField]
        private float _groundTestDistance = 50f;

        [Header( "Controls" )]
        [Tooltip( "Инпуты InputSystem для скиллов." )]
        [SerializeField]
        private InputActionReference[] _skillActions;

        [Tooltip( "Ввод InputSystem для позиции мыши." )]
        [SerializeField]
        private InputActionReference _mousePositionAction;

        [Header( "Initialize" )]
        [Tooltip( "Идентификаторы скиллов по умолчанию по слотам." )]
        [SerializeField]
        private string[] _defaultIds = new[] { "RIFLE" };

        private WeaponPreset[] _currentPresets;
        private bool[] _performedSkills;
        private bool _initialized;

        /// <summary>
        /// Инициализация контроллера стрельбы.
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
        /// Стартовая инициализация
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
        /// Проверка полей на корректность.
        /// </summary>
        private void ValidateFields()
        {
            Assert.IsNotNull( _mousePositionAction, "Mouse position action is not assigned." );
            Assert.AreEqual( _skillSlotsCount, _skillActions.Length, "Skill actions count does not match skill slots count." );
            Assert.IsTrue( _skillSlotsCount >= _defaultIds.Length, "Skill slots count is less than default IDs count." );
        }

        /// <summary>
        /// Инициализация пресетов скиллов по умолчанию.
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
        /// Обновление состояния контроллера.
        /// </summary>
        private void Update()
        {
            if (_initialized)
            {
                UpdateShooting();
            }
        }

        /// <summary>
        /// Обновление состояния стрельбы.
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
        /// Обработка стрельбы в зависимости от типа оружия.
        /// </summary>
        /// <param name="weaponPreset">Пресет оружия.</param>
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
        /// Стрельба из оружия типа Ray.
        /// </summary>
        /// <param name="weaponPreset">Пресет оружия.</param>
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
        /// Получение целевой позиции для стрельбы в зависимости от положения указателя.
        /// </summary>
        /// <param name="up">Направление вверх.</param>
        /// <returns>Координаты цели.</returns>
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
        /// Получение информации о попадании при стрельбе.
        /// </summary>
        /// <param name="target">Цель стрельбы.</param>
        /// <param name="distance">Максимальная дистанция стрельбы.</param>
        /// <returns>Результат стрельбы.</returns>
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
        /// Структура для хранения результата стрельбы типа Ray.
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