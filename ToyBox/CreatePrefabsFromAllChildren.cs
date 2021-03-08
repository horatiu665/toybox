using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CreatePrefabsFromAllChildren : MonoBehaviour
{
    public string path;

#if UNITY_EDITOR
    [DebugButton]
    void CreatePrefabsFromChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);

            Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + path + "/" + t.gameObject.name + ".prefab");
            PrefabUtility.ReplacePrefab(t.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
    }

    // http://answers.unity3d.com/questions/172601/how-do-i-apply-prefab-in-script-.html
    [DebugButton]
    void ApplyAllChildrenPrefabs()
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            var instanceRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(t.gameObject);
            var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(instanceRoot);
            PrefabUtility.ReplacePrefab(
                             instanceRoot,
                             targetPrefab,
                             ReplacePrefabOptions.ConnectToPrefab
                             );
        }
    }

    [DebugButton]
    void SetUniqueNames()
    {
        List<string> names = new List<string>(transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            int counter = 1;
            if (names.Any(n => n.Contains(t.name) && int.TryParse(n.Substring(n.LastIndexOf(" ") + 1), out counter)))
            {
#pragma warning disable 0219
                var last = names.Last(n => n.Contains(t.name) && int.TryParse(n.Substring(n.LastIndexOf(" ") + 1), out counter));
#pragma warning restore 0219
                t.name = t.name + " " + (counter + 1);
            }
            else if (names.Any(n => n == t.name))
            {
                t.name = t.name + " 2";
            }
            names.Add(t.name);
        }
    }
#endif
}
