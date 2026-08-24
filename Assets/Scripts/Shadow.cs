using UnityEngine;

public class Shadow : MonoBehaviour
{
    protected const float MAX_DISTANCE = 10f;
    protected const float SCALE_DISTANCE = 6f;
    protected const float OFFSET = 0.01f;

    [SerializeField] protected Transform source;
    [SerializeField] protected GameObject render;
    [SerializeField] protected LayerMask solid;

    protected RaycastHit hit;
    protected float baseDistance;

    private void Start()
    {
        baseDistance = Mathf.Abs(source.position.y - render.transform.position.y);
    }

    protected virtual void LateUpdate()
    {
        if (Physics.Raycast(source.position, Vector3.down, out hit, MAX_DISTANCE, solid))
        {
            float size = 1-(Mathf.Max(hit.distance-baseDistance, 0f)/ SCALE_DISTANCE);
            render.transform.localScale = Vector3.one * size;
            render.transform.position = hit.point + Vector3.up * OFFSET;
            render.SetActive(true);
        }
        else
        {
            render.SetActive(false);
        }
    }
}
