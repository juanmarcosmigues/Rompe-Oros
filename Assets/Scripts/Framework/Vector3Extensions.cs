using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static float SqrDistance(this Vector3 v1, Vector3 v2) => (v1 - v2).sqrMagnitude;
    public static Vector3 RandomPointOnXZCircle (this Vector3 point, float radius)
    {
        return point + Random.insideUnitSphere.FlattenY() * radius;
    }
    public static Vector3 FlattenY(this Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
    public static Vector3 xy2xz (this Vector2 vector)
    {
        return new Vector3(vector.x, 0f, vector.y);
    }
    public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return line_start + line_direction * project_length;
    }

    // For infinite lines:
    public static Vector3 GetClosestPointOnInfiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        return line_start + Vector3.Project(point - line_start, line_end - line_start);
    }
    public static Vector3 GetClosestPointOnLine(Vector3 linePoint, Vector3 lineDir, Vector3 point)
    {
        float t = Vector3.Dot(point - linePoint, lineDir) / lineDir.sqrMagnitude;
        return linePoint + lineDir * t;
    }

    public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, (Vector3 normal, Vector3 point) plane1, (Vector3 normal, Vector3 point) plane2)
    {
        linePoint = Vector3.zero;
        lineVec = Vector3.zero;

        //We can get the direction of the line of intersection of the two planes by calculating the
        //cross product of the normals of the two planes. Note that this is just a direction and the line
        //is not fixed in space yet.
        lineVec = Vector3.Cross(plane1.normal, plane2.normal);

        //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
        //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
        //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
        //the cross product of the normal of plane2 and the lineDirection.      
        Vector3 ldir = Vector3.Cross(plane2.normal, lineVec);

        float numerator = Vector3.Dot(plane1.normal, ldir);

        //Prevent divide by zero.
        if (Mathf.Abs(numerator) > 0.000001f)
        {

            Vector3 plane1ToPlane2 = plane1.point - plane2.point;
            float t = Vector3.Dot(plane1.normal, plane1ToPlane2) / numerator;
            linePoint = plane2.point + t * ldir;

            return true;
        }

        return false;
    }
}
