using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>第 7 章解答（教師版場景使用）。說明見 Teacher/Answers/Ch7_HeroStateMachine.txt</summary>
[RequireComponent(typeof(Animator))]
public class Ch7_HeroStateMachine_Answer : MonoBehaviour
{
    public HeroState state = HeroState.Idle;
    public float attackReach = 0.65f;
    public float attackRadius = 0.45f;

    Animator anim;
    Rigidbody2D rb;
    HeroMove move;
    SpriteRenderer sr;
    StateLabel label;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<HeroMove>();
        sr = GetComponent<SpriteRenderer>();
        label = GetComponentInChildren<StateLabel>();
    }

    void Start()
    {
        ChangeState(HeroState.Idle);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        bool attackPressed = kb.jKey.wasPressedThisFrame;
        bool moving = kb.aKey.isPressed || kb.dKey.isPressed || kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed;
        bool grounded = move.IsGrounded();
        bool falling = rb.linearVelocity.y <= 0.01f;

        if (state != HeroState.Attack)
        {
            if (move.MoveInput > 0f) sr.flipX = false;
            if (move.MoveInput < 0f) sr.flipX = true;
        }

        switch (state)
        {
            case HeroState.Idle:
                if (attackPressed) ChangeState(HeroState.Attack);
                else if (!grounded) ChangeState(HeroState.Jump);
                else if (moving) ChangeState(HeroState.Run);
                break;

            case HeroState.Run:
                // 7-A
                if (attackPressed) ChangeState(HeroState.Attack);
                else if (!grounded) ChangeState(HeroState.Jump);
                else if (!moving) ChangeState(HeroState.Idle);
                break;

            case HeroState.Jump:
                // 7-B
                if (grounded && falling) ChangeState(HeroState.Idle);
                break;

            case HeroState.Attack:
                break;
        }
    }

    void ChangeState(HeroState next)
    {
        state = next;
        anim.Play(next.ToString());
        if (label != null) label.Set(next.ToString());

        // 7-C
        if (next == HeroState.Attack)
        {
            move.enabled = false;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            move.enabled = true;
        }
    }

    public void AttackHit()
    {
        float facing = sr.flipX ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + new Vector2(facing * attackReach, 0f);
        foreach (Collider2D c in Physics2D.OverlapCircleAll(center, attackRadius))
        {
            Breakable box = c.GetComponent<Breakable>();
            if (box != null) box.Break();
        }
    }

    public void AttackEnd()
    {
        ChangeState(HeroState.Idle);
    }
}
