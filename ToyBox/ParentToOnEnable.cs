namespace ToyBoxHHH
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Parent to object OnEnable. Leave null to set parent to null.
    /// Useful when you hack around with the hierarchy, could be extra useful in combination with <see cref="FakeParenting"/>
    /// Super useful when doing stuff with rigidbodies - rigidbody physics always more stable without parenting.
    /// 
    /// made by @horatiu665
    /// </summary>
    public class ParentToOnEnable : MonoBehaviour
    {
        public Transform targetParent;
        private void OnEnable()
        {
            transform.SetParent(targetParent);
        }
    }
}