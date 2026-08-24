using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static void SetPositionAndDirection(this Transform t, Vector3 position, Vector3 direction)
    {
        t.position = position;
        t.forward = direction;
    }
    public static Vector3 DeltaTo(this Transform t, Vector3 point)
    {
        return point - t.position;
    }
    public static Vector3 FlatDeltaTo(this Transform t, Vector3 point)
    {
        point.y = t.position.y;
        return point - t.position;
    }
    public static void RemoveParent(this Transform t) => t.SetParent(null);

    public static void LookZY(this Transform t, Transform target)
    {
        Vector3 delta = target.position - t.position;
        Vector3 projected = Vector3.ProjectOnPlane(delta, t.up);

        if (projected.sqrMagnitude > 0.0001f)
            t.rotation = Quaternion.LookRotation(projected, t.up);
    }
}
