using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    [Serializable]
    public class CityGenerationSettings
    {
        [SerializeField]
        public Vector2Int ZoneSize = new( 64, 64 );

        [SerializeField]
        public Vector2Int GrowLimitMin = new( 1, 1 );

        [SerializeField]
        public Vector2Int GrowLimitMax = new( 3, 3 );

        [SerializeField]
        public float GrowProbability = .5f;
    }

    /// <summary>
    /// Класс для генерации карты зон города.
    /// </summary>
    public class CityZoneGenerator
    {
        private byte[,] _zoneMap;

        /// <summary>
        /// Создать карту зон города.
        /// </summary>
        /// <param name="settings">Настройки генерации города.</param>
        /// <param name="seed">Сид для генерации случайных чисел.</param>
        public void CreateZoneMap(CityGenerationSettings settings, int seed)
        {
            if (seed != -1)
            {
                Random.InitState( seed );
            }

            _zoneMap = new byte[settings.ZoneSize.x, settings.ZoneSize.y];
            for (int x = 0; x < settings.ZoneSize.x; x++)
            {
                for (int y = 0; y < settings.ZoneSize.y; y++)
                {
                    if (_zoneMap[x, y] == 0)
                    {
                        if (Random.value < settings.GrowProbability)
                        {
                            int growX = Random.Range( settings.GrowLimitMin.x, settings.GrowLimitMax.x + 1 );
                            int growY = Random.Range( settings.GrowLimitMin.y, settings.GrowLimitMax.y + 1 );
                            growX = Mathf.Min( growX, settings.ZoneSize.x - x );
                            growY = Mathf.Min( growY, settings.ZoneSize.y - y );

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

        /// <summary>
        /// Получить сгенерированную карту зон.
        /// </summary>
        /// <returns>Карта зон.</returns>
        public byte[,] GetZoneMap()
        {
            return _zoneMap;
        }
    }
}