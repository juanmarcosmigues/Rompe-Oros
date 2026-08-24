using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameFreeze : MonoBehaviour
{
    public static FrameFreeze instance 
    { 
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("FrameFreeze").AddComponent<FrameFreeze>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }

    protected static FrameFreeze _instance;
    protected List<System.Action> actions = new List<System.Action>();

    protected float freezingTime;
    protected int freezingFrames;
    protected float originalFixedDeltaTime;
    protected bool freezing;

    public static void Freeze(float t, System.Action onEnd = null)
    {
        instance.freezingTime = t;
        if (onEnd != null)
        {
            instance.actions.Add(onEnd);
        }
    }

    private void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }
    private void Update()
    {
        if (freezingTime > 0f)
        {
            freezingTime -= Time.unscaledDeltaTime;
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
            freezing = true;
        }
        else if (freezing)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            freezing = false;

            for (int i = 0; i < actions.Count;)
            {
                actions[0].Invoke();
                actions.RemoveAt(0);
            }
        }
    }
}
