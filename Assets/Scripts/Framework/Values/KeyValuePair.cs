using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeyValuePair <T>
{
    public static KeyValuePair<T> Empty => new KeyValuePair<T>("", default);
    public KeyValuePair(string key, T item)
    {
        this.key = key;
        this.value = item;
    }

    public string key;
    public T value;

    public static Dictionary<string, T> GetDictionary (params KeyValuePair<T>[] items)
    {
        Dictionary<string, T> dictionary = new Dictionary<string, T>();

        foreach (var item in items)
            dictionary.Add(item.key, item.value);

        return dictionary;
    }
}
