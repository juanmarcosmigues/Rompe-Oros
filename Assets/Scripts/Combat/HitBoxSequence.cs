using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxSequence : MonoBehaviour
{
    public FloatItemPair<GameObject>[] hitBoxes;
    public float duration;

    protected float t = 0f;

    private void OnEnable()
    {
        t = 0f;
    }

    private void Update()
    {
        if (t > duration)
        {
            for (int i = 0; i < hitBoxes.Length; i++)
            {
                hitBoxes[i].item.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < hitBoxes.Length; i++)
        {
            hitBoxes[i].item.SetActive(hitBoxes[i].val < t);
        }
        t += Time.deltaTime;

    }
}
