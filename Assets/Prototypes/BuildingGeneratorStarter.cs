using BuildingGen.Components;
using BuildingGen.Tools;
using NN;
using System.Text;
using UnityEngine;
using VYaml.Serialization;

namespace Test
{
    [RequireComponent( typeof( BuildingGeneratorObject ) )]
    public class BuildingGeneratorStarter : MonoBehaviour
    {
        [SerializeField]
        private string _configName = "";

        private BuildingGeneratorObject _buildingGenerator;
        private Transform _building;

        private void Awake()
        {
            _buildingGenerator = GetComponent<BuildingGeneratorObject>();
            _buildingGenerator.OnDefaultContextSetup = (parameters) =>
            {
                parameters.Add( "blockHealth", new Parameter<int>( 100 ) );
                parameters.Add( "physDamageMultiplier", new Parameter<float>( 1 ) );
            };
            _buildingGenerator.OnBlockInstantiate = (prefab) => ScenePools.Instance.Get( prefab );
            _buildingGenerator.OnBlockSetup = (go, evalCtx) =>
            {
                var gizmos = go.GetComponent<JointDebugDrawer>();
                if (!gizmos)
                {
                    gizmos = go.AddComponent<JointDebugDrawer>();
                }

                if (!evalCtx.GetContextParameter( Parameter.IsStatic ).ToBool())
                {
                    var db = go.GetComponent<Test.DestructibleBlock>();
                    if (!db)
                    {
                        db = go.AddComponent<Test.DestructibleBlock>();
                        db.Setup(
                            evalCtx.GetContextParameter( "blockHealth" ).ToInteger(),
                            evalCtx.GetContextParameter( "physDamageMultiplier" ).ToFloat()
                            );
                    }
                }
            };
            ScenePools.Instance.OnRestoreDefaults.AddListener( (args) =>
            {
                foreach (var db in args.GameObject.GetComponents<Test.DestructibleBlock>())
                {
                    Destroy( db );
                }
            } );
        }

        private void Start()
        {
            RunGeneration();
        }

        public void RunGeneration()
        {
            if (_building)
            {
                RemoveBuilding( _building );
            }
            TextAsset configContent = Resources.Load<TextAsset>( _configName );
            ConfigFile configFile = YamlSerializer.Deserialize<ConfigFile>( Encoding.UTF8.GetBytes( configContent.text ), BuildingGenYamlFormatterResolver.Options );
            _buildingGenerator.LoadConfig( _configName, configFile );
            _building = _buildingGenerator.GenerateBuilding( "b1" );
        }

        public void DestroyBuilding()
        {
            if (_building)
            {
                RemoveBuilding( _building );
            }
        }

        private void RemoveBuilding(Transform building)
        {
            var pool = ScenePools.Instance;
            for (int i = _building.childCount - 1; i >= 0; i--)
            {
                pool.Remove( _building.GetChild( i ).gameObject );
            }
            Destroy( building.gameObject );
        }
    }
}