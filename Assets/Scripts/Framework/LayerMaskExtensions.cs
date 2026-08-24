using UnityEngine;

public static class LayerMaskExtensions
{
    public static bool ContainsLayer (this LayerMask lm, int layer)
    {
        return (lm & (1 << layer)) != 0;
    }
}
