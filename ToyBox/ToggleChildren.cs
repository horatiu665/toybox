namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Useful script to toggle children using gameObject.SetActive()
    /// 
    /// made by @horatiu665
    /// </summary>
    public class ToggleChildren : MonoBehaviour
    {
        private List<Transform> childrenCache = new List<Transform>();

        [SerializeField]
        private Transform whichOne;

        [DebugButton]
        public void ToggleThisOne(Transform whichOne)
        {
            transform.GetChildrenNonAlloc(childrenCache);
            var indexOfActive = childrenCache.IndexOf(whichOne);
            childrenCache.ForEach(t => t.gameObject.SetActive(false));
            if (indexOfActive >= 0 && indexOfActive < transform.childCount)
                transform.GetChild(indexOfActive).gameObject.SetActive(true);
        }

        [DebugButton]
        public void ToggleNext()
        {
            transform.GetChildrenNonAlloc(childrenCache);
            var indexOfActive = childrenCache.FindIndex(t => t.gameObject.activeInHierarchy);
            indexOfActive++;
            if (indexOfActive == transform.childCount) indexOfActive = 0;
            childrenCache.ForEach(t => t.gameObject.SetActive(false));
            transform.GetChild(indexOfActive).gameObject.SetActive(true);
            whichOne = transform.GetChild(indexOfActive);
        }

        [DebugButton]
        public void ToggleRandom()
        {
            transform.GetChildrenNonAlloc(childrenCache);
            childrenCache.ForEach(t => t.gameObject.SetActive(false));
            var indexOfActive = Random.Range(0, transform.childCount);
            transform.GetChild(indexOfActive).gameObject.SetActive(true);
            whichOne = transform.GetChild(indexOfActive);
        }

    }
}