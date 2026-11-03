using UnityEngine;

/// <summary>
/// 把移動狀態告訴 Animator（就是第 4 章 Ch4_HeroAnimator 的參考做法）。
/// 第 6 章直接用它，讓你專心在 Animation Event 上。學生不用改。
/// </summary>
[RequireComponent(typeof(Animator))]
public class HeroAnimDriver : MonoBehaviour
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
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("Grounded", move.IsGrounded());
        if (move.MoveInput > 0f) sr.flipX = false;
        if (move.MoveInput < 0f) sr.flipX = true;
    }
}
