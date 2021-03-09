using UnityEngine;
using System.Collections;

public class LookAtObj : MonoBehaviour
{
    public Transform target;
    public bool onlyYRotation = false;
    public bool onUpdate = true;
    public bool onLateUpdate = false;

    [DebugButton]
    private void DoTheLookThing()
    {
        transform.LookAt(target);
        if (onlyYRotation)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    void Update()
    {
        if (onUpdate)
        {
            DoTheLookThing();
        }
    }

    void LateUpdate()
    {
        if (onLateUpdate)
        {
            DoTheLookThing();
        }
    }

}
