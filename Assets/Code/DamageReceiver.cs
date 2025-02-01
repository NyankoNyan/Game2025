using Unity.Netcode;
using UnityEngine;

namespace NN
{
    public struct DamageImpulse : INetworkSerializable
    {
        public DamageImpulseType damageImpulseType;
        public float force;
        public Vector3 point;
        public Vector3 direction;
        /// <summary>
        /// Расстояние на котором сила падает в два раза
        /// </summary>
        public float damping;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue( ref damageImpulseType );
            serializer.SerializeValue( ref force );
            serializer.SerializeValue( ref point );
            serializer.SerializeValue( ref direction );
            serializer.SerializeValue( ref damping );
        }

        public enum DamageImpulseType
        {
            Directional,
            Radial
        }
    }

    public class DamageReceiver : NetworkBehaviour
    {
        [SerializeField]
        protected float _maxHealth = 100f;

        [SerializeField]
        private bool _writeLogs = true;

        protected NetworkVariable<float> _currentHealth = new();

        protected bool _dead;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentHealth.Value = _maxHealth;
            }
        }

        /// <summary>
        /// Наносит урон объекту. Server only
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="damageImpulse"></param>
        /// <exception cref="System.Exception"></exception>
        public void ReceiveDamage(float damage, DamageImpulse damageImpulse)
        {
            if (!IsServer)
                throw new System.Exception( "Only server can receive damage" );

            if (_dead)
            {
                Debug.LogWarning( $"{transform.name}({NetworkObjectId}) is already dead" );
                return;
            }

            _currentHealth.Value -= damage;
            if (_writeLogs)
            {
                Debug.Log( $"{transform.name}({NetworkObjectId}) received {damage} damage" );
            }
            if (_currentHealth.Value <= 0)
            {
                _currentHealth.Value = 0;
                _dead = true;
                if (_writeLogs)
                {
                    Debug.Log( $"{transform.name}({NetworkObjectId}) is dead on Server" );
                }
                KillClientRpc( damageImpulse );
            }
        }

        /// <summary>
        /// Создаёт визуальное отображения уничтожения объекта.
        /// </summary>
        /// <param name="damageImpulse"></param>
        protected virtual void Kill(DamageImpulse damageImpulse)
        {
        }

        [ClientRpc]
        private void KillClientRpc(DamageImpulse damageImpulse, ClientRpcParams clientRpc = default)
        {
            _dead = true;
            if (_writeLogs)
            {
                Debug.Log( $"{transform.name}({NetworkObjectId}) is dead on Client" );
            }
            Kill( damageImpulse );
        }
    }
}