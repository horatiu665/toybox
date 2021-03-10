using UnityEngine;
using System;
using System.Collections;

public static class pTween
{
    // Further tweaked by HHH ~2015-2021

    // Peter's Tweening Library.
    // Written by Peter Bruun-Rasmussen (http://www.bipbipspil.dk).

    /*  Example:
     *  
     *    IEnumerator Sequence()
     *    {
     *        Vector3 p1 = new Vector3(0,0,0);
     *        Vector3 p2 = new Vector3(0,10,0);
     *    
     *        yield return StartCoroutine(pTween.To(2f, t => { transform.position = Vector3.Lerp(p1, p2, t); }));
     *    }
     */

    public static IEnumerator To(float duration, float startValue, float endValue, Action<float> callback)
    {
        float start = Time.time;
        float end = start + duration;
        float durationInv = 1f / duration;
        float startMulDurationInv = start / duration;

        for (float t = Time.time; t < end; t = Time.time) {
            callback(Mathf.Lerp(startValue, endValue, t * durationInv - startMulDurationInv));
            yield return 0;
        }
        callback(endValue);
    }

    public static IEnumerator FixedTo(float duration, float startValue, float endValue, Action<float> callback)
    {
        float start = Time.time;
        float end = start + duration;
        float durationInv = 1f / duration;
        float startMulDurationInv = start / duration;

        for (float t = Time.time; t < end; t += Time.fixedDeltaTime) {
            yield return new WaitForFixedUpdate();
            callback(Mathf.Lerp(startValue, endValue, t * durationInv - startMulDurationInv));

        }
        yield return new WaitForFixedUpdate();
        callback(endValue);
    }

    public static IEnumerator RealtimeTo(float duration, float startValue, float endValue, Action<float> callback)
    {
        float start = Time.realtimeSinceStartup;
        float end = start + duration;
        float durationInv = 1f / duration;
        float startMulDurationInv = start / duration;

        for (float t = Time.realtimeSinceStartup; t < end; t = Time.realtimeSinceStartup) {
            callback(Mathf.Lerp(startValue, endValue, t * durationInv - startMulDurationInv));
            yield return 0;
        }
        callback(endValue);
    }

    public static IEnumerator To(float duration, Action<float> callback)
    {
        return To(duration, 0f, 1f, callback);
    }

    public static IEnumerator Wait(float duration, Action callback)
    {
        float end = Time.time + duration;
        while (Time.time < end) {
            yield return 0;
        }
        callback();
    }

    public static IEnumerator WaitCondition(Func<bool> condition, Action callback)
    {
        while (!condition()) {
            yield return 0;
        }
        callback();
    }

    public static IEnumerator WaitFrames(int frames, Action callback)
    {
        for (int i = 0; i < frames; i++) {
            yield return 0;
        }
        callback();
    }

    public static IEnumerator WaitFixedFrames(int frames, Action callback)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        callback();
        // in case frames == 0
        yield break;
    }


    public static IEnumerator Then(this IEnumerator coroutine, IEnumerator after)
    {
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        while (after.MoveNext())
        {
            yield return after.Current;
        }
    }

    public static IEnumerator Then(this IEnumerator coroutine, Action after)
    {
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        after();
    }

    public static IEnumerator Then(this IEnumerator coroutine, float delay, Action after)
    {
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        yield return new WaitForSeconds(delay);

        after();
    }
}
