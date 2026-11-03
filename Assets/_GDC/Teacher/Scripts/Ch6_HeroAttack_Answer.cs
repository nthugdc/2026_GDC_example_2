using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>第 6 章解答（教師版場景使用）。說明見 Teacher/Answers/Ch6_HeroAttack.txt</summary>
[RequireComponent(typeof(Animator))]
public class Ch6_HeroAttack_Answer : MonoBehaviour
{
    public float attackReach = 0.65f;
    public float attackRadius = 0.45f;
    public Color dustColor = new Color(0.9f, 0.85f, 0.75f, 0.8f);

    Animator anim;
    HeroMove move;
    SpriteRenderer sr;

    void Awake()
    {
        anim = GetComponent<Animator>();
        move = GetComponent<HeroMove>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // 6-A
        if (kb.jKey.wasPressedThisFrame) anim.SetTrigger("Attack");
    }

    Vector2 AttackCenter()
    {
        float facing = sr.flipX ? -1f : 1f;
        return (Vector2)transform.position + new Vector2(facing * attackReach, 0f);
    }

    public void AttackHit()
    {
        // 6-B
        foreach (Collider2D c in Physics2D.OverlapCircleAll(AttackCenter(), attackRadius))
        {
            Breakable box = c.GetComponent<Breakable>();
            if (box != null) box.Break();
        }
    }

    public void Footstep()
    {
        // 6-C（含加分）
        if (!move.IsGrounded()) return;
        GDCFx.Puff(move.FeetPosition() + Vector3.up * 0.1f, dustColor, 0.25f);
    }

    void OnDrawGizmosSelected()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(AttackCenter(), attackRadius);
    }
}
