using System.Collections.Generic;

namespace NN
{
    public interface IWeaponPresetBank
    {
        IEnumerable<WeaponPreset> Weapons { get; }
        WeaponPreset GetWeapon(string id);
    }
}
