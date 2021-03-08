using UnityEngine;
using System.Collections;

public class ScaleBasedOnInitDistance : MonoBehaviour
{

    LookAtObj lookatObj;
    float initdist;

    Vector3 initScale;

    void Start()
    {
        initScale = transform.localScale;
        lookatObj = GetComponent<LookAtObj>();
        initdist = (lookatObj.target.position - transform.position).magnitude;

    }

    // Update is called once per frame
    void Update()
    {
        var curDist = (lookatObj.target.position - transform.position).magnitude;
        var scale = curDist / initdist;
        var ssss = initScale;
        ssss.z *= scale;
        transform.localScale = ssss;
    }
}
