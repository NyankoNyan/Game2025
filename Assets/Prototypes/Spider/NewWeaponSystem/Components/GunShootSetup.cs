using System;
using UnityEngine;

namespace NewWeaponSystem
{
    public interface IGunShootSetup
    {
        GunShootSetup Get();
    }

    [Serializable]
    public class GunShootSetup: IGunShootSetup
    {
        public GunShootType ShootType;
        public GameObject ShootPrefab;
        public GameObject GunEffectPrefab;
        public GameObject HitEffectPrefab;

        public GunShootSetup Get() => this;
    }
}