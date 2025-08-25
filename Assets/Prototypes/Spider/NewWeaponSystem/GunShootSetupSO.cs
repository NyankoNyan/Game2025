using UnityEngine;

namespace NewWeaponSystem
{
    /// <summary>
    /// ScriptableObject для GunChootSetup
    /// </summary>
    [CreateAssetMenu(fileName = "GunShootSetup", menuName = "NewWeaponSystem/GunShootSetup")]
    public class GunShootSetupSO : ScriptableObject, IGunShootSetup
    {
        [SerializeField]
        private GunShootSetup _setup;

        public GunShootSetup Get() => _setup;
    }
}