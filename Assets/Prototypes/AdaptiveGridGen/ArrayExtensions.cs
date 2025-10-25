using System;
using System.Collections.Generic;

namespace AdaptiveGrid
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T> AsRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array is null or empty");

            foreach (var i in SimpleRand.Sequence(array.Length))
            {
                yield return array[i];
            }
        }

        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array is null or empty");

            return array[UnityEngine.Random.Range(0, array.Length)];
        }
    }
}