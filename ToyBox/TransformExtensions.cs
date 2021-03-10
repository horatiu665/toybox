namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    public static class TransformExtensions
    {
        /// <summary>
        /// returns a list of transform's children, but not their children
        /// WARNING: new List<Transform>() 
        /// </summary>
        /// <returns></returns>
        public static List<Transform> GetChildren(this Transform t)
        {
            List<Transform> childList = new List<Transform>();
            for (int i = 0; i < t.childCount; i++)
            {
                childList.Add(t.GetChild(i));
            }
            return childList;
        }

        public static void GetChildrenNonAlloc(this Transform t, List<Transform> children)
        {
            children.Clear();
            for (int i = 0; i < t.childCount; i++)
            {
                children.Add(t.GetChild(i));
            }
        }
    }
}