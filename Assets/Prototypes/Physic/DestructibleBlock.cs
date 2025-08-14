using NN;
using UnityEngine;

namespace Test
{
    internal interface IDestructible
    {
        void InstantKill();

        void AddDamage(int damage);
    }

    [RequireComponent(typeof(Rigidbody))]
    public class DestructibleBlock : MonoBehaviour, IDestructible
    {
        [SerializeField]
        private int _healthBase = 100;

        [SerializeField]
        private int _healthRemain;

        [SerializeField]
        private float _physicsDamageMultiplier = 1f;

        [SerializeField]
        private bool _drawGizmos;

        private Rigidbody _rigidbody;
        private FixedJoint[] _joints;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (_healthRemain <= 0)
            {
                _healthRemain = _healthBase;
            }

            _joints = GetComponents<FixedJoint>();
        }

        public void Setup(int healthBase, float physicsDamageMultiplier = 1f, bool drawGizmos = false)
        {
            _healthBase = _healthRemain = healthBase;
            _physicsDamageMultiplier = physicsDamageMultiplier;
            _drawGizmos = drawGizmos;
        }

        public void AddDamage(int damage)
        {
            if (_healthRemain <= 0)
            {
                return; // Already destroyed
            }
            _healthRemain -= damage;
            if (_healthRemain <= 0)
            {
                Destruct();
            }
        }

        public void InstantKill()
        {
            Destruct();
        }

        private void Destruct()
        {
            ScenePools.Instance.Remove(this.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float deltaV;
            Vector3 normal = collision.GetContact(0).normal;
            if (collision.rigidbody)
            {
                var relativeVelocity = Vector3.ProjectOnPlane(collision.rigidbody.linearVelocity - _rigidbody.linearVelocity, normal);
                deltaV = relativeVelocity.magnitude * collision.rigidbody.mass / (collision.rigidbody.mass + _rigidbody.mass);
            }
            else
            {
                deltaV = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, normal).magnitude;
            }
            int damage = Mathf.FloorToInt(deltaV * _physicsDamageMultiplier);
            if (damage > 0)
            {
                Debug.Log($"{collision.gameObject.name} hit {gameObject.name} with damage {damage}");
                AddDamage(damage);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
            {
                return;
            }
            foreach (var joint in _joints)
            {
                if (joint && joint.connectedBody)
                {
                    Gizmos.color = Color.Lerp(Color.green, Color.red, joint.currentForce.magnitude / joint.breakForce);

                    Gizmos.DrawLine(transform.position, joint.connectedBody.position);
                }
            }
        }
    }
}