namespace ToyBoxHHH
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Flashes a color and fades it out. 
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        private static ScreenFlash _instance;
        public static ScreenFlash instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ScreenFlash>();
                }
                return _instance;
            }
        }

        [Header("Put this on the camera")]
        public bool ok;

        Texture2D solidColor;
        Color color = Color.white;
        float flashTimer = 0;
        float flashTimerDuration = 1;

        void Awake()
        {
            solidColor = new Texture2D(1, 1);
            SetTextureColor(color, 1);
        }

        void SetTextureColor(Color color, float alpha)
        {
            color.a = alpha;
            solidColor.SetPixel(0, 0, color);
            solidColor.Apply();

        }

        public static void Flash(float duration, Color color)
        {
            instance.color = color;
            instance.flashTimerDuration = duration;
            instance.flashTimer = Time.time + duration;

        }

        void OnGUI()
        {
            if (flashTimer > Time.time)
            {
                SetTextureColor(color, (flashTimer - Time.time) / flashTimerDuration);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), solidColor);

            }
        }

    }
}