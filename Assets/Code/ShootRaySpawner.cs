using UnityEngine;

namespace NN
{
    public class ShootRaySpawner : MonoBehaviour
    {
        private static ShootRaySpawner _instance;

        public static ShootRaySpawner Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<ShootRaySpawner>();
                    if (!_instance)
                    {
                        Debug.LogError( $"{nameof( ShootRaySpawner )} not found" );
                    }
                }
                return _instance;
            }
        }

        public void SpawnRay(Vector3 from, Vector3 to, string id)
        {
            var weaponPreset = SkillsAndWeapons.Instance.GetPreset( id );

            GameObject go = PoolManager.Instance.GetInstance( weaponPreset.prefab );
            if (weaponPreset.lifeTime > 0)
            {
                PoolManager.Instance.ReleaseInstance( go, weaponPreset.lifeTime );
            }

            Vector3 toTarget = to - from;
            Quaternion rotation = Quaternion.LookRotation( toTarget.normalized, Vector3.up );

            go.transform.SetPositionAndRotation( from, rotation );
            go.transform.localScale = new Vector3( 1, 1, toTarget.magnitude );
        }
    }
}