using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Timestamp
{
    public float timeStamp;
    public float goalTime;

    public Timestamp (float goalTime)
    {
        this.timeStamp = 0f;
        this.goalTime = 0f;

        Set(goalTime);
    }
    public bool initialized => timeStamp > 0f;
    public float time
    {
        get { return Time.realtimeSinceStartup - timeStamp; }
    }
    public float remainingTime
    {
        get { return goalTime - time; }
    }
    public float remainingTimeNormalized
    {
        get { return remainingTime / goalTime; }
    }
    public float remainingTimeNormalizedClamped
    {
        get { return Mathf.Clamp01(remainingTimeNormalized); }
    }
    public void Set(float goalTime = 0.0f)
    {
        this.goalTime = goalTime;
        timeStamp = Time.realtimeSinceStartup;
    }
}