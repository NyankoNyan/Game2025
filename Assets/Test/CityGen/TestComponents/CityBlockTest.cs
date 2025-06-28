using System;
using UnityEngine;

namespace Test
{
    /// <summary>
    /// Класс для тестирования генерации зданий.
    /// </summary>
    [RequireComponent( typeof( CityBlockGenerator ) )]
    public class CityBlockTest : MonoBehaviour
    {
        [SerializeField]
        private CityBlockGenerationSettings _gs; // Настройки генерации

        [SerializeField]
        private int _seed = -1; // Сид для генерации случайных чисел

        [SerializeField]
        private GameObject _floorPrefab; // Префаб пола

        private void Start()
        {
            // Создаем экземпляр генератора
            var generator = GetComponent<CityBlockGenerator>();
            generator.GenerateBuildings( _seed, _gs, transform );
            PlaceFloor();
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