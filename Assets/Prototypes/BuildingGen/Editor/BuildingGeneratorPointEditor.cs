using BuildingGen.Components;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BuildingGen.Editor
{
    [CustomEditor( typeof( BuildingGeneratorPoint ) )]
    public class BuildingGeneratorPointEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var targetComponent = (BuildingGeneratorPoint)target;
            if (GUILayout.Button( "Generate" ))
            {
                targetComponent.GenerateBuilding();
            }

            if (GUILayout.Button( "Clear" ))
            {
                targetComponent.RemoveBuilding();
            }
        }
    }
}
#endif