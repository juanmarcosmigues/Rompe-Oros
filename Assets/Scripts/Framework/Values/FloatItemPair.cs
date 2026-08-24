using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatItemPair <T>
{
    public static FloatItemPair<T> Empty => new FloatItemPair<T>(0f, default);
    public FloatItemPair(float val, T item)
    {
        this.val = val;
        this.item = item;
    }

    public float val;
    public T item;
}
