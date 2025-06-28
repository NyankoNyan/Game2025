using NN;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Test
{
    [Serializable]
    public class BuildingSettings
    {
        [Tooltip( "Префаб здания. Центр посередине." )]
        public GameObject Prefab;

        [Tooltip( "Размер здания в логических попугаях." )]
        public Vector2Int Size;
    }

    [Serializable]
    public class BuildingPriorityList
    {
        public int Priority;
        public BuildingSettings[] Buildings;
    }

    [Serializable]
    public class CityBlockGenerationSettings : ICloneable
    {
        [Tooltip( "Размер области размещения зданий в логических попугаях" )]
        public Vector2Int Size = new( 10, 20 );

        [Tooltip( "Размер логической единицы в метрах" )]
        public Vector2 LogicalUnitSize = new( 1, 1 );

        public BuildingPriorityList[] BuildingPriorityLists;

        public object Clone()
        {
            return new CityBlockGenerationSettings()
            {
                Size = Size,
                LogicalUnitSize = LogicalUnitSize,
                BuildingPriorityLists = BuildingPriorityLists,
            };
        }
    }

    /// <summary>
    /// Класс для генерации зданий в прямоугольной области.
    /// </summary>
    public class CityBlockGenerator : MonoBehaviour
    {
        [Tooltip( "Количество попыток размещения здания из одного списка" )]
        private int _placementTrying = 10;

        [Tooltip( "Через сколько итераций чистить мусорные углы" )]
        private int _garbageCollectionAfter = 10;

        /// <summary>
        /// Массив множителей для смещений углов здания относительно центра.
        /// </summary>
        private readonly (int, int)[] _m = { (-1, -1), (-1, 1), (1, 1), (1, -1) };

        /// <summary>
        /// Потенциальные точки размещения нового здания при размещении в другой точке.
        /// Представляет собой массив [4], в котором описан каждый угол здания по индексу.
        /// Для каждого угла есть N точек, описанных парой чисел (x, y), где x - другой угол текущего здания, y - новый угол.
        /// </summary>
        private readonly (int, int)[][] _pointGen = {
            new (int, int)[] { (1, 0), (1, 2), (2, 1), (2, 3), (3, 0), (3, 2) },
            new (int, int)[] { (2, 1), (2, 3), (3, 0), (3, 2), (0, 1), (0, 3) },
            new (int, int)[] { (1, 2), (1, 0), (0, 1), (0, 3), (3, 2), (3, 0) },
            new (int, int)[] { (2, 3), (2, 1), (1, 0), (1, 2), (0, 3), (0, 1) },
        };

        /// <summary>
        /// Генерация зданий в указанной области.
        /// </summary>
        /// <param name="seed">Сид для генерации случайных чисел.</param>
        /// <param name="settings">Настройки генерации.</param>
        /// <param name="parent">Трансформ, внутри которого будут размещаться здания.</param>
        public void GenerateBuildings(int seed, CityBlockGenerationSettings settings, Transform parent)
        {
            if (seed != -1)
                UnityEngine.Random.InitState( seed );

            var spawnPoints = new List<SpawnPoint>();
            var occupiedAreas = new List<RectInt>();

            // Сортируем списки по приоритету
            Array.Sort( settings.BuildingPriorityLists, (a, b) => a.Priority.CompareTo( b.Priority ) );

            // Находим минимальный размер здания
            Vector2Int minimalSize = settings.BuildingPriorityLists
                .SelectMany( x => x.Buildings )
                .Select( x => x.Size )
                .Aggregate( (a, b) => new Vector2Int( Math.Min( a.x, b.x ), Math.Min( a.y, b.y ) ) );

            // Инициализируем стартовые точки размещения
            for (int i = 0; i < _m.Length; i++)
            {
                spawnPoints.Add( new SpawnPoint( new Vector2Int( _m[i].Item1 * settings.Size.x, _m[i].Item2 * settings.Size.y ), i ) );
            }

            int placeCounter = 0;
            bool found;

            do
            {
                found = false;

                BuildingSettings building = default;
                Vector2Int center = default;
                int pointIndex = -1;
                SpawnPoint spawnPoint = default;

                // Перебираем списки зданий по приоритету
                foreach (var priorityList in settings.BuildingPriorityLists)
                {
                    for (int i = 0; i < _placementTrying; i++)
                    {
                        // Выбираем случайную точку размещения
                        pointIndex = UnityEngine.Random.Range( 0, spawnPoints.Count );
                        spawnPoint = spawnPoints[pointIndex];

                        // Выбираем случайное здание из списка
                        int buildingIndex = UnityEngine.Random.Range( 0, priorityList.Buildings.Length );
                        building = priorityList.Buildings[buildingIndex];

                        // Вычисляем центр здания
                        center = GetCenter( building.Size, spawnPoint );

                        // Проверяем, можно ли разместить здание
                        if (IsPositionValid( center, building.Size, settings.Size, occupiedAreas ))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        continue;

                    // Размещаем здание
                    PlaceBuilding( building.Prefab, center, settings.LogicalUnitSize, parent );
                    occupiedAreas.Add( new RectInt( center - building.Size, building.Size * 2 ) );

                    // Удаляем использованную точку
                    spawnPoints.RemoveAt( pointIndex );

                    // Добавляем новые потенциальные точки
                    foreach (var pg in _pointGen[spawnPoint.Corner])
                    {
                        SpawnPoint checkSpawnPoint = new( new Vector2Int( center.x + building.Size.x * _m[pg.Item1].Item1,
                                                                        center.y + building.Size.y * _m[pg.Item1].Item2 ),
                                                         pg.Item2 );
                        if (IsSpawnValid( checkSpawnPoint, minimalSize, settings.Size, occupiedAreas ))
                        {
                            spawnPoints.Add( checkSpawnPoint );
                        }
                    }
                    break;
                }

                placeCounter++;
                if (placeCounter >= _garbageCollectionAfter)
                {
                    // Удаляем невалидные точки
                    for (int i = spawnPoints.Count - 1; i >= 0; i--)
                    {
                        if (!IsSpawnValid( spawnPoints[i], minimalSize, settings.Size, occupiedAreas ))
                        {
                            spawnPoints.RemoveAt( i );
                        }
                    }
                }
            } while (found && spawnPoints.Count > 0);
        }

        private Vector2Int GetCenter(Vector2Int buildingSize, SpawnPoint spawnPoint)
        {
            return new Vector2Int( spawnPoint.Position.x - buildingSize.x * _m[spawnPoint.Corner].Item1,
                                  spawnPoint.Position.y - buildingSize.y * _m[spawnPoint.Corner].Item2 );
        }

        private bool IsPositionValid(Vector2Int position, Vector2Int size, Vector2Int areaSize, List<RectInt> occupiedAreas)
        {
            // Проверяем, чтобы здание не выходило за границы области
            if (position.x - size.x < -areaSize.x || position.x + size.x > areaSize.x ||
                position.y - size.y < -areaSize.y || position.y + size.y > areaSize.y)
            {
                return false;
            }

            // Проверяем, чтобы здание не пересекалось с уже размещенными
            RectInt newRect = new( position - size, size * 2 );
            foreach (var occupied in occupiedAreas)
            {
                if (newRect.Overlaps( occupied ))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSpawnValid(SpawnPoint spawnPoint, Vector2Int minimalSize, Vector2Int areaSize, List<RectInt> occupiedAreas)
        {
            var checkCenter = GetCenter( minimalSize, spawnPoint );
            return IsPositionValid( checkCenter, minimalSize, areaSize, occupiedAreas );
        }

        private void PlaceBuilding(GameObject prefab, Vector2Int logicalCenter, Vector2 logicalUnitSize, Transform parent)
        {
            // Преобразуем логические координаты в мировые
            Vector3 realCenter = parent.localToWorldMatrix.MultiplyPoint( 
                new Vector3( logicalCenter.x * logicalUnitSize.x / 2, 0, logicalCenter.y * logicalUnitSize.y / 2 ) );
            GPUInstancePools.Instance.AddInstance( prefab, realCenter, parent.rotation, prefab.transform.localScale );
            //Instantiate( prefab, realCenter, parent.rotation, parent );
        }

        private struct SpawnPoint
        {
            public Vector2Int Position;
            public int Corner;

            public SpawnPoint(Vector2Int position, int corner)
            {
                Position = position;
                Corner = corner;
            }
        }
    }
}