using System.Linq;
using UnityEngine;

namespace Test
{
    public class DistrictGraphCreator : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _zoneSize = new( 100, 100 );

        [SerializeField]
        private Vector2Int _axisRate = new( 1, 1 );

        [SerializeField]
        private Vector2Int _minimalBlockSize = new( 5, 5 );

        [SerializeField]
        private int _maxDepth = 5;

        [SerializeField]
        [Range( 1f, 10f )]
        private float _rateConvergence = 2f;

        [SerializeField]
        private int[] _depthRoadWidth = new int[] { 3, 2, 1 };

        [SerializeField]
        private int _seed = -1;

        [SerializeField]
        private GameObject _blockPrefab;

        [SerializeField]
        private GameObject _roadPrefab;

        [SerializeField]
        private Vector2 _physicalScale = new( 2, 2 );

        private Node _root;

        private class Node
        {
            public RectInt Rect;
            public readonly Node[] Children = new Node[2];
            public int SplitAxis;
        }

        private void Start()
        {
            PlaceDistrictBlocks();
        }

        private void PlaceDistrictBlocks()
        {
            if (_seed != -1)
                Random.InitState( _seed );

            _root = CreateSubnode( new( 0, 0, _zoneSize.x, _zoneSize.y ), 0, _axisRate );
            PlaceObjectsOnNode( _root, 0 );
        }

        private Node CreateSubnode(RectInt innerZone, int depth, Vector2Int currentRate)
        {
            Node node = new() { Rect = innerZone };
            if (depth >= _maxDepth)
                return node;
            int roadWidth = GetRoadWidth( depth );

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

            float probability2 = Mathf.Pow( rate2 / (float)rate1, _rateConvergence );
            bool splitByMax = probability2 / (probability2 + 1f) < Random.value;
            bool splitByX = splitByMax ? xMax : !xMax;

            if (splitByX)
            {
                if (innerZone.width >= _minimalBlockSize.x * 2 + roadWidth)
                {
                    int splitX1 = Random.Range( innerZone.xMin + _minimalBlockSize.x, innerZone.xMax - _minimalBlockSize.x - roadWidth + 1 );
                    var newRate = new Vector2Int( currentRate.x, currentRate.y + 1 );
                    node.Children[0] = CreateSubnode( new RectInt( innerZone.xMin, innerZone.yMin, splitX1 - innerZone.xMin, innerZone.height ), depth + 1, newRate );
                    node.Children[1] = CreateSubnode( new RectInt( splitX1 + roadWidth, innerZone.yMin, innerZone.xMax - splitX1 - roadWidth, innerZone.height ), depth + 1, newRate );
                    node.SplitAxis = 0;
                }
            } else
            {
                if (innerZone.height >= _minimalBlockSize.y * 2 + roadWidth)
                {
                    int splitY1 = Random.Range( innerZone.yMin + _minimalBlockSize.y, innerZone.yMax - _minimalBlockSize.y - roadWidth + 1 );
                    var newRate = new Vector2Int( currentRate.x + 1, currentRate.y );
                    node.Children[0] = CreateSubnode( new RectInt( innerZone.xMin, innerZone.yMin, innerZone.width, splitY1 - innerZone.yMin ), depth + 1, newRate );
                    node.Children[1] = CreateSubnode( new RectInt( innerZone.xMin, splitY1 + roadWidth, innerZone.width, innerZone.yMax - splitY1 - roadWidth ), depth + 1, newRate );
                    node.SplitAxis = 1;
                }
            }

            return node;
        }

        private int GetRoadWidth(int depth)
        {
            return (depth < _depthRoadWidth.Length) ? _depthRoadWidth[depth] : _depthRoadWidth.Last();
        }

        private void PlaceObjectsOnNode(Node node, int depth)
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
                    var roadWidth = GetRoadWidth( depth );
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