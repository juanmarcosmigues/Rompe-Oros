using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tasker : MonoBehaviour
{
    public static Tasker instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = CreateInstance();
            }

            return _instance;
        }
    }
    private static Tasker _instance;
    public delegate bool Condition();

    public static Tasker CreateInstance ()
    {
        var dc = FindFirstObjectByType<Tasker>();
        if (dc != null)
            return dc;

        var go = new GameObject("Tasker", typeof(Tasker));
        DontDestroyOnLoad(go);
        dc = go.GetComponent<Tasker>();
        return dc;
    }

    public static void Clean ()
    {
        instance.StopAllCoroutines();
    }
    public static void StopTask (Coroutine task)
    {
        instance.StopCoroutine(task);
    }
    public static Coroutine RunCoroutine (IEnumerator coroutine)
    {
        return instance.StartCoroutine(coroutine);
    }
    public static Coroutine WaitFrames (int frames, System.Action action)
    {
        return RunCoroutine(instance._WaitFrames(frames, action));
    }
    public static Coroutine Delay (float delay, System.Action action) 
    {
        return RunCoroutine(instance._Delay(delay, action));
    }
    public static Coroutine RealTimeDelay (float delay, System.Action action)
    {
        return RunCoroutine(instance._RealTimeDelay(delay, action));
    }
    public static Coroutine While (Condition condition, System.Action update, System.Action onEnd)
    {
        return RunCoroutine(instance._While(condition, update, onEnd));
    }
    public static Coroutine Loop (float time, System.Action<float> update, System.Action onEnd = null)
    {
        return RunCoroutine(instance._Time(time, update, onEnd));
    }
    public static Coroutine FixedTimeLoop (float time, System.Action<float> update, System.Action onEnd = null)
    {
        return RunCoroutine(instance._FixedTime(time, update, onEnd));
    }
    public static Coroutine RealTimeLoop(float time, System.Action<float> update, System.Action onEnd = null)
    {
        return RunCoroutine(instance._Realtime(time, update, onEnd));
    }
    public static Coroutine Tick(float duration, float interval, System.Action<float> tick, System.Action onEnd = null)
    {
        return RunCoroutine(instance._Tick(duration, interval, tick, onEnd));
    }
    public static Coroutine PlayAnimation (Animator target, string animation, int layer, System.Action onEnd)
    {
        return RunCoroutine(instance._PlayAnimation(target, animation, layer, onEnd));
    }

    private IEnumerator _WaitFrames (int frames, System.Action action)
    {
        for (int i = 0; i < frames; i++) yield return null;
        action();
    }
    private IEnumerator _Delay (float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    private IEnumerator _RealTimeDelay(float delay, System.Action action)
    {
        yield return new WaitForSecondsRealtime(delay);
        action();
    }
    private IEnumerator _While(Condition condition, System.Action update, System.Action onEnd)
    {
        while (condition())
        {
            update();
            yield return null;
        }

        onEnd();
    }

    private IEnumerator _Time(float time, System.Action<float> update, System.Action onEnd)
    {
        float t = 0.0f;
        while (true)
        {
            update(t / time);

            yield return null;
            if (t >= time) break;

            t += UnityEngine.Time.deltaTime;
            t = Mathf.Clamp(t, 0, time);
        }

        if (onEnd != null)
            onEnd();
    }
    private IEnumerator _FixedTime(float time, System.Action<float> update, System.Action onEnd)
    {
        float t = 0.0f;
        while (true)
        {
            update(t / time);

            yield return new WaitForFixedUpdate();
            if (t >= time) break;

            t += UnityEngine.Time.fixedDeltaTime;
            t = Mathf.Clamp(t, 0, time);
        }

        if (onEnd != null)
            onEnd();
    }
    private IEnumerator _Realtime(float time, System.Action<float> update, System.Action onEnd)
    {
        float t = 0.0f;
        float startT = UnityEngine.Time.unscaledTime;
        while (true)
        {
            update(t / time);

            yield return null;
            if (t >= time) break;

            t = UnityEngine.Time.unscaledTime - startT;
            t = Mathf.Clamp(t, 0, time);
        }

        if (onEnd != null)
            onEnd();
    }
    private IEnumerator _Tick(float duration, float interval, System.Action<float> tick, System.Action onEnd)
    {
        float t = 0.0f;
        while (true)
        {
            tick(t / duration);

            yield return new WaitForSeconds(interval);
            if (t >= duration) break;

            t += interval;
            t = Mathf.Clamp(t, 0, duration);
        }

        if (onEnd != null)
            onEnd();
    }

    private IEnumerator _PlayAnimation (Animator target, string animationName, int layer, System.Action onEnd)
    {
        target.Play(animationName);

        // We have to give one frame for the animation to start
        // and the currentAnimatorState has the new animation
        yield return null;

        bool playingAnimation = true;
        while (playingAnimation)
        {
            playingAnimation = target.GetCurrentAnimatorStateInfo(layer).shortNameHash == Animator.StringToHash(animationName);
            playingAnimation &= target.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f;
            yield return null;
        }

        onEnd?.Invoke();
    }
}
