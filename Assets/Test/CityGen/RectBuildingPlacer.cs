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
    internal class CityBlockGenerationSettings
    {
        [Tooltip( "Размер области размещения зданий в логических попугаях" )]
        public Vector2Int Size = new( 10, 20 );

        public BuildingPriorityList[] BuildingPriorityLists;
    }

    // В этом классе мне немного помог копилот, он основную хуйню написал я. А ещё он целочисленный, поэтому готовимся к артефактикам.
    public class RectBuildingPlacer : MonoBehaviour
    {
        [SerializeField]
        private CityBlockGenerationSettings _gs; // Настройки генерации

        [SerializeField]
        private int _seed = -1; // Сид для генерации случайных чисел

        [SerializeField]
        [Tooltip( "Размер логической единицы в метрах" )]
        private Vector2 _logicalUnitSize = new( 1, 1 ); // Размер логической единицы

        [Tooltip( "Количество попыток размещения здания из одного списка" )]
        public int PlacementTrying = 10;

        [Tooltip( "Через сколько итераций чистить мусорные углы" )]
        public int GarbageCollectionAfter = 10;

        [SerializeField]
        private GameObject _floorPrefab; // Префаб пола

        private struct SpawnPoint
        {
            public Vector2Int Position;
            public int Corner;

            public SpawnPoint(Vector2Int position, int corner)
            {
                Position = position;
                Corner = corner;
            }

            public override string ToString()
            {
                return $"{Position}-{Corner}";
            }
        }

        private List<SpawnPoint> _spawnPoints = new(); // Список точек для размещения зданий
        private List<RectInt> _occupiedAreas = new(); // Список занятых областей

        /// <summary>
        /// Массив множителей для смещений углов здания относительно центра.
        /// </summary>
        private (int, int)[] _m = { (-1, -1), (-1, 1), (1, 1), (1, -1) };

        /// <summary>
        /// Потенциальные точки размещения нового здания при размещении в другой точке.
        /// Представляет собой массив [4], в котором описан каждый угол здания по индексу.
        /// Для каждого угла есть N точек, описанных парой чисел (x, y), где x - другой угол текущего здания, y - новый угол.
        /// </summary>
        private (int, int)[][] _pointGen = {
                    new (int, int)[] { (1, 0), (1, 2), (2, 1), (2, 3), (3, 0), (3, 2) },
                    new (int, int)[] { (2, 1), (2, 3), (3, 0), (3, 2), (0, 1), (0, 3) },
                    new (int, int)[] { (1, 2), (1, 0), (0, 1), (0, 3), (3, 2), (3, 0) },
                    new (int, int)[] { (2, 3), (2, 1), (1, 0), (1, 2), (0, 3), (0, 1) },
                };

        private void Start()
        {
            PlaceBuildings();
            PlaceFloor();
        }

        private void PlaceBuildings()
        {
            if (_seed != -1)
                UnityEngine.Random.InitState( _seed );

            // Сортируем списки по приоритету
            Array.Sort( _gs.BuildingPriorityLists, (a, b) => a.Priority.CompareTo( b.Priority ) );

            // Находим минимальный размер здания
            Vector2Int minimalSize = _gs.BuildingPriorityLists
                .SelectMany( x => x.Buildings )
                .Select( x => x.Size )
                .Aggregate( (a, b) =>
                    new Vector2Int( Math.Min( a.x, b.x ), Math.Min( a.y, b.y )
                    ) );

            // Инициализируем стартовые точки размещения
            for (int i = 0; i < _m.Length; i++)
            {
                _spawnPoints.Add( new( new( _m[i].Item1 * _gs.Size.x, _m[i].Item2 * _gs.Size.y ), i ) );
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
                foreach (var priorityList in _gs.BuildingPriorityLists)
                {
                    for (int i = 0; i < PlacementTrying; i++)
                    {
                        // Выбираем случайную точку размещения
                        pointIndex = UnityEngine.Random.Range( 0, _spawnPoints.Count );
                        spawnPoint = _spawnPoints[pointIndex];

                        // Выбираем случайное здание из списка
                        int buildingIndex = UnityEngine.Random.Range( 0, priorityList.Buildings.Length );
                        building = priorityList.Buildings[buildingIndex];

                        // Вычисляем центр здания
                        center = GetCenter( building.Size, spawnPoint );

                        // Проверяем, можно ли разместить здание
                        if (IsPositionValid( center, building.Size ))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        continue;

                    // Размещаем здание
                    PlaceBuilding( building.Prefab, center );
                    _occupiedAreas.Add( new RectInt( center - building.Size, building.Size * 2 ) );

                    // Удаляем использованную точку
                    _spawnPoints.RemoveAt( pointIndex );

                    // Добавляем новые потенциальные точки
                    foreach (var pg in _pointGen[spawnPoint.Corner])
                    {
                        SpawnPoint checkSpawnPoint = new( new( center.x + building.Size.x * _m[pg.Item1].Item1,
                                                             center.y + building.Size.y * _m[pg.Item1].Item2 ),
                                                          pg.Item2 );
                        if (IsSpawnValid( checkSpawnPoint ))
                        {
                            _spawnPoints.Add( checkSpawnPoint );
                        }
                    }
                    break;
                }

                placeCounter++;
                if (placeCounter >= GarbageCollectionAfter)
                {
                    // Удаляем невалидные точки
                    for (int i = _spawnPoints.Count - 1; i >= 0; i--)
                    {
                        if (!IsSpawnValid( _spawnPoints[i] ))
                        {
                            _spawnPoints.RemoveAt( i );
                        }
                    }
                }
            } while (found && _spawnPoints.Count > 0);

            // Локальная функция для вычисления центра здания
            Vector2Int GetCenter(Vector2Int buildingSize, SpawnPoint spawnPoint)
            {
                return new Vector2Int( spawnPoint.Position.x - buildingSize.x * _m[spawnPoint.Corner].Item1,
                                       spawnPoint.Position.y - buildingSize.y * _m[spawnPoint.Corner].Item2 );
            }

            // Локальная функция для проверки валидности точки
            bool IsSpawnValid(SpawnPoint spawnPoint)
            {
                var checkCenter = GetCenter( minimalSize, spawnPoint );
                return IsPositionValid( checkCenter, minimalSize );
            }
        }

        private bool IsPositionValid(Vector2Int position, Vector2Int size)
        {
            // Проверяем, чтобы здание не выходило за границы области
            if (position.x - size.x < -_gs.Size.x || position.x + size.x > _gs.Size.x ||
                position.y - size.y < -_gs.Size.y || position.y + size.y > _gs.Size.y)
            {
                return false;
            }

            // Проверяем, чтобы здание не пересекалось с уже размещенными
            RectInt newRect = new( position - size, size * 2 );
            foreach (var occupied in _occupiedAreas)
            {
                if (newRect.Overlaps( occupied ))
                {
                    return false;
                }
            }

            return true;
        }

        private void PlaceBuilding(GameObject prefab, Vector2Int logicalCenter)
        {
            // Преобразуем логические координаты в мировые
            Vector3 realCenter = transform.localToWorldMatrix * new Vector3( logicalCenter.x * _logicalUnitSize.x / 2, 0, logicalCenter.y * _logicalUnitSize.y / 2 );
            Instantiate( prefab, realCenter, transform.rotation, transform );
        }

        private void PlaceFloor()
        {
            if (_floorPrefab)
            {
                // Размещаем пол
                var floorGO = Instantiate( _floorPrefab, transform.position, transform.rotation, transform );
                floorGO.transform.localScale = new( _gs.Size.x * floorGO.transform.localScale.x,
                                                    floorGO.transform.localScale.y,
                                                    _gs.Size.y * floorGO.transform.localScale.z );
            }
        }
    }
}