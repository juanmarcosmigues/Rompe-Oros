using UnityEngine;

public static class EasingLibrary
{
    public enum EasingType { Linear, EaseIn, EaseOut }
    
    public static float GetEasing (float t, EasingType type)
    {
        switch (type)
        {
            case EasingType.Linear:
                return Linear(t);
            case EasingType.EaseIn:
                return EaseInExpo(t);
            case EasingType.EaseOut:
                return EaseOut(t);
        }

        return 0;
    }
    public static float Linear (float t)
    { return t; }
    public static float EaseInExpo(float t, float exponential = 5f)
    {
        return Mathf.Pow(t, exponential);
    }
    public static float EaseOut (float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
}
