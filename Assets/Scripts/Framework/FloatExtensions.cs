using UnityEngine;

public static class FloatExtensions
{
    public static float Fraction(this float value)
    {
        return value - Mathf.Floor(value);
    }
}
