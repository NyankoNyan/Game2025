using System;
using UnityEngine;

namespace Test
{
    /// <summary>
    /// ����� ��� ������������ ��������� ������.
    /// </summary>
    [RequireComponent( typeof( CityBlockGenerator ) )]
    public class CityBlockTest : MonoBehaviour
    {
        [SerializeField]
        private CityBlockGenerationSettings _gs; // ��������� ���������

        [SerializeField]
        private int _seed = -1; // ��� ��� ��������� ��������� �����

        [SerializeField]
        private GameObject _floorPrefab; // ������ ����

        private void Start()
        {
            // ������� ��������� ����������
            var generator = GetComponent<CityBlockGenerator>();
            generator.GenerateBuildings( _seed, _gs, transform );
            PlaceFloor();
        }

        private void PlaceFloor()
        {
            if (_floorPrefab)
            {
                // ��������� ���
                var floorGO = Instantiate( _floorPrefab, transform.position, transform.rotation, transform );
                floorGO.transform.localScale = new( _gs.Size.x * floorGO.transform.localScale.x,
                                                    floorGO.transform.localScale.y,
                                                    _gs.Size.y * floorGO.transform.localScale.z );
            }
        }
    }
}