using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace NewWeaponSystem
{
    [RequireComponent(typeof(GunChanger))]
    public class DefaultGunsLoader : MonoBehaviour
    {
        [SerializeField]
        List<GunSetupSO> _gunSetups;

        
        private void Start()
        {
            var gunChanger = GetComponent<GunChanger>();
            foreach(var gunSetup in _gunSetups)
            {
                gunChanger.AddGun(gunSetup);
            }
        }
    }
}