using System;
using System.Collections;
using UnityEngine;

public static class Coroutines
{
    public static IEnumerator WaitFrames (int frames, System.Action action)
    {
        for (int i = 0; i < frames; i++)
            yield return null;

        action();
    }
    public static IEnumerator Wait (float time, System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    public static IEnumerator Loop (float time, System.Action<float> action)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t += Time.deltaTime/time);
            action(t);
            yield return null;
        }
    }
    public static IEnumerator LoopFixedUpdate (float time, System.Action<float> action)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t += Time.fixedDeltaTime / time);
            action(t);
            yield return new WaitForFixedUpdate();
        }
    }
    public static IEnumerator Loop(float time, Func<float, IEnumerator> routine)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t += Time.deltaTime / time);
            yield return routine(t);
        }
    }
    public static IEnumerator WaitForAnimation (Animation animation, AnimationClip clip)
    {
        yield return null;
        while (animation.isPlaying &&  animation.clip == clip) yield return null;
    }
}
