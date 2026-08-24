using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public virtual int entityID { get; set; }
    public int hitBoxID { get; set; }
    public Collider coll { get; private set; }

    public string[] hitTags;
    public string ignoreTag;
    public int damage;
    public float weight;
    public string hitboxTag;
    public bool hitboxGroup;
    [SerializeField] GameObject owner;

    public CustomContactCalculator customContact;
    public System.Action<GameObject, Hurtbox, int, Vector3, Vector3> onHit;
    public System.Action<CombatEntity, string, CombatEntity.HitResults, 
        int, Vector3, Vector3> onHitResults;

    protected (Vector3, Vector3) _points; //item1 = contactpoint, item2 = direction
    protected bool initialized;
    protected LayerMask hitMask;

    public delegate (Vector3, Vector3) 
        CustomContactCalculator (Hurtbox target, HitBox source);

    protected virtual void Awake()
    {
        if (!initialized)
            Initialize();
    }

    public virtual void Initialize ()
    {
        gameObject.layer = LayerMask.NameToLayer("Hitbox");
        hitMask = LayerMask.GetMask("Hurtbox");
        hitBoxID = hitboxGroup ? transform.parent.gameObject.GetInstanceID() : gameObject.GetInstanceID();
        coll = gameObject.GetComponent<Collider>();
        initialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnCollision(other.gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        OnCollision(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision.gameObject);
    }
    protected virtual void OnCollision (GameObject other)
    {
        if (hitMask != (hitMask | (1 << other.gameObject.layer)))
            return;
        if (hitTags.Length > 0 && hitTags.Get(other.gameObject.tag) == default(string))
            return;
        if (other.gameObject.tag == ignoreTag) 
            return;

        var hurtBox = other.GetComponent<Hurtbox>();

        _points = Calculate(hurtBox);

        if (hurtBox != null)
        {
            //avoid detecting the same entity it belongs
            if (hurtBox.entityID != 0 && hurtBox.entityID.Equals(this.entityID))
                return;

            //Ignore hit if below threhsold of hurtbox
            if (hurtBox.weightDetectionThreshold > weight || hurtBox.damageDetectionThreshold > damage)
            {
                HitResults(null, tag, CombatEntity.HitResults.Ignored, 0, _points.Item1, _points.Item2);
                return;
            }

            hurtBox.GetHit(this, hitboxTag, weight, damage, _points.Item1, _points.Item2);
        }

        onHit?.Invoke(other, hurtBox, damage, _points.Item1, _points.Item2);
    }

    protected virtual (Vector3, Vector3) Calculate (Hurtbox target)
    {
        if (customContact != null)
            return customContact(target, this);

        return (transform.position, (target.transform.position - transform.position).normalized);
    }

    public virtual void HitResults (CombatEntity entity, string tag, CombatEntity.HitResults HitResults, int damageDealt, Vector3 point, Vector3 direction)
    {
        onHitResults?.Invoke(entity, tag, HitResults, damageDealt, point, direction);
    }

    public GameObject Owner => owner != null ? owner.gameObject : gameObject;
}
