using System;
using UnityEngine;

namespace BlockSpawnLogic
{
    [Serializable]
    public class LogicBlockSettings
    {
        public string Id = "";
        public float Weight = 1f;
        public int MaxRoomsCount = -1;
        public Vector2Int SpawnDepth = new Vector2Int(0, 100);
        public Vector2Int OutputRoomsCount = new Vector2Int(0, 2);
        public string[] CompatibleOutput = new string[0];
        public string[] CompatibleInput = new string[0];

        public GridBlockSettings GridSettings = new();
    }

    [Serializable]
    public class GridBlockSettings
    {
        public Vector2Int SizeX = new(5, 5);
        public Vector2Int SizeY = new(5, 5);
        public bool LockSize = false;
    }
}