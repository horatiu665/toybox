namespace ToyBoxHHH
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = UnityEngine.Random;

    // inspired by http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
    // adjusted by @horatiu665
    // added min/max aspect ratio, and checks the delta of the resolution
    public class CameraAspectLocker : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;
        public new Camera camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }
                return _camera;
            }
        }

        [Header("Use math! 16 / 9 ~= 1.777")]
        public float minAspectRatio = 16f / 9;
        public float maxAspectRatio = 21f / 9;

        [Header("When")]
        public bool onUpdate = true;
        private Vector2 prevScreenRes;

        private void Start()
        {
            if (!onUpdate)
                ResetAspect();
        }

        private void Update()
        {
            if (onUpdate)
            {
                if (prevScreenRes.x != Screen.width || prevScreenRes.y != Screen.height)
                {
                    ResetAspect();
                }
            }
        }

        private void Reset()
        {
            if (camera != null)
            {
            }
        }

        void ResetAspect()
        {
            // set the desired aspect ratio
            float targetaspect = minAspectRatio;

            // determine the game window's current aspect ratio
            float windowaspect = (float)Screen.width / (float)Screen.height;

            // targetaspect is a clamped windowaspect between the min and max values
            targetaspect = Mathf.Clamp(windowaspect, minAspectRatio, maxAspectRatio);

            // current viewport height should be scaled by this amount
            float scaleheight = windowaspect / targetaspect;

            // if scaled height is less than current height, add letterbox
            if (scaleheight < 1.0f)
            {
                Rect rect = camera.rect;

                rect.width = 1.0f;
                rect.height = scaleheight;
                rect.x = 0;
                rect.y = (1.0f - scaleheight) / 2.0f;

                camera.rect = rect;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleheight;

                Rect rect = camera.rect;

                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;

                camera.rect = rect;
            }

            prevScreenRes = new Vector2(Screen.width, Screen.height);
        }
    }
}