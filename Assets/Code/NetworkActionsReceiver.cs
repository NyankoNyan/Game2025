using Unity.Netcode;
using UnityEngine;

namespace NN
{

    public interface INetworkActionsReceiver
    {
        void ShootRay(ShootRayInfo shootRayInfo);
    }

    public class NetworkActionsReceiver : NetworkBehaviour, INetworkActionsReceiver
    {
        private static NetworkActionsReceiver _instance;

        public static INetworkActionsReceiver Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<NetworkActionsReceiver>();
                    if (!_instance)
                    {
                        Debug.LogError( $"{nameof( NetworkActionsReceiver )} not found" );
                    }
                }
                return _instance;
            }
        }

        public void ShootRay(ShootRayInfo shootRayInfo)
        {
            ShootRayServerRpc( shootRayInfo );
        }

        [ServerRpc( RequireOwnership = false )]
        private void ShootRayServerRpc(ShootRayInfo shootRayInfo, ServerRpcParams serverParams = default)
        {
            ulong sender = serverParams.Receive.SenderClientId;

            if (shootRayInfo.successHit)
            {
                if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue( shootRayInfo.hitTargetNetId, out NetworkObject networkObject ))
                {
                    DamageReceiver damageReceiver = networkObject.GetComponent<DamageReceiver>();
                    if (damageReceiver)
                    {
                        var weaponPreset = SkillsAndWeapons.Instance.GetPreset( shootRayInfo.weaponPresetId.ToString() );

                        damageReceiver.ReceiveDamage( weaponPreset.damage, new DamageImpulse()
                        {
                            damageImpulseType = DamageImpulse.DamageImpulseType.Directional,
                            force = 1000f * weaponPreset.damage, // TODO придумать формулу и где настраивать
                            point = shootRayInfo.localTo,
                            direction = (networkObject.transform.position - shootRayInfo.localTo).normalized,
                            damping = 0 // TODO придумать как настраивать и подобрать значения
                        } );
                    }
                } else
                {
                }
            }

            ShootRayClientRpc( shootRayInfo, sender );
        }

        [ClientRpc]
        private void ShootRayClientRpc(ShootRayInfo shootRayInfo, ulong shootOwner, ClientRpcParams clientParams = default)
        {
            if (shootOwner == NetworkManager.Singleton.LocalClientId)
                return;

            ShootRaySpawner.Instance.SpawnRay( shootRayInfo.localFrom, shootRayInfo.localTo, shootRayInfo.weaponPresetId.ToString() );
        }
    }
}