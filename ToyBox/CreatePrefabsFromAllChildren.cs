namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Have you ever not done something in Unity just because you dreaded the amount of work it might take, only to wish there was a tool that could quickly do it for you?
    /// If that thing was making a lot of prefabs at once, this tool is for you!
    /// 
    /// Create prefabs out of all children of a transform in the scene, or apply the changes made to those prefabs, with the click of a button.
    ///     It even handles unique names by suffixing some numbers, if you have multiple children you want to make prefabs out of, and they share the same name.
    /// Unity will work for a bit cause it's slow to update many prefabs at once, but while Unity does its compilation, you can watch a youtube funny cats compilation of your own!
    ///     Ah, game development, what a riot!
    /// 
    /// made by @horatiu665
    /// </summary>
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

                PrefabUtility.SaveAsPrefabAssetAndConnect(t.gameObject, "Assets/" + path + "/" + t.gameObject.name + ".prefab", InteractionMode.UserAction);

            }
        }

        // http://answers.unity3d.com/questions/172601/how-do-i-apply-prefab-in-script-.html
        [DebugButton]
        void ApplyAllChildrenPrefabs()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(t.gameObject);
                //var targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
                var pathOfPrefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
                PrefabUtility.SaveAsPrefabAssetAndConnect(instanceRoot, pathOfPrefab, InteractionMode.UserAction);
                //PrefabUtility.ReplacePrefab(
                //                 instanceRoot,
                //                 targetPrefab,
                //                 ReplacePrefabOptions.ConnectToPrefab
                //                 );
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
}