namespace ToyBoxHHH.TimescaleHack
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Makes a little slider using OnGUI() for changing the Time.timeScale. Useful for debugging various things.
    /// 
    /// made by @horatiu665
    /// </summary>
    public class TimescaleHack : MonoBehaviour
    {
        public Vector2 sliderPos = new Vector2(10, 10);
        public Vector2 sliderSize = new Vector2(100, 20f);

        public Vector2 timeScaleRange = new Vector2(0, 2);
        public float timeScale = 1f;
        public float snapTo1 = 0.03f;

        public bool hideInBuild = false;

        private void Update()
        {
            if (hideInBuild)
                if (!Application.isEditor)
                    return;

            Time.timeScale = timeScale;

        }

        private void OnGUI()
        {
            if (hideInBuild)
                if (!Application.isEditor)
                    return;

            timeScale = GUI.HorizontalSlider(new Rect(sliderPos.x, sliderPos.y, sliderSize.x, sliderSize.y), timeScale, timeScaleRange.x, timeScaleRange.y);
            if (Mathf.Abs(timeScale - 1f) < snapTo1)
            {
                timeScale = 1f;
            }

        }
    }
}