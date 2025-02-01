using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NN
{
    [CreateAssetMenu( fileName = "Weapons", menuName = "NN/WeaponPresetListSO" )]
    public class WeaponPresetListSO : ScriptableObject, IWeaponPresetBank
    {
        public List<WeaponPreset> presets = new();

        public IEnumerable<WeaponPreset> Weapons => presets;

        public WeaponPreset GetWeapon(string id)
        {
            return presets.Single( w => w.id == id );
        }
    }
}