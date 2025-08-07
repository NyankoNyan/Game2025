using UnityEngine;

namespace NN
{
    public interface IMissileSettings
    {
        GameObject MissilePrefab { get; }

        GameObject EmitterEffectPrefab { get; }

        ImpactSettings ImpactCollection { get; }
    }

    [CreateAssetMenu( fileName = "Missile", menuName = "NN/MissileSettings" )]
    public class MissileSettings : ScriptableObject, IMissileSettings
    {
        [SerializeField]
        private GameObject _missilePrefab;

        [SerializeField]
        private GameObject _emitterEffectPrefab;

        [SerializeField]
        private ImpactSettings _impactCollection;

        public GameObject MissilePrefab => _missilePrefab;

        public GameObject EmitterEffectPrefab => _emitterEffectPrefab;

        public ImpactSettings ImpactCollection => _impactCollection;
    }
}