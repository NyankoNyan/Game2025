using System;
using System.Linq;
using UnityEngine;

namespace Test
{
    /// <summary>
    /// Класс для тестирования генерации районов города.
    /// </summary>
    public class DistrictTreeTest : MonoBehaviour
    {
        [SerializeField]
        private CityDistrictGenerationSettings _settings; // Настройки генерации районов

        [SerializeField]
        private int _seed = -1; // Сид для генерации случайных чисел

        [SerializeField]
        private GameObject _blockPrefab; // Префаб блока

        [SerializeField]
        private GameObject _roadPrefab; // Префаб дороги

        [SerializeField]
        private Vector2 _physicalScale = new( 2, 2 ); // Физический масштаб

        private void Start()
        {
            // Создаем дерево районов
            var treeBuilder = new DistrictTreeGenerator();
            treeBuilder.BuildTree( _settings, _seed );

            // Получаем корневой узел дерева
            var root = treeBuilder.GetRoot();

            // Размещаем объекты на сцене
            PlaceObjectsOnNode( root, 0 );
        }

        private void PlaceObjectsOnNode(DistrictTreeGenerator.Node node, int depth)
        {
            if (node.Children[0] == null)
            {
                if (_blockPrefab)
                {
                    Vector3 center = transform.localToWorldMatrix * new Vector3( (node.Rect.xMin + node.Rect.width / 2f) * _physicalScale.x, 0,
                                                                                 (node.Rect.yMin + node.Rect.height / 2f) * _physicalScale.y );
                    var go = Instantiate( _blockPrefab, center, transform.rotation, transform );
                    Vector3 scale = go.transform.lossyScale;
                    scale.x *= node.Rect.width;
                    scale.z *= node.Rect.height;
                    go.transform.localScale = scale;
                }
            } else
            {
                if (_roadPrefab)
                {
                    var roadWidth = _settings.GetRoadWidth( depth );
                    if (node.SplitAxis == 0)
                    {
                        Vector3 center = transform.localToWorldMatrix * new Vector3( (node.Children[0].Rect.xMax + roadWidth / 2f) * _physicalScale.x, 0,
                                                                                     (node.Rect.yMin + node.Rect.height / 2f) * _physicalScale.y );
                        var go = Instantiate( _roadPrefab, center, transform.rotation, transform );
                        Vector3 scale = go.transform.lossyScale;
                        scale.x *= roadWidth;
                        scale.z *= node.Rect.height;
                        go.transform.localScale = scale;
                    } else
                    {
                        Vector3 center = transform.localToWorldMatrix * new Vector3( (node.Rect.xMin + node.Rect.width / 2f) * _physicalScale.x, 0,
                                                                                     (node.Children[0].Rect.yMax + roadWidth / 2f) * _physicalScale.y );
                        var go = Instantiate( _roadPrefab, center, transform.rotation, transform );
                        Vector3 scale = go.transform.lossyScale;
                        scale.x *= node.Rect.width;
                        scale.z *= roadWidth;
                        go.transform.localScale = scale;
                    }
                }

                PlaceObjectsOnNode( node.Children[0], depth + 1 );
                PlaceObjectsOnNode( node.Children[1], depth + 1 );
            }
        }
    }
}