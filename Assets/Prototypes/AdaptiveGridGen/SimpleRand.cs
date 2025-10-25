using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdaptiveGrid
{
    public static class SimpleRand
    {
        public static int Seed = 82589933;

        public static IEnumerable<int> Sequence(int minInclusive, int maxExclusive, int start)
        {
            int range = maxExclusive - minInclusive;
            if(range <= 0)
            {
                yield break;
            }

            if (start < minInclusive || start >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException();
            }
            int currentOffset = start - minInclusive;
            
            for (int i = range; i > 0; i--)
            {
                yield return currentOffset + minInclusive;
                currentOffset = (currentOffset + Seed) % range;
            }
        }

        public static IEnumerable<int> Sequence(int maxExclusive)
        {
            return Sequence(0, maxExclusive, UnityEngine.Random.Range(0, maxExclusive));
        }

        public static IEnumerable<int> Sequence(int minInclusive, int maxExclusive)
        {
            return Sequence(minInclusive, maxExclusive, UnityEngine.Random.Range(minInclusive, maxExclusive));
        }

        public static IEnumerable<Vector2Int> Sequence(Vector2Int sqrSize)
        {
            foreach(int i in Sequence(sqrSize.x * sqrSize.y))
            {
                yield return new Vector2Int(i % sqrSize.x, i / sqrSize.x);
            }
        }

        public static IEnumerable<Vector2Int> GrowableSquare(int growCount, Vector2Int offset, Vector2Int coreSize)
        {

            for(int layer = 0; layer < growCount; layer++)
            {
                Vector2Int currentCore = new Vector2Int(layer, layer) * 2 + coreSize;
                foreach(Vector2Int coord in Sequence(currentCore))
                {
                    yield return coord - new Vector2Int(layer, layer) + offset;
                }
            }
        }

        public static IEnumerable<Vector2Int> GrowableSquare(int growCount, Vector2Int offset)
        {
            return GrowableSquare(growCount, offset, Vector2Int.one);
        }
    }
}