using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RangeVector3
{
    public Vector3 min;
    public Vector3 max;

    private Vector3 _bufferVector3;

    public Vector3  GetRandomValue()
    {
        _bufferVector3.x = Mathf.Lerp(min.x, max.x, Random.Range(0, 1f));
        _bufferVector3.y = Mathf.Lerp(min.y, max.y, Random.Range(0, 1f));
        _bufferVector3.z = Mathf.Lerp(min.z, max.z, Random.Range(0, 1f));

        return _bufferVector3;
    }
    public Vector3 GetValue(float t) => Vector3.Lerp(min, max, t);
    public Vector3 GetDelta() => max - min;
}
