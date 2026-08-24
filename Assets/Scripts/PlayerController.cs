using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public SpriteAnimator animator;
    public PlayerInput playerInput;
    public Character character;

    public float jumpHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator.Play("Player@Idle01", 0);
        playerInput.GetButton("ButtonWest").onPressedDown = Attack;
        playerInput.GetButton("ButtonSouth").onPressedDown = Jump;
        playerInput.GetButton("ButtonSouth").onRelease = CancelJump;
        playerInput.onMovementAxisMove = Move;
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
    }
    public void Jump ()
    {
        character.Jump(jumpHeight);
    }
    public void CancelJump (float releaseButtonTime)
    {
        //character.ClearUpwardGravity();
    }
    public void Attack ()
    {
        animator.Play("Player@Attack01", 1);
    }
}
