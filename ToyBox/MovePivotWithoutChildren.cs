using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class MovePivotWithoutChildren : MonoBehaviour
{
    public bool update = false;
    public Vector3 prevPos;

    private void Reset()
    {
        prevPos = transform.position;
    }

    void Update()
    {
        if (update)
            DoIt();

    }

    private void DoIt()
    {
        var delta = transform.position - prevPos;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).position -= delta;
        }

        prevPos = transform.position;

    }

}