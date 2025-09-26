using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlockSpawnLogic
{
    [Serializable]
    public class LogicBlockGraphBuilder
    {
        private struct NodePlaceVariant
        {
            public float CummulativeWeight;
            public int ParentIndex;
            public LogicBlockSettings BlockSettings;
        }

        [SerializeField]
        private List<LogicBuildStage> _stages = new();

        [SerializeField]
        private bool _debugMessages;

        [SerializeField]
        private int _seed = -1;

        private List<LogicBlockNode> _allNodes = new();

        private Dictionary<string, int> _roomCountById = new();

        private int _currentSeed = 0;

        public LogicBlockNode[] BuildGraph()
        {
            return BuildAllStages().ToArray();
        }

        private void Clear()
        {
            _allNodes.Clear();
            _roomCountById.Clear();
        }

        private IEnumerable<LogicBlockNode> BuildAllStages()
        {
            if (_seed == -1)
                _currentSeed = Environment.TickCount;
            else
                _currentSeed = _seed;

            UnityEngine.Random.InitState(_currentSeed);

            for (int i = 0; i < _stages.Count; i++)
            {
                LogicBuildStage stage = _stages[i];
                if (_debugMessages) Debug.Log($"Building stage {i}");

                foreach (var node in BuildStage(stage))
                {
                    yield return node;
                }
            }
        }

        private IEnumerable<LogicBlockNode> BuildStage(LogicBuildStage stage)
        {
            List<NodePlaceVariant> availableVariants = new();
            float weightSum;

            int roomCount = stage.RoomCount.EvalRnd();
            for (int i = 0; i < roomCount; i++)
            {
                availableVariants.Clear();
                weightSum = 0;

                if (_allNodes.Count > 0)
                {
                    for (int j = 0; j < _allNodes.Count; j++)
                    {
                        var potentialParent = _allNodes[j];

                        foreach (var variant in stage.BlockVariants)
                        {
                            if (IsBlockAvailable(variant, potentialParent))
                            {
                                weightSum += variant.Weight;
                                availableVariants.Add(new()
                                {
                                    CummulativeWeight = weightSum,
                                    ParentIndex = j,
                                    BlockSettings = variant
                                });
                            }
                        }
                    }
                }
                else
                {
                    foreach (var variant in stage.BlockVariants)
                    {
                        if (IsBlockAvailable(variant, null))
                        {
                            weightSum += variant.Weight;
                            availableVariants.Add(new()
                            {
                                CummulativeWeight = weightSum,
                                ParentIndex = -1,
                                BlockSettings = variant
                            });
                        }
                    }
                }

                if (availableVariants.Count == 0)
                {
                    if (_debugMessages) Debug.LogWarning($"No available variants to place at room {i + 1} in stage with {roomCount} rooms.");
                    yield break;
                }

                var selectVariant = SearchDixotomia(availableVariants, UnityEngine.Random.Range(0, weightSum));

                LogicBlockNode newNode = new()
                {
                    Settings = selectVariant.BlockSettings,
                };

                if (selectVariant.ParentIndex >= 0)
                {
                    var parentNode = _allNodes[selectVariant.ParentIndex];
                    newNode.Depth = parentNode.Depth + 1;
                    parentNode.AddChild(newNode);
                }
                else
                {
                    newNode.Depth = 0;
                }

                _allNodes.Add(newNode);

                _roomCountById.TryAdd(newNode.Settings.Id, 0);
                _roomCountById[newNode.Settings.Id]++;

                if (_debugMessages) Debug.Log($"[{i + 1}/{roomCount}][Parent: ({selectVariant.ParentIndex})] Placed room ({newNode.Settings.Id}) at depth {newNode.Depth} (total of this type: {_roomCountById[newNode.Settings.Id]})");

                yield return newNode;
            }
        }

        private NodePlaceVariant SearchDixotomia(List<NodePlaceVariant> variants, float target)
        {
            int left = 0;
            int right = variants.Count - 1;

            while (left < right)
            {
                int mid = (left + right) / 2;
                if (variants[mid].CummulativeWeight < target)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid;
                }
            }

            return variants[left];
        }

        private bool IsBlockAvailable(LogicBlockSettings block, LogicBlockNode parent)
        {
            if (block.Weight <= 0)
            {
                return false;
            }

            if (block.MaxRoomsCount == 0)
            {
                return false;
            }
            else if (block.MaxRoomsCount > 0)
            {
                if (_roomCountById.TryGetValue(block.Id, out int currentCount) && currentCount >= block.MaxRoomsCount)
                {
                    return false;
                }
            }

            int currentDepth = (parent == null) ? 0 : (parent.Depth + 1);
            if (currentDepth < block.SpawnDepth.x || currentDepth > block.SpawnDepth.y)
            {
                return false;
            }

            if (parent != null)
            {
                if (parent.Children.Count >= parent.Settings.OutputRoomsCount.y)
                {
                    return false;
                }

                if (parent.Settings.CompatibleOutput.Length > 0 && !parent.Settings.CompatibleOutput.Contains(block.Id))
                {
                    return false;
                }

                if (block.CompatibleInput.Length > 0 && !block.CompatibleInput.Contains(parent.Settings.Id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}