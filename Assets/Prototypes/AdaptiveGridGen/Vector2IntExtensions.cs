using UnityEngine;

namespace BlockSpawnLogic
{
    public static class Vector2IntExtensions
    {
        public static int EvalRnd(this Vector2Int v1)
        {
            return UnityEngine.Random.Range(v1.x, v1.y);
        }
    }
}
