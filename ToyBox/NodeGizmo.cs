namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Draws a little colorful sphere gizmo. Useful when doing stuff like procedural generation, AI nodes, whatever uses objects that need to be visible in the scene view.
    /// 
    /// made by @horatiu665
    /// </summary>
    public class NodeGizmo : MonoBehaviour
    {

        public float size = 0.2f;
        public Color gizmosColor = new Color(1, 1, 1, 1);
        public bool wire = false;

        void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            if (wire)
            {
                Gizmos.DrawWireSphere(transform.position, size);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, size);
            }
        }
    }
}