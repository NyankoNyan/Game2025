using UnityEngine;

namespace Test
{
    public class CityPlanCreator : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _zoneSize = new( 64, 64 );

        [SerializeField]
        private Vector2Int _growLimitMin = new( 1, 1 );

        [SerializeField]
        private Vector2Int _growLimitMax = new( 3, 3 );

        [SerializeField]
        private float _growProbability = .5f;

        [SerializeField]
        private GameObject _groundPrefab;

        [SerializeField]
        private Vector2 _scale = new( 2, 2 );

        [SerializeField]
        private float _padding = 1f;

        [SerializeField]
        private int _seed = -1;

        private byte[,] _zoneMap;

        private void Start()
        {
            CheckInputs();
            CreateZoneMap();
            PlaceGround();
        }

        private void CheckInputs()
        {
            if (_zoneSize.x < 1 || _zoneSize.y < 1)
                Debug.LogError( "Zone size must be greater than 0" );
            if (_growLimitMin.x < 1 || _growLimitMin.y < 1)
                Debug.LogError( "Grow limit min must be greater than 0" );
            if (_growLimitMax.x < _growLimitMin.x || _growLimitMax.y < _growLimitMin.y)
                Debug.LogError( "Grow limit max must be greater than grow limit min" );
            if (_growProbability < 0f || _growProbability > 1f)
                Debug.LogError( "Grow probability must be between 0 and 1" );
            if (_growLimitMax.x >= 15 || _growLimitMax.y >= 15)
                Debug.LogError( "Grow limit max must be less than 15" );
        }

        private void CreateZoneMap()
        {
            if (_seed != -1)
            {
                Random.InitState( _seed );
            }

            _zoneMap = new byte[_zoneSize.x, _zoneSize.y];
            for (int x = 0; x < _zoneSize.x; x++)
            {
                for (int y = 0; y < _zoneSize.y; y++)
                {
                    if (_zoneMap[x, y] == 0)
                    {
                        if (Random.value < _growProbability)
                        {
                            int growX = Random.Range( _growLimitMin.x, _growLimitMax.x + 1 );
                            int growY = Random.Range( _growLimitMin.y, _growLimitMax.y + 1 );
                            growX = Mathf.Min( growX, _zoneSize.x - x );
                            growY = Mathf.Min( growY, _zoneSize.y - y );

                            bool check = true;

                            for (int i = 0; i < growX; i++)
                            {
                                for (int j = 0; j < growY; j++)
                                {
                                    if (_zoneMap[x + i, y + j] != 0)
                                    {
                                        check = false;
                                        break;
                                    }
                                }
                                if (!check)
                                    break;
                            }

                            if (!check)
                            {
                                _zoneMap[x, y] = 0x11;
                                continue;
                            }
                            for (int i = 0; i < growX; i++)
                            {
                                for (int j = 0; j < growY; j++)
                                {
                                    _zoneMap[x + i, y + j] = 255;
                                }
                            }

                            _zoneMap[x, y] = (byte)((growX << 4) | growY);
                        } else
                        {
                            _zoneMap[x, y] = 0x11;
                        }
                    }
                }
            }
        }

        private void PlaceGround()
        {
            for (int x = 0; x < _zoneSize.x; x++)
            {
                for (int y = 0; y < _zoneSize.y; y++)
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
                        GameObject ground = Instantiate( _groundPrefab, position, Quaternion.identity, transform );
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