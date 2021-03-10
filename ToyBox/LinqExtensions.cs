namespace ToyBoxHHH
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class LinqExtensions
    {
        /// <summary>
        /// Returns the transform from the list which is closest to the position specified. uses sqrMagnitude and Aggregate(). returns null if not found.
        /// </summary>
        /// <param name="transformList">list of transforms to choose from</param>
        /// <param name="position">position to compare distance with</param>
        /// <returns>the closest transform to the position, from the list</returns>
        public static Transform ClosestTransform(this IEnumerable<Transform> transformList, Vector3 position)
        {
            if (transformList.Any())
            {
                return transformList.Aggregate((t1, t2) => (t1.position - position).sqrMagnitude > (t2.position - position).sqrMagnitude ? t2 : t1);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the collider from the list which is closest to the position specified. uses sqrMagnitude and Aggregate(). returns null if not found.
        /// </summary>
        /// <param name="colliderList">list to choose from</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Collider ClosestCollider(this IEnumerable<Collider> colliderList, Vector3 position)
        {
            if (colliderList.Any())
            {
                return colliderList.Aggregate((t1, t2) => (t1.transform.position - position).sqrMagnitude > (t2.transform.position - position).sqrMagnitude ? t2 : t1);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the collider from the list which is closest to the position specified. uses Aggregate() and collider bounds. returns null if not found.
        /// </summary>
        /// <param name="colliderList">list to choose from</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Collider ClosestColliderBounds(this IEnumerable<Collider> colliderList, Vector3 position)
        {
            if (colliderList.Any())
            {
                return colliderList.Aggregate((c1, c2) => (c1.ClosestPointOnBounds(position) - position).sqrMagnitude > (c2.ClosestPointOnBounds(position) - position).sqrMagnitude ? c2 : c1);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// aggregate that does not care about nullable types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T AggregateSmart<T>(this IEnumerable<T> list, System.Func<T, T, T> func)
        {
            if (list == null || !list.Any())
                return default(T);
            else
            {
                return list.Aggregate<T>(func);
            }
        }

        /// <summary>
        /// Returns the raycasthit object from the list, which is closest to the position specified. uses sqrMagnitude and Aggregate(). returns a new RaycastHit() if empty list.
        /// </summary>
        /// <param name="raycastHitList">list to choose from</param>
        /// <param name="position">close to this pos</param>
        /// <returns></returns>
        public static RaycastHit ClosestRaycastHit(this IEnumerable<RaycastHit> raycastHitList, Vector3 position)
        {
            bool a;
            return ClosestRaycastHit(raycastHitList, position, out a);
        }

        /// <summary>
        /// Returns the raycasthit object from the list, which is closest to the position specified. uses sqrMagnitude and Aggregate(). returns a new RaycastHit() if empty list. Also returns true/false if there were any objects
        /// </summary>
        /// <param name="raycastHitList">list to choose from</param>
        /// <param name="position">close to this pos</param>
        /// <returns></returns>
        public static RaycastHit ClosestRaycastHit(this IEnumerable<RaycastHit> raycastHitList, Vector3 position, out bool success)
        {
            if (raycastHitList.Any())
            {
                success = true;
                return raycastHitList.Aggregate((r1, r2) => (r1.point - position).sqrMagnitude > (r2.point - position).sqrMagnitude ? r2 : r1);
            }
            else
            {
                success = false;
                return new RaycastHit();
            }
        }

        /// <summary>
        /// Returns the index of the max element of the sequence. stolen from https://stackoverflow.com/questions/462699/how-do-i-get-the-index-of-the-highest-value-in-an-array-using-linq
        /// </summary>
        /// <typeparam name="T">comparable element</typeparam>
        /// <param name="sequence"></param>
        /// <returns>index, or -1 if not found</returns>
        public static int MaxIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int maxIndex = -1;
            T maxValue = default(T); // Immediately overwritten anyway

            int index = 0;
            foreach (T value in sequence)
            {
                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }
    }
}
