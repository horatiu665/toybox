namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    public class FlyCamExtended : MonoBehaviour
    {

        /*
        FURTHER EXTENDED BY HHH / ~2015-2021

        EXTENDED FLYCAM
            Desi Quintans (CowfaceGames.com), 17 August 2012.
            Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.

        LICENSE
            Free as in speech, and free as in beer.

        FEATURES
            WASD/Arrows:    Movement
                      Q:    Climb
                      E:    Drop
                          Shift:    Move faster
                        Control:    Move slower
                            End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
        */

        public float cameraSensitivity = 300;
        public bool invertMouseY = true;
        public float climbMultiplier = -1f;
        [Header("accel 0 = instantly")]
        [Range(0, 1f)]
        public float acceleration = 0;
        public float normalMoveSpeed = 10;
        public float slowMoveFactor = 0.25f;
        public float fastMoveFactor = 3;

        public bool altDisablesCameraRotation = false;

        public bool lockCursor = true;

        private float rotationX = 0.0f;
        private float rotationY = 0.0f;

        Vector3 curMoveSpeed;

        void OnEnable()
        {
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;

            rotationY = transform.eulerAngles.x - (transform.eulerAngles.x > 90 ? 360 : 0);
            rotationX = transform.eulerAngles.y;
        }

        void Update()
        {
            // rotation
            var altPressed = Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (!(altDisablesCameraRotation && altPressed))
            {
                rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
                rotationY += (invertMouseY ? -1 : 1) * Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, -90, 90);
                transform.eulerAngles = new Vector3(rotationY, rotationX, transform.eulerAngles.z);
            }

            // position
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");
            float u = ((Input.GetKey(KeyCode.Q) ? 1f : 0f) + (Input.GetKey(KeyCode.E) ? -1f : 0f)) * climbMultiplier;

            // target speed / modifiers
            var targetSpeed = (v != 0 || h != 0 || u != 0) ? normalMoveSpeed : 0f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                targetSpeed *= fastMoveFactor;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                targetSpeed *= slowMoveFactor;
            }

            // accel
            if (acceleration == 0)
            {
                curMoveSpeed = targetSpeed * new Vector3(v, h, u);
            }
            else
            {
                curMoveSpeed = Vector3.Lerp(curMoveSpeed, targetSpeed * new Vector3(v, h, u), acceleration);
            }

            transform.position += transform.forward * curMoveSpeed.x * Time.deltaTime;
            transform.position += transform.right * curMoveSpeed.y * Time.deltaTime;
            transform.position += transform.up * curMoveSpeed.z * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                lockCursor = !lockCursor;
                Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !lockCursor;

            }
        }
    }
}
