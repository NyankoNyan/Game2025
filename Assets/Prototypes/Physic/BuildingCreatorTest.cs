using UnityEngine;

namespace Test
{
    public class BuildingCreatorTest : MonoBehaviour
    {
        [SerializeField]
        private GameObject _partPrefab;

        [SerializeField]
        private float _breakForce = 10f;

        [SerializeField]
        private float _breakTorque = 10f;

        [SerializeField]
        private Vector3Int _gridSize = new Vector3Int( 3, 10, 3 );

        [SerializeField]
        private Vector3 _partSize = new Vector3( 1f, 1f, 1f );

        [SerializeField]
        private Vector3 _offset = new Vector3( 0f, 0.5f, 0f );

        private GameObject[,,] _spawnedParts;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            CreateBuilding();
        }

        private void CreateBuilding()
        {
            _spawnedParts = new GameObject[_gridSize.x, _gridSize.y, _gridSize.z];
            for (int x = 0; x < _gridSize.x; x++)
            {
                for (int y = 0; y < _gridSize.y; y++)
                {
                    for (int z = 0; z < _gridSize.z; z++)
                    {
                        Vector3 position = new Vector3(
                            x * _partSize.x + _offset.x,
                            y * _partSize.y + _offset.y,
                            z * _partSize.z + _offset.z );
                        GameObject part = Instantiate( _partPrefab, position + transform.position, Quaternion.identity * transform.rotation );
                        part.gameObject.name += $"_{x}_{y}_{z}";
                        if (x > 0)
                        {
                            AddJoint( x - 1, y, z, part );
                        }
                        if (y > 0)
                        {
                            AddJoint( x, y - 1, z, part );
                        } else
                        {
                            var joint = part.AddComponent<FixedJoint>();
                            joint.connectedBody = _rigidbody;
                            joint.breakForce = _breakForce;
                            joint.breakTorque = _breakTorque;
                        }
                        if (z > 0)
                        {
                            AddJoint( x, y, z - 1, part );
                        }

                        _spawnedParts[x, y, z] = part;
                    }
                }
            }

            void AddJoint(int x, int y, int z, GameObject part)
            {
                var joint = part.AddComponent<FixedJoint>();
                joint.connectedBody = _spawnedParts[x, y, z].GetComponent<Rigidbody>();
                joint.breakForce = _breakForce;
                joint.breakTorque = _breakTorque;
            }
        }
    }
}