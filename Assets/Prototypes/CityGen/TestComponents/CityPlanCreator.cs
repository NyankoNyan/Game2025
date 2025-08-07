using System;
using UnityEngine;

namespace Test
{
    /// <summary>
    /// ����� ��� ������������ ��������� ����� ��� ������ � ���������� ��������.
    /// </summary>
    public class CityPlanCreator : MonoBehaviour
    {
        [SerializeField]
        private CityGenerationSettings _settings; // ��������� ��������� ������

        [SerializeField]
        private GameObject _groundPrefab; // ������ �����

        [SerializeField]
        private Vector2 _scale = new( 2, 2 ); // ������� ��������

        [SerializeField]
        private float _padding = 1f; // ������ ����� ���������

        [SerializeField]
        private int _seed = -1; // ��� ��� ��������� ��������� �����

        private byte[,] _zoneMap;

        private void Start()
        {
            // ������� ��������� ���
            var zoneGenerator = new CityZoneGenerator();
            zoneGenerator.CreateZoneMap( _settings, _seed );

            // �������� ��������������� ����� ���
            _zoneMap = zoneGenerator.GetZoneMap();

            // ��������� ������� �� �����
            PlaceGround();
        }

        private void PlaceGround()
        {
            for (int x = 0; x < _settings.ZoneSize.x; x++)
            {
                for (int y = 0; y < _settings.ZoneSize.y; y++)
                {
                    byte cellValue = _zoneMap[x, y];
                    if (cellValue != 0 && cellValue != 255)
                    {
                        Vector2 realSize = new(
                            ((cellValue >> 4) - 1) * _padding + (cellValue >> 4) * _scale.x,
                            ((cellValue & 0xF) - 1) * _padding + (cellValue & 0xF) * _scale.y );
                        Vector3 position = transform.localToWorldMatrix
                            * new Vector3( x * (_scale.x + _padding) + realSize.x / 2, 0,
                                           y * (_scale.y + _padding) + realSize.y / 2 );
                        GameObject ground = Instantiate( _groundPrefab, position, transform.rotation, transform );
                        Vector3 scale = ground.transform.localScale;
                        scale.x *= realSize.x;
                        scale.z *= realSize.y;
                        ground.transform.localScale = scale;

                        ground.name += $" ({cellValue >> 4},{cellValue & 0xF})";
                    }
                }
            }
        }
    }
}