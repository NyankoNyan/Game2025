using NN;
using UnityEngine;

namespace Test
{
    public class GPUInstancingTest : MonoBehaviour
    {
        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private Vector3Int _gridSize = new( 10, 10, 10 );

        [SerializeField]
        private Vector3 _offset = new( 2, 2, 2 );

        private void Start()
        {
            SpawnObjects();
        }

        private void SpawnObjects()
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                for (int y = 0; y < _gridSize.y; y++)
                {
                    for (int z = 0; z < _gridSize.z; z++)
                    {
                        Vector3 position = new Vector3( x * _offset.x, y * _offset.y, z * _offset.z );
                        Matrix4x4 matrix = Matrix4x4.TRS( position, Quaternion.identity, Vector3.one );
                        // Добавляем объект в пул для GPU Instancing
                        GPUInstancePools.Instance.AddInstance( _prefab, matrix );
                    }
                }
            }
        }
    }
}