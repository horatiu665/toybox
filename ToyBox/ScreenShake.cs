namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake _instance;
        public static ScreenShake instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ScreenShake>();
                }
                return _instance;
            }
        }

        [Header("Put me on a camera/parent that can move")]
        public bool ok;

        Vector3 shakeDelta;

        void Awake()
        {
            if (instance == null)
                _instance = this;
            else
            {
                Debug.Log("More than one ScreenShake");
            }
        }

        public static void StartShaking(float duration, float intensity)
        {
            if (instance != null)
            {
                instance.StopCoroutine("ShakeCoroutine");
                instance.transform.position -= instance.shakeDelta;
                instance.StartCoroutine("ShakeCoroutine", new Vector2(duration, intensity));
            }
        }


        IEnumerator ShakeCoroutine(Vector2 durationIntensity)
        {
            var duration = durationIntensity.x;
            var startValue = durationIntensity.y;
            var endValue = 0;

            float start = Time.realtimeSinceStartup;
            float end = start + duration;

            shakeDelta = Vector3.zero;

            if (duration > 0)
            {
                float durationInv = 1f / duration;
                float startMulDurationInv = start / duration;

                for (float timer = Time.realtimeSinceStartup; timer < end; timer = Time.realtimeSinceStartup)
                {
                    var t = (Mathf.Lerp(startValue, endValue, timer * durationInv - startMulDurationInv));

                    transform.position -= shakeDelta;
                    shakeDelta = t * new Vector3(
                        Random.Range(-1, 1) * transform.lossyScale.x,
                        Random.Range(-1, 1) * transform.lossyScale.y,
                        0//Random.Range(-1, 1) * transform.lossyScale.z
                        );
                    transform.position += shakeDelta;

                    yield return 0;
                }
            }

            transform.position -= shakeDelta;
        }
    }
}