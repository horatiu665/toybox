namespace ToyBoxHHH
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// A super useful debug script for VR and mobile platforms, where Debug.DrawLine is nonexisting.
    /// 
    /// Usage: DrawLineInGame.DrawVisibleGizmo(position, direction, color) - this one stays forever but you can destroy it yourself. 
    ///     you also have the pooled option, or an auto-destroy with a delay
    ///     and you also have the option to pass a condition for not destroying it, such as when an error happens, or while holding some kind of debug key if you want to inspect something closely.
    ///     
    /// made by @horatiu665
    /// </summary>
    public class DrawLineInGame : MonoBehaviour
    {
        private const string TAG = "[DrawLineInGame] ";

        private static Dictionary<int, GameObject> pooledGizmos = new Dictionary<int, GameObject>();

        public float length => transform.localScale.z;
        public Collider colliderHit;


        /// <summary>
        /// Draw a gizmo line from pos, into direction dir, with default material with color color, for duration duration, and save a reference of a collider on a script on the debug obj if you want 
        ///     then you can select the debug object in the unity editor and check the collider reference manually for further debug.
        /// </summary>
        public static void DrawVisibleGizmo(Vector3 pos, Vector3 dir, Color color, float duration, System.Func<bool> doNotDestroyCondition = null, Collider colliderHit = null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = TAG + "gizmo";
            Destroy(g.GetComponent<Collider>());
            g.transform.position = pos + dir / 2;
            g.transform.LookAt(pos + dir);
            g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
            g.GetComponent<Renderer>().material.color = color;
            var dd = g.AddComponent<DrawLineInGame>();
            dd.StartCoroutine(dd.WaitAndDestroy_ButDontDestroyWhileCondition(duration, doNotDestroyCondition));
            dd.colliderHit = colliderHit;

        }

        // see above
        public static void DrawVisibleGizmo(Vector3 pos, Vector3 dir, Color color, float duration, Collider colliderHit = null)
        {
            DrawVisibleGizmo(pos, dir, color, duration, null, colliderHit);
        }

        /// <summary>
        /// Draw a gizmo line from pos, into direction dir, with default material with color color
        /// 
        /// Does not destroy the gizmo, but returns it for further handling by the requester
        /// </summary>
        public static GameObject DrawVisibleGizmo(Vector3 pos, Vector3 dir, Color color)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = TAG + "gizmo";
            Destroy(g.GetComponent<Collider>());
            g.transform.position = pos + dir / 2;
            g.transform.LookAt(pos + dir);
            g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
            g.GetComponent<Renderer>().material.color = color;
            return g;
        }

        /// <summary>
        /// see <see cref="DrawVisibleGizmo(Vector3, Vector3, Color)"/> but with custom sharedMaterial
        /// </summary>
        public static GameObject DrawVisibleGizmo(Vector3 pos, Vector3 dir, Material sharedMaterial)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = TAG + "gizmo";
            Destroy(g.GetComponent<Collider>());
            g.transform.position = pos + dir / 2;
            g.transform.LookAt(pos + dir);
            g.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
            g.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
            return g;
        }

        /// <summary>
        /// Does not even create gizmo, but instead updates pos/dir to draw a new line using an old gizmo.
        /// </summary>
        public static void DrawVisibleGizmo(GameObject existingGizmo, Vector3 pos, Vector3 dir)
        {
            existingGizmo.transform.position = pos + dir / 2;
            existingGizmo.transform.LookAt(pos + dir);
            existingGizmo.transform.localScale = new Vector3(0.01f, 0.01f, dir.magnitude);
        }

        public IEnumerator WaitAndDestroy_ButDontDestroyWhileCondition(float minDur, System.Func<bool> doNotDestroyCondition = null)
        {
            yield return new WaitForSeconds(minDur);

            while (doNotDestroyCondition())
            {
                yield return 0;
            }
            // the above condition is the equivalent of holding a key as long as you don't want the gizmo to be destroyed.
            //while (InputVR.GetPress(true, InputVR.ButtonMask.Touchpad) || InputVR.GetPress(false, InputVR.ButtonMask.Touchpad))
            //{
            //    yield return 0;
            //}
            Destroy(gameObject);
            yield break;
        }

        /// <summary>
        /// Draws a visible gizmo, by first creating and later reusing 
        ///     the same object using a simple poolId number. This way it is more efficient than creating/destroying gameobjects, and one does not need any fancy pooling library.
        /// </summary>
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
}