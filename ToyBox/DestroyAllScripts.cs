namespace ToyBox
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [ExecuteAlways]
    public class DestroyAllScripts : MonoBehaviour
    {
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

            foreach (var gg in GetComponentsInChildren<MonoBehaviour>())
            {
                if (gg != this)
                    Undo.DestroyObjectImmediate(gg);
                //DestroyImmediate(gg);
            }
            Undo.CollapseUndoOperations(undoGroupIndex);
#endif
        }

    }
}