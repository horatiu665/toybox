namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    public class LookAtObj : MonoBehaviour
    {
        public Transform target;
        public bool lookAtMainCamera = false;
        public bool onlyYRotation = false;
        public bool onUpdate = true;
        public bool onLateUpdate = false;

        private Camera _cameraMain;
        public Camera cameraMain
        {
            get
            {
                if (_cameraMain == null || !_cameraMain.isActiveAndEnabled)
                {
                    _cameraMain = Camera.main;
                }
                return _cameraMain;
            }
        }

        [DebugButton]
        private void DoTheLookThing()
        {
            var t = target;

            // note: from Unity 2020.2 or so, Camera.main has been optimized. For now, the cached version is still safest.
            if (lookAtMainCamera)
                t = cameraMain.transform;

            if (t == null)
                return;

            transform.LookAt(t);
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
}
