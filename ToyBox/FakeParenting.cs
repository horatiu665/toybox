namespace ToyBox
{
    using UnityEngine;
    using System.Collections;

    public class FakeParenting : MonoBehaviour
    {
        [Header("Move pos/rot to this target object")]
        public Transform fakeParent;

        public bool pos = true, rot = true;

        public bool update = true, fixedUpdate, lateUpdate;

        private void DoIt()
        {
            if (fakeParent == null)
            {
                return;
            }
            if (pos)
            {
                transform.position = fakeParent.position;
            }
            if (rot)
            {
                transform.rotation = fakeParent.rotation;
            }
        }

        void Update()
        {
            if (update)
            {
                DoIt();
            }
        }

        void FixedUpdate()
        {
            if (fixedUpdate)
            {
                DoIt();
            }
        }

        void LateUpdate()
        {
            if (lateUpdate)
            {
                DoIt();
            }
        }
    }
}