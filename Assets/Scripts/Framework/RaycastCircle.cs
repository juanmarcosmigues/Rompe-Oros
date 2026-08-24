using System;
using UnityEditor;
using UnityEngine;
using Utils;

public class RaycastCircle : MonoBehaviour
{
    [System.Serializable]
    public struct Ring
    {
        public int resolution;
        public float rotation;
        public float offset;

        public bool Equals(Ring other)
        {
            return resolution == other.resolution
                && rotation == other.rotation
                && offset == other.offset;
        }
        public override bool Equals(object obj)
        => obj is Ring other && Equals(other);
        public override int GetHashCode()
        => HashCode.Combine(resolution, rotation, offset);
    }

    public Ring[] rings;
    public Color color;
    [Range(0f, 180f)]
    public float maxAngle = 180f;
    public bool debug = true;
    public bool debugIndexes = false;

    protected Vector3[] origins;
    protected Ring[] cachedRings;
    protected float radius => (transform.localScale.x + transform.localScale.z) / 2;
    public (bool hit, RaycastHit data)[] hits { get; protected set; }

    private void Awake()
    {
        SetOrigins();
    }

    void SetOrigins()
    {
        if (rings == null || rings.Length == 0) return;

        int amount = 0;
        
        rings.ForEach(r => { amount += r.resolution; });

        origins = new Vector3[amount];

        int i = 0;
        for (int r = 0; r < rings.Length; r++)
        {
            for (int o = 0; o < rings[r].resolution; o++)
            {
                origins[i] = Vector3.zero;
                origins[i] +=
                    Quaternion.AngleAxis(360f * ((float)o / rings[r].resolution) + rings[r].rotation, Vector3.up) * //get directoin
                    Vector3.forward * ((float)r / Mathf.Clamp((rings.Length - 1), 1f, float.MaxValue) + rings[r].offset); //get distance

                i++;
            }
        }

        hits = new (bool hit, RaycastHit data)[origins.Length];

        cachedRings = rings;
    }

    public int Evaluate(LayerMask mask)
    {
        int hitsAmount = 0;

        for (int r = 0; r < origins.Length; r++)
        {
            hits[r].hit = Physics.Linecast(
                transform.TransformPoint(origins[r]), //From
                transform.TransformPoint(origins[r] + Vector3.up), //To
                out hits[r].data,
                mask);

            if (hits[r].hit)
            {
                if (Vector3.Angle(-transform.up, hits[r].data.normal) < maxAngle)
                {
                    hitsAmount++;
                }
                else
                {
                    hits[r].hit = false;
                }
            }
        }

        return hitsAmount;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!debug) return;

        if (!Application.isPlaying)
        {
            SetOrigins();
        }

        var c = color * 0.5f;
        c.a = 0.2f;
        Gizmos.color = c;

        GizmosExtensions.DrawWireCircle(transform.position, radius, 20, transform.rotation);

        Gizmos.color = color;
        for (int i = 0; i < origins.Length; i++)
        {
            if (debugIndexes)
            Handles.Label(transform.TransformPoint(origins[i] - Vector3.forward * 0.5f) + Vector3.up * 0.1f, i.ToString());

            GizmosExtensions.DrawArrow(transform.TransformPoint(origins[i]),
                transform.TransformPoint(origins[i] + Vector3.up));
        }
#endif
    }
}
