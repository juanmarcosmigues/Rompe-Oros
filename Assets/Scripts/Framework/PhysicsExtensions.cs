using UnityEngine;

public static class PhysicsExtensions
{
    /// <summary>
    /// Made with AI:
    /// Calculates the exact world-space AABB of this BoxCollider,
    /// independent of Unity's internal physics bounds.
    /// </summary>
    public static Bounds CalculateWorldAABB(this BoxCollider box)
    {
        Transform t = box.transform;

        // World-space center
        Vector3 center = t.TransformPoint(box.center);

        Vector3 halfSize = box.size * 0.5f;

        Vector3 right = t.right * halfSize.x * t.lossyScale.x;
        Vector3 up = t.up * halfSize.y * t.lossyScale.y;
        Vector3 forward = t.forward * halfSize.z * t.lossyScale.z;

        Vector3 extents = new Vector3(
            Mathf.Abs(right.x) + Mathf.Abs(up.x) + Mathf.Abs(forward.x),
            Mathf.Abs(right.y) + Mathf.Abs(up.y) + Mathf.Abs(forward.y),
            Mathf.Abs(right.z) + Mathf.Abs(up.z) + Mathf.Abs(forward.z)
        );

        return new Bounds(center, extents * 2f);
    }
    public static Bounds CalculateWorldAABB(this SphereCollider sphere)
    {
        Transform t = sphere.transform;

        // World-space center
        Vector3 center = t.TransformPoint(sphere.center);

        Vector3 halfSize = sphere.radius * Vector3.one;

        Vector3 right = t.right * halfSize.x * t.lossyScale.x;
        Vector3 up = t.up * halfSize.y * t.lossyScale.y;
        Vector3 forward = t.forward * halfSize.z * t.lossyScale.z;

        Vector3 extents = new Vector3(
            Mathf.Abs(right.x) + Mathf.Abs(up.x) + Mathf.Abs(forward.x),
            Mathf.Abs(right.y) + Mathf.Abs(up.y) + Mathf.Abs(forward.y),
            Mathf.Abs(right.z) + Mathf.Abs(up.z) + Mathf.Abs(forward.z)
        );

        return new Bounds(center, extents * 2f);
    }
}