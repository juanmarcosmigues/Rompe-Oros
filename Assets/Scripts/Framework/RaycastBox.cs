
using UnityEngine;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RaycastBox : MonoBehaviour
{
    public Vector2 resolution;
    public Color color;
    [Range(0f, 180f)]
    public float maxAngle = 180f;
    public bool debug = true;
    public bool debugIndexes = false;

    protected Vector2 cachedResolution;
    protected Vector3[] origins;
    public (bool hit, RaycastHit data)[] hits { get; protected set; }

    private void Awake()
    {
        SetOrigins();
    }

    void SetOrigins ()
    {
        origins = new Vector3[Mathf.RoundToInt(resolution.x * resolution.y)];
        int i = 0;
        for (int y = 0; y < resolution.y; y++)
        {
            for (int x = 0; x < resolution.x; x++)
            {
                origins[i] = 
                    new Vector3(Mathf.Lerp(-transform.localScale.x * 0.5f, 
                    transform.localScale.x * 0.5f, resolution.x > 1 ? (x / (resolution.x-1)) : 0.5f),
                    Mathf.Lerp(transform.localScale.y * 0.5f, 
                    -transform.localScale.y * 0.5f,resolution.y > 1 ? (y / (resolution.y - 1)) : 0.5f));

                i++;
            }    
        }

        hits = new (bool hit, RaycastHit data)[origins.Length];

        cachedResolution = resolution;
    }

    public int Evaluate (LayerMask mask)
    {
        int hitsAmount = 0;

        for (int r = 0; r < origins.Length; r++)
        {
            hits[r].hit = Physics.Linecast(
                transform.TransformPoint(origins[r] - Vector3.forward * 0.5f), //From
                transform.TransformPoint(origins[r] + Vector3.forward * 0.5f), //To
                out hits[r].data,
                mask);

            if (hits[r].hit)
            {
                if (Vector3.Angle(-transform.forward, hits[r].data.normal) < maxAngle)
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
        if (!debug ) return;

        var c = color * 0.5f;
        c.a = 0.2f;
        Gizmos.color = c;

        GizmosExtensions.DrawWireCube(transform.position, transform.lossyScale, transform.rotation);

        if (origins == null || cachedResolution != resolution)
        {
            SetOrigins();
        }

        Gizmos.color = color;
        for (int i = 0; i < origins.Length; i++)
        {
            if (debugIndexes)
            Handles.Label(transform.TransformPoint(origins[i] - Vector3.forward * 0.5f) + Vector3.up * 0.1f, i.ToString());

            GizmosExtensions.DrawArrow(transform.TransformPoint(origins[i] - Vector3.forward * 0.5f),
                transform.TransformPoint(origins[i] + Vector3.forward * 0.5f));
        }

#endif
    }
}
