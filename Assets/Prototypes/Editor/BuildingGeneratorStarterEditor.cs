using UnityEngine;
using UnityEditor;
using BuildingGen.Tools;

#if UNITY_EDITOR
namespace Test
{
    [CustomEditor(typeof(BuildingGeneratorStarter))]
    public class BuildingGeneratorStarterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var targetComponent = (BuildingGeneratorStarter)target;
            if (GUILayout.Button("Run Generation"))
            {
                targetComponent.RunGeneration();
            }

            if (GUILayout.Button("Destroy Building"))
            {
                targetComponent.DestroyBuilding();
            }
        }
    }
}
#endif
