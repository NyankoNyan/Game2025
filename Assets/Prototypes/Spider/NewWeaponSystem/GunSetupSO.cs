using UnityEngine;

namespace NewWeaponSystem
{
    /// <summary>
    /// SerializableObject для GunSetup
    /// </summary>
    [CreateAssetMenu(fileName = "GunSetup", menuName = "NewWeaponSystem/GunSetup")]
    public class GunSetupSO : ScriptableObject, IGunSetup
    {
        [SerializeField]
        private GameObject _modelPrefab;

        [SerializeField]
        private GunShootSetupSO _shootSetup;

        public GunSetup Get() => new GunSetup()
        {
            ModelPrefab = _modelPrefab,
            ShootSetup = _shootSetup
        };
    }
}