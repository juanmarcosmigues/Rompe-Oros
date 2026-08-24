using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;

public class CombatEntity : ExtendedMonobehaviour
{
    private const float HIT_DETECT_TIME_THRESHOLD = 0.3f;
    private const int HIT_BUFFER_SIZE = 10;

    public enum HitResults { Ignored, Defended, Hit, Kill }

    [Header("References")]
    public Hurtbox[] hurtboxes;
    public HitBox[] hitboxes;

    [Header("Settings")]
    public int life;
    public bool invulnerable = false;
    public bool disabled = false;
    public bool disableOnDeath = true;
    public bool autoFill = false;

    [Header("Events")]
    public UnityEvent onHurt;
    public UnityEvent onDeath;
    public System.Action<HitBox, string, HitResults, int, float, Vector3, Vector3> onRecieveDamageEvent;
    public System.Action<CombatEntity, string, HitResults, int, Vector3, Vector3> onHitPerformedResults;
    public System.Action<GameObject, Hurtbox, int, Vector3, Vector3> onHitSomething;
    public System.Action<int> onLifeChange;
    public System.Action<int, string> onHeal;
    public GetHitEvaluation customEvaluation;
    public DamageOutput customDamageOutput;

    public delegate int DamageOutput 
        (HitBox hb, string tag, float weight, int damage, Vector3 point, Vector3 direction);
    public delegate bool GetHitEvaluation(
        HitBox hb, string tag, float weight, int damage, Vector3 point, Vector3 direction);
    public int myId { get; private set; }
    public int currentLife
    {
        get
        {
            return _currentLife;
        }
        protected set
        {
            _bufferedCurrentLife = _currentLife;            
            _currentLife = value;
            onLifeChange?.Invoke(value - _bufferedCurrentLife);
        }
    }
    public float currentLifeNormalized => currentLife / (float)life;
    public virtual bool dead { get; protected set; }

    protected int _bufferedCurrentLife;
    protected int _currentLife;
    protected Dictionary<int, float> _recentHitBoxes;
    protected List<int> _recentHitBoxesIDs;
    protected int[] _recentHitBoxesToDelete;
    protected int _index;

    protected virtual void Awake()
    {
        myId = gameObject.GetInstanceID();

        currentLife = life;

        _recentHitBoxes = new Dictionary<int, float>();
        _recentHitBoxesIDs = new List<int>();
        _recentHitBoxesToDelete = new int[HIT_BUFFER_SIZE];

        if (autoFill)
        {
            hurtboxes = GetComponentsInChildren<Hurtbox>(true);
            hitboxes = GetComponentsInChildren<HitBox>(true);
        }
    }
    protected override void Start()
    {
        base.Start();

        foreach (var hb in hurtboxes)
        {
            hb.entity = this;
            hb.onHurt += (HitBox hitbox, string tag, float weight, int dmg, Vector3 pt, Vector3 dir) =>
            {
                RecieveDamage(hitbox, tag, weight, dmg, pt, dir);
            };
        }
        foreach (var hitb in hitboxes)
        {
            hitb.Initialize();
            hitb.entityID = this.myId;
            hitb.onHit += (GameObject o, Hurtbox hb, int damage, Vector3 pt, Vector3 dir) =>
            {
                onHitSomething?.Invoke(o, hb, damage, pt, dir);
            };
            hitb.onHitResults += DealDamage;
        }
    }

    private void LateUpdate()
    {
        _index = 0;

        for (var e = _recentHitBoxesIDs.GetEnumerator(); e.MoveNext();)
        {
            _recentHitBoxes[e.Current] -= Time.deltaTime;

            if (_recentHitBoxes[e.Current] < 0f)
            {
                _recentHitBoxes.Remove(e.Current);
                _recentHitBoxesToDelete[_index] = e.Current;
                _index++;
            }            
        }

        for (int i = 0; i < _index; i++)
        {
            _recentHitBoxesIDs.Remove(_recentHitBoxesToDelete[i]);
        }
    }
    public virtual void DealDamage (CombatEntity ce, string tag, HitResults hr, int damage, Vector3 point, Vector3 direction)
    {
        onHitPerformedResults?.Invoke(ce, tag, hr, damage, point, direction);
    }
    public virtual void RecieveDamage (HitBox hb, string tag, float weight, int damage, Vector3 point, Vector3 direction)
    {
        //REGISTER HIT: this is used to avoid hitting the same hitbox a few frames apart.
        if (hb != null)
        {            
            if (_recentHitBoxes.ContainsKey(hb.hitBoxID))
            {
                return;
            }
            _recentHitBoxesIDs.Add(hb.hitBoxID);
            _recentHitBoxes.Add(hb.hitBoxID, HIT_DETECT_TIME_THRESHOLD);
        }

        //EVALUATE
        if (disabled)
        {
            hb?.HitResults(this, tag, HitResults.Ignored, 0, point, direction);
            return;
        }           

        if (invulnerable)
        {
            hb?.HitResults(this, tag, HitResults.Ignored, 0, point, direction);
            return;
        }

        if (dead)
        {
            hb?.HitResults(this, tag, HitResults.Ignored, 0, point, direction);
            return;
        }

        if (customEvaluation != null && !customEvaluation(hb, tag, weight, damage, point, direction))
        {
            hb?.HitResults(this, tag, HitResults.Ignored, 0, point, direction);
            return;
        }

        ///CUSTOM DAMAGE OUTPUT
        damage = customDamageOutput != null ? 
            customDamageOutput(hb, tag, weight, damage, point, direction) 
            : damage;

        currentLife -= damage;

        if (currentLife <= 0)
        {
            onDeath.Invoke();
            onRecieveDamageEvent?.Invoke(hb, tag, HitResults.Kill, damage, weight, point, direction);
            hb?.HitResults(this, tag, HitResults.Kill, damage, point, direction);

            dead = true;

            if (disableOnDeath)
                gameObject.SetActive(false);

            return;
        }

        onHurt.Invoke();
        onRecieveDamageEvent?.Invoke(hb, tag, HitResults.Hit, damage, weight, point, direction);
        hb?.HitResults(this, tag, HitResults.Hit, damage, point, direction);
    }

    public void Heal (int amount, string tag, bool capHeal = true)
    {
        if (capHeal)
        {
            amount = Mathf.Clamp(amount, 0, life - currentLife);
        }
        currentLife += amount;
        onHeal?.Invoke(amount, tag);
    }
    public void ChangeMaxLife (int newMaxLife, bool heal = true)
    {
        var diff = newMaxLife - life;
        life = newMaxLife;
        if (heal && diff > 0)
        {
            Heal(diff, "Change Max Life");
        }
        else if (diff < 0)
        {
            currentLife = life;
        }
    }
    public void SetLife(int life)
    {
        currentLife = life;
        dead = currentLife > 0;
    }
    private void OnDisable()
    {
        _index = 0;
        _recentHitBoxes.Clear();
        _recentHitBoxesIDs.Clear();
    }
    public void Restart()
    {
        dead = false;
        currentLife = life;
    }
}
