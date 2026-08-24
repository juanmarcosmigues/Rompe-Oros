using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeyItemPair <T>
{
    public static KeyItemPair<T> Empty => new KeyItemPair<T>("", default);
    public KeyItemPair(string key, T item)
    {
        this.key = key;
        this.item = item;
    }

    public string key;
    public T item;

    public static Dictionary<string, T> GetDictionary (params KeyItemPair<T>[] items)
    {
        Dictionary<string, T> dictionary = new Dictionary<string, T>();

        foreach (var item in items)
            dictionary.Add(item.key, item.item);

        return dictionary;
    }
}
