using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hurtbox : MonoBehaviour
{
    public float weightDetectionThreshold;
    public int damageDetectionThreshold;
    public string[] ignoreTags;

    public CombatEntity entity { get; set; }
    public Collider coll {  get; private set; }
    public int entityID => entity? entity.myId : 0; //didnt optimize this

    public System.Action<HitBox, string, float, int, Vector3, Vector3> onHurt;
    public UnityEvent onHurtEvent;


    private void Awake()
    {
        gameObject.layer =
            LayerMask.NameToLayer("Hurtbox");
        coll = gameObject.GetComponent<Collider>();
    }
    public virtual bool GetHit(HitBox hitbox, string tag, float weight, int damage, Vector3 contactPoint, Vector3 direction)
    {
        for (int i = 0; i < ignoreTags.Length; i++) 
            if (ignoreTags[i] == tag) return false;

        if (hitbox != null)
        {
            if (hitbox.transform.parent == transform.parent) //has to improve this
                return false;
        }
        onHurt?.Invoke(hitbox, tag, weight, damage, contactPoint, direction);
        onHurtEvent?.Invoke();

        return true;
    }
}
