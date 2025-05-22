using UnityEngine;

namespace NN
{
    public interface IWeaponSettings
    {
        IMissileSettings MissileSettings { get; }
        float Speed { get; }
        float Damage { get; }
        float ReloadTime { get; }
        float RecoilForce { get; }
        float BlastForce { get; }
        float BlastRaduis { get; }
    }

    [CreateAssetMenu( fileName = "Shoot", menuName = "NN/ShootSettings" )]
    public class WeaponSettings : ScriptableObject, IWeaponSettings
    {
        [SerializeField]
        private MissileSettings _missileSettings;

        [SerializeField]
        private float _speed = 1f;

        [SerializeField]
        private float _damage = 10f;

        [SerializeField]
        private float _blastForce = 10f;

        [SerializeField]
        private float _blastRadius = 1;

        [SerializeField]
        private float _reloadTime = .1f;

        [SerializeField]
        private float _recoilForce = 2f;

        public IMissileSettings MissileSettings => _missileSettings;

        public float Speed => _speed;

        public float Damage => _damage;

        public float ReloadTime => _reloadTime;

        public float RecoilForce => _recoilForce;

        public float BlastForce => _blastForce;

        public float BlastRaduis => _blastRadius;
    }
}