namespace ToyBoxHHH
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// When you wanna export a prefab from Unity but don't want the entire script hierarchy, it's useful to delete all the scripts on it first. This utility does just that.
    /// Slap it on a GameObject and click the button.
    /// 
    /// made by @horatiu665
    /// </summary>
    [ExecuteAlways]
    public class DestroyAllScripts : MonoBehaviour
    {
        public bool removeSelf = false;

        [DebugButton]
        void DestroyWithUndo()
        {
            DeleteRecursive(transform);
        }

        void DeleteRecursive(Transform parent)
        {
#if UNITY_EDITOR
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Destroy All Scripts @" + parent);
            var undoGroupIndex = Undo.GetCurrentGroup();

            foreach (var gg in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (gg != this)
                    Undo.DestroyObjectImmediate(gg);
                //DestroyImmediate(gg);
            }

            if (removeSelf)
            {
                Undo.DestroyObjectImmediate(this);
            }

            Undo.CollapseUndoOperations(undoGroupIndex);
#endif
        }

    }
}