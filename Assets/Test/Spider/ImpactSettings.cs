using System;
using System.Linq;
using UnityEngine;

namespace NN
{
    [Serializable]
    public class ImpactEffect
    {
        [SerializeField]
        private AudioClip _sound;

        [SerializeField]
        private GameObject _effect;

        [SerializeField]
        private GameObject _decal;

        public AudioClip Sound => _sound;
        public GameObject Effect => _effect;
        public GameObject Decal => _decal;
    }

    [Serializable]
    public class ClassifiedImpactEffect
    {
        [SerializeField]
        private string _contactMaterial;

        [SerializeField]
        private ImpactEffect _effect;

        public string ContactMaterial => _contactMaterial;
        public ImpactEffect Effect => _effect;
    }

    public interface IImpactCollection
    {
        ImpactEffect GetEffect(string contactMaterial);
    }

    [CreateAssetMenu( fileName = "ImpactSetting", menuName = "NN/ImpactSettings" )]
    public class ImpactSettings : ScriptableObject, IImpactCollection
    {
        [SerializeField]
        private ClassifiedImpactEffect[] _effects;

        [SerializeField]
        private ImpactEffect _defaultEffect;

        public ImpactEffect GetEffect(string contactMaterial)
        {
            var result = _effects.SingleOrDefault( e => e.ContactMaterial == contactMaterial )?.Effect;
            if (result == null)
            {
                result = _defaultEffect;
            }
            return result;
        }
    }
}