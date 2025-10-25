using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockSpawnLogic
{
    public interface ILevelGraphStagesSettings
    {
        IEnumerable<LogicBuildStage> Stages { get; }
    }

    [Serializable]
    public class LevelStagesComposition : ILevelGraphStagesSettings
    {
        public List<LevelGraphStagesSettings> _stagesSettings = new();

        public IEnumerable<LogicBuildStage> Stages
        {
            get
            {
                foreach (var settings in _stagesSettings)
                {
                    foreach (var stage in settings.Stages)
                    {
                        yield return stage;
                    }
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "LevelGraphStagesSettings", menuName = "NN/Level Graph Stages Settings")]
    public class LevelGraphStagesSettings : ScriptableObject, ILevelGraphStagesSettings
    {
        [SerializeField]
        private List<LogicBuildStage> _stages = new();

        public IEnumerable<LogicBuildStage> Stages => _stages;
    }
}