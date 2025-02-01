using UnityEngine;

namespace NN
{
    public class EnvironmentDamageReceiver : DamageReceiver
    {
        [SerializeField]
        private GameObject _ruinsPrefab;

        [SerializeField]
        private float _ruinsLifetime = 30f;

        protected override void Kill(DamageImpulse damageImpulse)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild( i ).gameObject.SetActive( false ); ;
            }

            GameObject ruins = PoolManager.Instance.GetInstance( _ruinsPrefab );
            if (_ruinsLifetime > 0)
            {
                PoolManager.Instance.ReleaseInstance( ruins, _ruinsLifetime );
            }

            ruins.transform.SetPositionAndRotation( transform.position, transform.rotation );

            foreach (var rb in ruins.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 pathToRB = rb.transform.position - damageImpulse.point;
                float force;

                if (damageImpulse.damping > 0)
                {
                    force = damageImpulse.force / Mathf.Pow( 2, pathToRB.magnitude / damageImpulse.damping );
                } else
                {
                    force = damageImpulse.force;
                }

                Vector3 direction;

                switch (damageImpulse.damageImpulseType)
                {
                    case DamageImpulse.DamageImpulseType.Directional:
                    {
                        direction = damageImpulse.direction;
                    }
                    break;

                    case DamageImpulse.DamageImpulseType.Radial:
                    {
                        direction = pathToRB.normalized;
                    }
                    break;

                    default:
                        throw new System.Exception( $"Unknown damage impulse type {damageImpulse.damageImpulseType}" );
                }

                rb.AddForce( direction * force, ForceMode.Impulse );
            }
        }
    }
}