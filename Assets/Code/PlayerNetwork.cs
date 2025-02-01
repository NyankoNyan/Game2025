using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace NN
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField]
        Transform _cameraTarget;

        NetworkVariable<Quaternion> _rotation = new( default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner );

        public override void OnNetworkSpawn()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            if (IsOwner)
            {
                GetComponent<MoveController>()?.Init();
                GetComponent<ShootController>()?.Init();
                var camera = GameObject.FindGameObjectWithTag( "CinemachineMainCamera" ).GetComponent<CinemachineCamera>();
                Assert.IsNotNull( camera );
                camera.Follow = _cameraTarget ?? transform;
                gameObject.tag = "Player";
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                _rotation.Value = transform.rotation;
            } else
            {
                transform.rotation = _rotation.Value;
            }
        }
    }
}
