using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TimescaleHack : MonoBehaviour
{
    public Vector2 sliderPos = new Vector2(10, 10);
    public Vector2 sliderSize = new Vector2(100, 20f);

    public float timeScale = 1f;
    public float snapTo1 = 0.03f;

    private void Update()
    {
        Time.timeScale = timeScale;

        // drag??? NAHHH ;)
    }

    private void OnGUI()
    {
        timeScale = GUI.HorizontalSlider(new Rect(sliderPos.x, sliderPos.y, sliderSize.x, sliderSize.y), timeScale, 0, 2f);
        if (Mathf.Abs(timeScale - 1f) < snapTo1)
        {
            timeScale = 1f;
        }

    }
}