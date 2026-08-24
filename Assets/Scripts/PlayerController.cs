using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public SpriteAnimator animator;
    public PlayerInput playerInput;
    public Character character;
    public HitBox hitBox;

    public float jumpHeight;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        animator.Play("Player@Idle01", 0);
        playerInput.GetButton("ButtonWest").onPressedDown = Attack;
        playerInput.GetButton("ButtonSouth").onPressedDown = Jump;
        playerInput.GetButton("ButtonSouth").onRelease = CancelJump;
        playerInput.onMovementAxisMove = Move;

        hitBox.onHitResults += OnHitOther;
    }

    void Update()
    {
        
    }

    public void Move (Vector2 dir, float amount)
    {
        Vector3 wDir = default;
        wDir.z = dir.y;
        wDir.x = dir.x;
        amount *= 1 + Mathf.Min(Mathf.Abs(Mathf.Pow(wDir.z, 5)), 0.75f);
        character.Move(wDir.normalized, amount);

        Vector3 flipScale = Vector3.one;
        if (wDir.x < 0)
            flipScale.x = -1f;
        else
            flipScale.x = 1f;

        animator.transform.localScale = flipScale;
        Vector3 hbPos = hitBox.transform.localPosition;
        hbPos.x = Mathf.Abs(hbPos.x) * flipScale.x;
        hitBox.transform.localPosition = hbPos;
    }
    public void Jump ()
    {
        character.Jump(jumpHeight);
    }
    public void CancelJump (float releaseButtonTime)
    {
        character.CutJump();
    }
    public void Attack ()
    {
        animator.Play("Player@Attack01", 1);
    }

    void OnHitOther (CombatEntity target, string tag, CombatEntity.HitResults results,
        int damage, Vector3 point, Vector3 dir)
    {
        if (results == CombatEntity.HitResults.Hit)
            FrameFreeze.Freeze(0.1f);
        else if (results == CombatEntity.HitResults.Kill)
            FrameFreeze.Freeze(0.2f);
    }
}
