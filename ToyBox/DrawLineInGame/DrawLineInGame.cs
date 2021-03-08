using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawLineInGame : MonoBehaviour
{
    private static Dictionary<int, GameObject> pooledGizmos = new Dictionary<int, GameObject>();

    public Collider colliderHit;

    public static void DrawVisibleGizmo(Vector3 pos, Vector3 dir, Color color, float duration, Collider colliderHit = null)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "[DebugInGame] gizmo";
        Destroy(g.GetComponent<Collider>());
        g.transform.position = pos + dir / 2;
        g.transform.LookAt(pos + dir);
        g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
        g.GetComponent<Renderer>().material.color = color;
        var dd = g.AddComponent<DrawLineInGame>();
        dd.StartCoroutine(dd.waitToDie(duration));
        dd.colliderHit = colliderHit;

    }

    /// <summary>
    /// does not destroy gizmo, but returns it for futher handling by requisitor. does not set material.
    /// </summary>
    public static GameObject DrawVisibleGizmo(Vector3 pos, Vector3 dir, Color color)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "[DebugInGame] gizmo";
        Destroy(g.GetComponent<Collider>());
        g.transform.position = pos + dir / 2;
        g.transform.LookAt(pos + dir);
        g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
        g.GetComponent<Renderer>().material.color = color;
        return g;
    }

    /// <summary>
    /// does not destroy gizmo, but returns it for futher handling by requisitor
    /// </summary>
    public static GameObject DrawVisibleGizmo(Vector3 pos, Vector3 dir, Material material)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "[DebugInGame] gizmo";
        Destroy(g.GetComponent<Collider>());
        g.transform.position = pos + dir / 2;
        g.transform.LookAt(pos + dir);
        g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
        g.GetComponent<Renderer>().sharedMaterial = material;
        return g;
    }

    /// <summary>
    /// Does not even create gizmo, but instead just draws a new line using an old gizmo.
    /// </summary>
    /// <param name="existingGizmo"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static void DrawVisibleGizmo(GameObject existingGizmo, Vector3 pos, Vector3 dir)
    {
        existingGizmo.transform.position = pos + dir / 2;
        existingGizmo.transform.LookAt(pos + dir);
        existingGizmo.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
    }

    public IEnumerator waitToDie(float minDur)
    {
        yield return new WaitForSeconds(minDur);
        //while (InputVR.GetPress(true, InputVR.ButtonMask.Touchpad) || InputVR.GetPress(false, InputVR.ButtonMask.Touchpad))
        //{
        //    yield return 0;
        //}
        Destroy(gameObject);
        yield break;
    }

    public static GameObject DrawVisibleGizmo(Color color = default(Color))
    {
        return DrawVisibleGizmo(Vector3.zero, Vector3.zero, color);
    }

    public float length;

    public static void DrawVisibleGizmoPooled(Vector3 pos, Vector3 dir, Color color, int poolId)
    {
        if (pooledGizmos.ContainsKey(poolId))
        {
            DrawVisibleGizmo(pooledGizmos[poolId], pos, dir);
        }
        else
        {
            pooledGizmos[poolId] = DrawVisibleGizmo(pos, dir, color);
        }
    }
}
