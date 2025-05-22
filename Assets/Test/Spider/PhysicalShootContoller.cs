using UnityEngine;
using UnityEngine.InputSystem;

namespace NN
{
    internal class WeaponState
    {
        public float LastShootTime;
    }

    public class PhysicalShootContoller : MonoBehaviour
    {
        [SerializeField]
        private Transform _physicsModelEmitter;

        [SerializeField]
        private Transform _visualEmitter;

        [SerializeField]
        private WeaponSettings _weapon;

        [SerializeField]
        private InputActionReference _shootInput;

        private WeaponState _weaponState = new();

        private void OnEnable()
        {
            if (_shootInput)
            {
                _shootInput.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (_shootInput)
            {
                _shootInput.action.Disable();
            }
        }

        private void Update()
        {
            bool isShoot = false;
            if (_shootInput)
                isShoot = _shootInput.action.ReadValue<float>() > 0;

            if (isShoot)
                MakeShoot( _weapon, _weaponState );
        }

        private void MakeShoot(IWeaponSettings weaponSettings, WeaponState weaponState)
        {
            float timeFromLastShoot = Time.time - weaponState.LastShootTime;
            if (timeFromLastShoot < weaponSettings.ReloadTime)
                return;
            weaponState.LastShootTime = Time.time;

            var rb = _physicsModelEmitter.GetComponentInParent<Rigidbody>();
            rb.AddForceAtPosition( -_physicsModelEmitter.forward * weaponSettings.RecoilForce, _physicsModelEmitter.position, ForceMode.Impulse );

            //Instancing
            var missileGO = ScenePools.Instance.Get( weaponSettings.MissileSettings.MissilePrefab );
            missileGO.transform.SetPositionAndRotation( _visualEmitter.position, _visualEmitter.rotation );
            var missileRB = missileGO.GetComponent<Rigidbody>();
            if (missileRB)
            {
                missileRB.angularVelocity = Vector3.zero;
                missileRB.linearVelocity = missileGO.transform.forward * weaponSettings.Speed;
            }

            var missile = missileGO.GetComponent<Missile>();
            if (missile)
            {
                missile.Setup( weaponSettings );
            }

            var effectGO = ScenePools.Instance.Get( weaponSettings.MissileSettings.EmitterEffectPrefab );
            effectGO.transform.SetPositionAndRotation( _visualEmitter.position, _visualEmitter.rotation );
        }
    }
}