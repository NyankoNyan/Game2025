using System;
using UnityEngine;

namespace NN
{
    [Serializable]
    public class WeaponPreset
    {
        public string id;
        public string name;
        public WeaponType weaponType;

        public GameObject prefab;

        public float damage;
        public float reloadTime;
        public float skillLockTime;
        public float lifeTime;
        public float maxDistance;

        public enum WeaponType
        {
            Ray
        }
    }
}
