using NN;
using System.Collections.Generic;
using Test;
using UnityEngine;

[RequireComponent( typeof( CityBlockGenerator ) )]
public class FullCityGenerator : MonoBehaviour
{
    [Header( "Generation Settings" )]
    [SerializeField]
    private int _seed;

    [Header( "City settings" )]
    [SerializeField]
    private CityGenerationSettings _cityLayerSettings;

    [SerializeField]
    private Vector2 _cityScale = new( 1000, 1000 );

    [SerializeField]
    private float _betweenDistrictsGap = 10f;

    [SerializeField]
    private GameObject _districtSidePrefab;

    [SerializeField]
    private GameObject _districtCornerPrefab;

    [Header( "District settings" )]
    [SerializeField]
    private CityDistrictGenerationSettings _districtLayerSettings;

    [SerializeField]
    private GameObject _roadPrefab;

    [SerializeField]
    private float _roadScale = .1f;

    [SerializeField]
    private Vector2 _districtUnitSize = new( 10, 10 );

    [Header( "Block settings" )]
    [SerializeField]
    private CityBlockGenerationSettings _blockLayerSettings;

    [SerializeField]
    private GameObject _floorPrefab;

    private Dictionary<Vector2Int, DistrictState> _districts = new();
    private CityBlockGenerator _blockGenerator;

    private void Awake()
    {
        _blockGenerator = GetComponent<CityBlockGenerator>();
    }

    private void Start()
    {
        GenerateAllLayers();
    }

    private void GenerateAllLayers()
    {
        CityZoneGenerator cityZoneGenerator = new();
        cityZoneGenerator.CreateZoneMap( _cityLayerSettings, _seed );
        var zoneMap = cityZoneGenerator.GetZoneMap();

        CreateAllDistricts( zoneMap );
    }

    private void CreateAllDistricts(byte[,] zoneMap)
    {
        for (int x = 0; x < _cityLayerSettings.ZoneSize.x; x++)
        {
            for (int y = 0; y < _cityLayerSettings.ZoneSize.y; y++)
            {
                byte cellValue = zoneMap[x, y];
                if (cellValue != 0 && cellValue != 255)
                {
                    Vector2Int coord = new Vector2Int( x, y );
                    Vector2Int size = new( cellValue >> 4, cellValue & 0xF );
                    Vector2 realSize = new(
                        (size.x - 1) * _betweenDistrictsGap + size.x * _cityScale.x,
                        (size.y - 1) * _betweenDistrictsGap + size.y * _cityScale.y );
                    Vector3 districtCorner = GetDistrictCorner( coord );
                    Vector3 position = transform.TransformPoint( districtCorner );
                    CreateDistrict( coord, size, realSize, position, transform.rotation );
                    PlaceDistrictSides( coord, size );
                }
            }
        }
    }

    private void CreateDistrict(Vector2Int coord, Vector2Int size, Vector2 realSize, Vector3 position, Quaternion rotation)
    {
        var settings = (CityDistrictGenerationSettings)_districtLayerSettings.Clone();
        Vector2 logs = new( Mathf.Log( size.x, 2 ), Mathf.Log( size.y, 2 ) );
        settings.MaxDepth += (int)(logs.x + logs.y);
        settings.ZoneSize = new( (int)(realSize.x / _districtUnitSize.x),
                                 (int)(realSize.y / _districtUnitSize.y) );
        settings.AxisRate = new( size.y, size.x );

        var districtGenerator = new DistrictTreeGenerator();
        int districtSeed = GetDistrictSeed( coord, _seed );
        districtGenerator.BuildTree( settings, districtSeed );
        var rootNode = districtGenerator.GetRoot();

        var rootTransform = new GameObject( $"District_{coord.x}_{coord.y}" );
        rootTransform.transform.SetPositionAndRotation( position, rotation );
        PlaceDistrictNode( rootNode, 0, rootTransform.transform, districtSeed );

        _districts.Add( coord, new DistrictState()
        {
            RootNode = rootNode,
        } );
    }

    private int GetDistrictSeed(Vector2Int coord, int baseSeed)
    {
        return baseSeed ^ coord.GetHashCode();
    }

    private void PlaceDistrictSides(Vector2Int coord, Vector2Int size)
    {
        // X side
        for (int x = 0; x < size.x; x++)
        {
            var corner = GetDistrictCorner( coord + new Vector2Int( x, size.y ) );
            SpawnElement( _districtSidePrefab, Quaternion.identity, corner );

            corner = GetDistrictCorner( coord + new Vector2Int( x + 1, size.y ) );
            SpawnElement( _districtCornerPrefab, Quaternion.identity, corner );

            if (coord.y == 0)
            {
                corner = GetDistrictCorner( coord + new Vector2Int( x, 0 ) );
                SpawnElement( _districtSidePrefab, Quaternion.identity, corner );

                corner = GetDistrictCorner( coord + new Vector2Int( x + 1, 0 ) );
                SpawnElement( _districtCornerPrefab, Quaternion.identity, corner );

                if (coord.x == 0)
                {
                    corner = GetDistrictCorner( coord );
                    SpawnElement( _districtCornerPrefab, Quaternion.identity, corner );
                }
            }
        }

        // Y side
        Quaternion rotation = Quaternion.Euler( 0, -90, 0 );
        for (int y = 0; y < size.y; y++)
        {
            var corner = GetDistrictCorner( coord + new Vector2Int( size.x, y ) );
            SpawnElement( _districtSidePrefab, rotation, corner );

            if (y != size.y - 1)
            {
                corner = GetDistrictCorner( coord + new Vector2Int( size.x, y + 1 ) );
                SpawnElement( _districtCornerPrefab, Quaternion.identity, corner );
            }

            if (coord.x == 0)
            {
                corner = GetDistrictCorner( coord + new Vector2Int( 0, y ) );
                SpawnElement( _districtSidePrefab, rotation, corner );

                if (y != size.y - 1)
                {
                    corner = GetDistrictCorner( coord + new Vector2Int( 0, y + 1 ) );
                    SpawnElement( _districtCornerPrefab, Quaternion.identity, corner );
                }
            }
        }

        void SpawnElement(GameObject prefab, Quaternion rotation, Vector3 corner)
        {
            var go = GameObject.Instantiate( prefab, transform );
            go.transform.SetLocalPositionAndRotation( corner, rotation );
        }
    }

    private Vector3 GetDistrictCorner(Vector2Int coord)
    {
        return new( coord.x * (_cityScale.x + _betweenDistrictsGap) + _betweenDistrictsGap / 2f, 0,
                    coord.y * (_cityScale.y + _betweenDistrictsGap) + _betweenDistrictsGap / 2f );
    }

    private void PlaceDistrictNode(DistrictTreeGenerator.Node node, int depth, Transform parent, int districtSeed)
    {
        if (node.Children[0] == null)
        {
            PlaceBlock( node, parent, districtSeed );
        } else
        {
            var roadWidth = _districtLayerSettings.GetRoadWidth( depth );
            if (node.SplitAxis == 0)
            {
                Vector3 pivot = parent.TransformPoint(
                    new Vector3( (node.Children[0].Rect.xMax + roadWidth / 2f) * _districtUnitSize.x + _betweenDistrictsGap / 2f, 0,
                                 (node.Rect.yMin + node.Rect.height / 2f) * _districtUnitSize.y + _betweenDistrictsGap / 2f ) );
                Vector3 scale = _roadPrefab.transform.localScale;
                scale.x *= roadWidth;
                scale.z *= node.Rect.height * _districtUnitSize.y * _roadScale;
                GPUInstancePools.Instance.AddInstance( _roadPrefab, pivot, transform.rotation, scale );
                //Instantiate( _roadPrefab, center, transform.rotation ).transform.localScale = scale;
            } else
            {
                Vector3 pivot = parent.TransformPoint(
                    new Vector3( (node.Rect.xMin + node.Rect.width / 2f) * _districtUnitSize.x + _betweenDistrictsGap / 2f, 0,
                                 (node.Children[0].Rect.yMax + roadWidth / 2f) * _districtUnitSize.y + _betweenDistrictsGap / 2f ) );
                Vector3 scale = _roadPrefab.transform.localScale;
                scale.x *= node.Rect.width * _roadScale * _districtUnitSize.x;
                scale.z *= roadWidth;
                GPUInstancePools.Instance.AddInstance( _roadPrefab, pivot, transform.rotation, scale );
                //Instantiate( _roadPrefab, center, transform.rotation ).transform.localScale = scale;
            }

            PlaceDistrictNode( node.Children[0], depth + 1, parent, districtSeed );
            PlaceDistrictNode( node.Children[1], depth + 1, parent, districtSeed );
        }
    }

    private void PlaceBlock(DistrictTreeGenerator.Node node, Transform parent, int districtSeed)
    {
        GameObject blockGO = new GameObject( $"Block_{node.Rect.xMin}_{node.Rect.yMin}" );
        blockGO.transform.parent = parent;
        blockGO.transform.SetLocalPositionAndRotation(
            new( node.Rect.center.x * _districtUnitSize.x + _betweenDistrictsGap / 2f, 0, 
                 node.Rect.center.y * _districtUnitSize.y + _betweenDistrictsGap / 2f ),
            parent.rotation );

        // Place floor
        Vector3 position = parent.localToWorldMatrix.MultiplyPoint( new(
            node.Rect.center.x * _districtUnitSize.x + _betweenDistrictsGap / 2f, 0,
            node.Rect.center.y * _districtUnitSize.y + _betweenDistrictsGap / 2f ) );
        GPUInstancePools.Instance.AddInstance( _floorPrefab, position, parent.rotation,
            new( node.Rect.size.x * _floorPrefab.transform.localScale.x * _districtUnitSize.x,
                _floorPrefab.transform.localScale.y,
                node.Rect.size.y * _floorPrefab.transform.localScale.z * _districtUnitSize.y ) );
        //var floorGO = Instantiate( _floorPrefab, blockGO.transform );
        //floorGO.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
        //floorGO.transform.localScale = new( node.Rect.size.x * floorGO.transform.localScale.x * _districtUnitSize.x,
        //                                    floorGO.transform.localScale.y,
        //                                    node.Rect.size.y * floorGO.transform.localScale.z * _districtUnitSize.y );

        // Place buildings
        var blockSettings = (CityBlockGenerationSettings)_blockLayerSettings.Clone();
        blockSettings.Size = node.Rect.size;
        blockSettings.LogicalUnitSize = _districtUnitSize;
        _blockGenerator.GenerateBuildings( GetBlockSeed( node.Rect.position, districtSeed ), blockSettings, blockGO.transform );
    }

    private int GetBlockSeed(Vector2Int coord, int baseSeed)
    {
        return baseSeed ^ coord.GetHashCode();
    }

    private class DistrictState
    {
        public DistrictTreeGenerator.Node RootNode;
    }
}