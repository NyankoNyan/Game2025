using UnityEngine;

namespace BlockSpawnLogic
{
    public class LogicBlockGraphBuilderTester:MonoBehaviour
    {
        [SerializeField]
        LogicBlockGraphBuilder _logicBlockGraphBuilder;

        private void Start()
        {
            var graph = _logicBlockGraphBuilder.BuildGraph();
            Debug.Log($"Graph built with {graph.Length} nodes.");
        }
    }
}