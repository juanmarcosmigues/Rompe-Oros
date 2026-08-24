using UnityEngine;

public static class CameraExtensions
{
    public static Vector3 RotateTowardsCamera(this Camera cam, Vector2 value)
    {
        Vector3 axisX = Vector3.Cross(cam.transform.forward, Vector3.up);
        Vector3 axisZ = Vector3.Cross(axisX, Vector3.up);

        Vector3 outValue = axisZ * -value.y;
        outValue += axisX * -value.x;
        outValue.Normalize();

        return outValue;
    }
}
