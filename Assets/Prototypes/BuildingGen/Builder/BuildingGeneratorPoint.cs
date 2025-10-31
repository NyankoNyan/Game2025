using System.Text;
using UnityEngine;
using VYaml.Serialization;

namespace BuildingGen.Components
{
    /// <summary>
    /// Точка генерации здания в пространстве сцены.
    /// </summary>
    public class BuildingGeneratorPoint : MonoBehaviour
    {
        [SerializeField]
        private string _configName = "";

        [SerializeField]
        private string _buildingId = "";

        public void GenerateBuilding()
        {
            RemoveBuilding();
            var _buildingGenerator = new BuildingGenerator();

            TextAsset configContent = Resources.Load<TextAsset>( _configName );
            ConfigFile configFile = YamlSerializer.Deserialize<ConfigFile>( Encoding.UTF8.GetBytes( configContent.text ), BuildingGenYamlFormatterResolver.Options );

            _buildingGenerator.LoadConfig( _configName, configFile );
            if (string.IsNullOrWhiteSpace( _buildingId ))
            {
                _buildingId = configFile.Buildings[0].Id;
            }

            var _building = _buildingGenerator.GenerateBuilding( _buildingId );

            _building.parent = transform;
            _building.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
        }

        public void RemoveBuilding()
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate( transform.GetChild( i ).gameObject );
            }
        }
    }
}