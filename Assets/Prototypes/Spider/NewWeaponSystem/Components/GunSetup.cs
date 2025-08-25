using System;
using UnityEngine;

namespace NewWeaponSystem
{
    public interface IGunSetup
    {
        GunSetup Get();
    }

    [Serializable]
    public class GunSetup : IGunSetup
    {
        public GameObject ModelPrefab;
        public IGunShootSetup ShootSetup;

        public GunSetup Get() => this;
    }
}