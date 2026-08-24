
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static T GetRandom<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }
    public static T[] MoveSetDown<T>(this T[] array)
    {
        for (int i = array.Length-1; i > 0; i--)
        {
            array[i] = array[i - 1];
        }
        array[0] = default;
        return array;
    }
    public static T[] MoveSetUp<T>(this T[] array)
    {
        for (int i = 0; i < array.Length-1; i++)
        {
            array[i] = array[i + 1];
        }
        array[array.Length - 1] = default;
        return array;
    }
    public static T Get<T> (this T[] array, T target)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(target)) return array[i];
        }

        return default;
    }
    public static int GetNextIndex<T>(this T[] array, int currentIndex)
    {
        return currentIndex >= array.Length - 1 ? 0 : currentIndex + 1;
    }
    public static int GetPreviousIndex<T>(this T[] array, int currentIndex)
    {
        return 0 == currentIndex ? array.Length - 1 : currentIndex - 1;
    }
    public static T GetNext<T>(this T[] array, int currentIndex) =>
        array[array.GetNextIndex(currentIndex)];
    public static T GetPrevious<T>(this T[] array, int currentIndex) =>
        array[array.GetPreviousIndex(currentIndex)];

    public static void Shuffle<T>(this T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length); // UnityEngine.Random
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    public static void ForEach<T>(this T[] array, System.Action<T> action)
    {
        foreach (var t in array) action(t);
    }
    public static bool ContentEquals<T>(this T[] a, T[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
            if (!a[i].Equals(b[i]))
                return false;

        return true;
    }
}
