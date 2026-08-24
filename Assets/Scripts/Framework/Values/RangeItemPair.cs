using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RangeItemPair <T>
{
    public static RangeItemPair<T> Empty => new RangeItemPair<T>(new RangeValue(), default);
    public RangeItemPair(RangeValue range, T item)
    {
        this.range = range;
        this.item = item;
    }

    public RangeValue range;
    public T item;
}
