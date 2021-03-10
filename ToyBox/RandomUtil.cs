namespace ToyBoxHHH
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class RandomUtil
    {
        /// <summary>
        /// Returns a random element from the list
        /// </summary>
        public static T Random<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random value between x and y, inclusive.
        /// </summary>
        public static float Random(this Vector2 vec)
        {
            return UnityEngine.Random.Range(vec.x, vec.y);
        }
    }
}