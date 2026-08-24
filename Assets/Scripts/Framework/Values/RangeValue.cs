using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RangeValue
{
    public float min;
    public float max;

    public float GetRandomValue() => Random.Range(min, max);
    public float GetValue(float t) => Mathf.Lerp(min, max, t);
    public float GetDelta() => max - min;
    public float GetValueInverse(float value) => Mathf.InverseLerp(min, max, value);
}
