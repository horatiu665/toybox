using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DestroyAllScripts : MonoBehaviour
{
    public bool doIt;

    void Update()
    {
        if (doIt) {
            doIt = false;
            IrreversibleDelete();
        }
    }

    void IrreversibleDelete()
    {
        DeleteRecursive(transform);
    }

    void DeleteRecursive(Transform parent)
    {
        foreach (var gg in GetComponentsInChildren<MonoBehaviour>()) {
            if (gg != this)
                DestroyImmediate(gg);
        }
    }
}
