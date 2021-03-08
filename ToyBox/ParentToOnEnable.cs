using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentToOnEnable : MonoBehaviour
{
    public Transform targetParent;
    private void OnEnable()
    {
        transform.SetParent(targetParent);
    }
}
