using UnityEngine;

namespace NN
{
    [RequireComponent( typeof( ParticleSystem ) )]
    public class PoolReturnerForParticleSystem : MonoBehaviour
    {
        private void OnParticleSystemStopped()
        {
            ScenePools.Instance.Remove( gameObject );
        }
    }
}