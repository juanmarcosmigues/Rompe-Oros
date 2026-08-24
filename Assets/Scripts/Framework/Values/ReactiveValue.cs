using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveValue <T>
{
    public T value
    {
        get
        {
            return val;
        }
        set
        {
            val = value;
            React();
        }
    }
    private T val;

    public event System.Action<T> OnChange;

    private void React ()
    {
        OnChange.Invoke(val);
    }
}
