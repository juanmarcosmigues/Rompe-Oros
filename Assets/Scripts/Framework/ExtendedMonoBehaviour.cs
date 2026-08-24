using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMonobehaviour : MonoBehaviour
{
    public virtual bool IsReady => lateStartDone;
    protected bool lateStartDone;
    protected bool readyStartDone;

    protected virtual void OnEnable ()
    {
        if (!lateStartDone || !readyStartDone)
            StartCoroutine(_LateStart());
    }
    protected virtual void Start()
    {
    }
    private IEnumerator _LateStart ()
    {
        if (!lateStartDone)
        {
            yield return null;
            LateStart();
            lateStartDone = true;
        }
        if (!readyStartDone)
        {
            while (!IsReady) yield return null;
            ReadyStart();
            readyStartDone = true;
        }
    }
    protected virtual void LateStart ()
    {

    }
    protected virtual void ReadyStart ()
    {

    }
    protected virtual void Update ()
    {
        if (IsReady) ReadyUpdate();
    }
    protected virtual void ReadyUpdate ()
    {

    }
    public Coroutine Loop(System.Action<float> action, float time) 
        => StartCoroutine(_Loop(action, time));
    protected virtual IEnumerator _Loop(System.Action<float> action, float time)
    {
        float t = 0f;
        while (t < time)
        {
            action(Mathf.Clamp01(t / time));
            yield return null;
            t += Time.deltaTime;
        }
    }

    public Coroutine LoopFixed(System.Action<float> action, float time) 
        => StartCoroutine(_LoopFixed(action, time));
    protected virtual IEnumerator _LoopFixed(System.Action<float> action, float time)
    {
        float t = 0f;
        while (t < time)
        {
            action(Mathf.Clamp01(t / time));
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }
    }

    public Coroutine Yield(float time, System.Action onStart, System.Action onEnd) 
        => StartCoroutine(_Yield(time, onStart, onEnd));
    protected virtual IEnumerator _Yield(float time, System.Action onStart, System.Action onEnd)
    {
        onStart?.Invoke();
        yield return new WaitForSeconds(time);
        onEnd();
    }
}
