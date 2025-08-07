using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    [Serializable]
    public class CityDistrictGenerationSettings : ICloneable
    {
        public Vector2Int ZoneSize = new( 100, 100 );
        public Vector2Int AxisRate = new( 1, 1 );
        public Vector2Int MinimalBlockSize = new( 5, 5 );
        public int MaxDepth = 5;

        [Range( 1f, 10f )]
        public float RateConvergence = 2f;

        public int[] DepthRoadWidth = new int[] { 3, 2, 1 };

        public object Clone()
        {
            return new CityDistrictGenerationSettings()
            {
                ZoneSize = ZoneSize,
                AxisRate = AxisRate,
                MinimalBlockSize = MinimalBlockSize,
                MaxDepth = MaxDepth,
                RateConvergence = RateConvergence,
                DepthRoadWidth = DepthRoadWidth
            };
        }

        /// <summary>
        /// Получить ширину дороги для заданной глубины.
        /// </summary>
        /// <param name="depth">Глубина узла.</param>
        /// <returns>Ширина дороги.</returns>
        public int GetRoadWidth(int depth)
        {
            return (depth < DepthRoadWidth.Length) ? DepthRoadWidth[depth] : DepthRoadWidth[^1];
        }
    }

    /// <summary>
    /// Класс для построения логического дерева районов города.
    /// </summary>
    public class DistrictTreeGenerator
    {
        private Node _root;

        /// <summary>
        /// Построить логическое дерево районов города.
        /// </summary>
        /// <param name="settings">Настройки генерации районов.</param>
        /// <param name="seed">Сид для генерации случайных чисел.</param>
        public void BuildTree(CityDistrictGenerationSettings settings, int seed)
        {
            if (seed != -1)
                Random.InitState( seed );

            _root = CreateSubnode( new RectInt( 0, 0, settings.ZoneSize.x, settings.ZoneSize.y ), 0, settings.AxisRate, settings );
        }

        /// <summary>
        /// Получить корневой узел дерева.
        /// </summary>
        /// <returns>Корневой узел дерева.</returns>
        public Node GetRoot()
        {
            return _root;
        }

        private Node CreateSubnode(RectInt innerZone, int depth, Vector2Int currentRate, CityDistrictGenerationSettings settings)
        {
            Node node = new() { Rect = innerZone };
            if (depth >= settings.MaxDepth)
                return node;

            int roadWidth = settings.GetRoadWidth( depth );

            bool xMax = currentRate.x > currentRate.y;
            float rate1, rate2;
            if (xMax)
            {
                rate1 = currentRate.x;
                rate2 = currentRate.y;
            } else
            {
                rate1 = currentRate.y;
                rate2 = currentRate.x;
            }

            float probability2 = Mathf.Pow( rate2 / rate1, settings.RateConvergence );
            bool splitByMax = probability2 / (probability2 + 1f) < Random.value;
            bool splitByX = splitByMax ? xMax : !xMax;

            if (splitByX)
            {
                if (innerZone.width >= settings.MinimalBlockSize.x * 2 + roadWidth)
                {
                    int splitX1 = Random.Range( innerZone.xMin + settings.MinimalBlockSize.x, innerZone.xMax - settings.MinimalBlockSize.x - roadWidth + 1 );
                    var newRate = new Vector2Int( currentRate.x, currentRate.y + 1 );
                    node.Children[0] = CreateSubnode( new RectInt( innerZone.xMin, innerZone.yMin, splitX1 - innerZone.xMin, innerZone.height ), depth + 1, newRate, settings );
                    node.Children[1] = CreateSubnode( new RectInt( splitX1 + roadWidth, innerZone.yMin, innerZone.xMax - splitX1 - roadWidth, innerZone.height ), depth + 1, newRate, settings );
                    node.SplitAxis = 0;
                }
            } else
            {
                if (innerZone.height >= settings.MinimalBlockSize.y * 2 + roadWidth)
                {
                    int splitY1 = Random.Range( innerZone.yMin + settings.MinimalBlockSize.y, innerZone.yMax - settings.MinimalBlockSize.y - roadWidth + 1 );
                    var newRate = new Vector2Int( currentRate.x + 1, currentRate.y );
                    node.Children[0] = CreateSubnode( new RectInt( innerZone.xMin, innerZone.yMin, innerZone.width, splitY1 - innerZone.yMin ), depth + 1, newRate, settings );
                    node.Children[1] = CreateSubnode( new RectInt( innerZone.xMin, splitY1 + roadWidth, innerZone.width, innerZone.yMax - splitY1 - roadWidth ), depth + 1, newRate, settings );
                    node.SplitAxis = 1;
                }
            }

            return node;
        }

        /// <summary>
        /// Узел дерева районов.
        /// </summary>
        public class Node
        {
            public RectInt Rect;
            public readonly Node[] Children = new Node[2];
            public int SplitAxis;
        }
    }
}