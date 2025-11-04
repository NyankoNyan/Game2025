using System;
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

        [Serializable]
        public class GizmoInfo
        {
            public bool Initialized = false;
            public Vector3 From, To;
        }

        [HideInInspector]
        public GizmoInfo _gizmoInfo = new GizmoInfo();

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
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate( transform.GetChild( i ).gameObject );
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!_gizmoInfo.Initialized)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube( (_gizmoInfo.From + _gizmoInfo.To) / 2f, _gizmoInfo.To - _gizmoInfo.From );
            }
        }

#endif

        public void UpdateGizmo()
        {
            TextAsset configContent = Resources.Load<TextAsset>( _configName );
            ConfigFile configFile = YamlSerializer.Deserialize<ConfigFile>( Encoding.UTF8.GetBytes( configContent.text ), BuildingGenYamlFormatterResolver.Options );

            var building = configFile.Buildings.Find( b => b.Id == _buildingId );
            EvaluationContext context = new( new( null, configFile.Parameters ), building.Parameters );

            Vector3 from = Vector3.zero;
            Vector3 to = Vector3.zero;

            foreach (var section in configFile.Buildings.Find( b => b.Id == _buildingId ).Sections)
            {
                var sectionContext = new EvaluationContext( context, section.Parameters );
                Vector3Int gridSize = section.GenerationSettingsGrid.Size.AsVector3Int( sectionContext );
                Vector3 spacing = section.GenerationSettingsGrid.Spacing.AsVector3( sectionContext );
                Vector3 sectionSize = Vector3.Scale( gridSize, spacing );
                Vector3 offset = section.Position.AsVector3( sectionContext );
                from = Vector3.Min( from, offset - sectionSize / 2f );
                to = Vector3.Max( to, offset + sectionSize / 2f );
            }
            // Смена осей с blender-style на Unity-style
            _gizmoInfo.From = new Vector3( from.x, from.z, from.y );
            _gizmoInfo.To = new Vector3( to.x, to.z, to.y );
        }
    }
}