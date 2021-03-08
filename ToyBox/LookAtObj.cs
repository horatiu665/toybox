using UnityEngine;
using System.Collections;

public class LookAtObj : MonoBehaviour {

	public Transform target;
    public bool onlyYRotation = false;
    public bool lateUpdate = false;
	void Update()
	{
        if (!lateUpdate) {
            transform.LookAt(target);
            if (onlyYRotation) {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
        }
	}

    void LateUpdate()
    {
        if (lateUpdate) {
            transform.LookAt(target);
            if (onlyYRotation) {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
        }
    }

}
