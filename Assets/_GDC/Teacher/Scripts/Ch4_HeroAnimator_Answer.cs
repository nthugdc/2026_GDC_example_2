using UnityEngine;

/// <summary>第 4 章解答（教師版場景使用）。說明見 Teacher/Answers/Ch4_HeroAnimator.txt</summary>
[RequireComponent(typeof(Animator))]
public class Ch4_HeroAnimator_Answer : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    HeroMove move;
    SpriteRenderer sr;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<HeroMove>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 4-A
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // 4-B
        anim.SetBool("Grounded", move.IsGrounded());

        // 4-C
        if (move.MoveInput > 0f) sr.flipX = false;
        if (move.MoveInput < 0f) sr.flipX = true;
    }
}
