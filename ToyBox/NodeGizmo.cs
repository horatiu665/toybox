using UnityEngine;
using System.Collections;

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
