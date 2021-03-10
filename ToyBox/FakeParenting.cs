namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Pretend that you have a parent with a simple script that positions/rotates a transform relative to another transform, almost (but not quite) like it was parented there!
    /// Useful when the object hierarchy gets messy, or when you want to do something funky with one object and do not want it to be destroyed with another.
    /// 
    /// NOTE: the FakeParenting script might have a frame of delay, so if you chain a bunch of them you will notice the objects update kind of smoothly. 
    ///     You cannot mitigate that, unless you make sure the code runs in a particular order, inverse from the hierarchy order, it's basically too much hassle for this kind of tool.
    ///     
    /// made by @horatiu665
    /// </summary>
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