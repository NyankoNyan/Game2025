using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NN
{
    public struct ShootRayInfo : INetworkSerializable
    {
        public Vector3 localFrom;
        public Vector3 localTo;
        public bool successHit;
        public ulong hitTargetNetId;
        public FixedString32Bytes weaponPresetId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue( ref localFrom );
            serializer.SerializeValue( ref localTo );
            serializer.SerializeValue( ref successHit );
            serializer.SerializeValue( ref hitTargetNetId );
            serializer.SerializeValue( ref weaponPresetId );
        }
    }
}