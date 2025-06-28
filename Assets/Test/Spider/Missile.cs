using System.Linq;
using UnityEngine;

namespace NN
{
    public class Missile : MonoBehaviour
    {
        private IWeaponSettings _weaponSettings;

        public void Setup(IWeaponSettings weaponSettings)
        {
            _weaponSettings = weaponSettings;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_weaponSettings.BlastRaduis > 0)
            {
                var colliders = Physics.OverlapSphere( transform.position, _weaponSettings.BlastRaduis, ~0, QueryTriggerInteraction.Ignore );
                foreach (var collider in colliders)
                {
                    TryAddForce( collider.gameObject );
                }
            } else
            {
                TryAddForce( other.gameObject );
            }

            ImpactEffect impactEffects = _weaponSettings.MissileSettings.ImpactCollection.GetEffect( other.tag );
            var effect = ScenePools.Instance.Get( impactEffects.Effect );
            effect.transform.SetPositionAndRotation( transform.position, transform.rotation );

            var decal = ScenePools.Instance.Get( impactEffects.Decal );
            decal.transform.SetPositionAndRotation( transform.position, Quaternion.LookRotation( -Vector3.up ) );
            decal.transform.parent = other.transform;
            SetDecalPositionAndRotation( other, decal.transform );

            ScenePools.Instance.Remove( gameObject );
        }

        private void TryAddForce(GameObject target)
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce( _weaponSettings.BlastForce, transform.position, _weaponSettings.BlastRaduis, 0, ForceMode.Impulse );
            }

            if (target.TryGetComponent<Test.IDestructible>( out var destructible ))
            {
                destructible.AddDamage( (int)_weaponSettings.Damage );
            }
        }

        private void SetDecalPositionAndRotation(Collider collider, Transform decal)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 prevPosition = transform.position - rb.linearVelocity * Time.fixedDeltaTime;
                var hits = Physics.RaycastAll( prevPosition, (transform.position - prevPosition).normalized, rb.linearVelocity.magnitude * 2f );
                var hit = hits.FirstOrDefault( h => h.collider == collider );
                if (hit.collider)
                {
                    decal.SetPositionAndRotation( hit.point, Quaternion.LookRotation( -hit.normal ) );
                }
            }
        }
    }
}