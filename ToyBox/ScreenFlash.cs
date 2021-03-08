using System.Collections;
using UnityEngine;

public class ScreenFlash : MonoBehaviour
{
    [Header("Put this on the camera")]

    static ScreenFlash instance;
    Texture2D solidColor;
    Color color = Color.white;
    float flashTimer = 0;
    float flashTimerDuration = 1;

    void Start()
    {
        instance = this;
        solidColor = new Texture2D(1, 1);
        SetTextureColor(color, 1);

    }

    // Use this for initialization
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
