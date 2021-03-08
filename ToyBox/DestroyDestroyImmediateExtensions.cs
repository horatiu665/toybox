using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DestroyDestroyImmediateExtensions
{
    public static void DestroySmart(this MonoBehaviour mb, UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            MonoBehaviour.Destroy(obj);
        }
#if UNITY_EDITOR

        else
        {
            MonoBehaviour.DestroyImmediate(obj);
        }
#endif
    }
}