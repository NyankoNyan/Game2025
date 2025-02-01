using UnityEngine;

namespace NN
{
    public class SkillsAndWeapons : MonoBehaviour
    {
        private static SkillsAndWeapons _instance;

        public static SkillsAndWeapons Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<SkillsAndWeapons>();
                    if (!_instance)
                    {
                        Debug.LogError( $"{nameof( SkillsAndWeapons )} not found" );
                    }
                }
                return _instance;
            }
        }

        [Tooltip( "Список пресетов оружия/скиллов." )]
        [SerializeField]
        private WeaponPresetListSO _weaponPresets;

        public WeaponPreset GetPreset(string id) => _weaponPresets.GetWeapon( id );
    }
}