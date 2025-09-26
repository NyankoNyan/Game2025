using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockSpawnLogic
{
    [Serializable]
    public class LogicBuildStage
    {
        public Vector2Int RoomCount = new Vector2Int(1, 1);
        public List<LogicBlockSettings> BlockVariants = new();
    }
}
