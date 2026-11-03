using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 第 6 章練習：Animation Event —— 讓「動畫」在對的那一格呼叫你的函式。
/// 掛在 Player 上。按 J 攻擊，劍揮到的那一格打碎前方的木箱；跑步時腳邊揚起灰塵。
///
/// ═══ Animation Event 是什麼 ═══
///
///   在 Animation Clip 的時間軸上插一面小旗子，動畫播到那裡時，
///   Unity 會去呼叫「同一個 GameObject 上」某個腳本的函式。
///
///   這支腳本的 AttackHit() 和 Footstep() 不是你自己呼叫的，
///   是你在 Animation 視窗按 Add Event、選好 Function 之後，由動畫呼叫的。
///
/// ═══ 四條規則（記不得就回來看）═══
///
///   1. 函式要是 public，Function 下拉選單才找得到
///   2. 腳本要掛在「有 Animator 的那個 GameObject」上
///   3. 參數最多一個，型別只能是 float、int、string 或 Object（也可以沒有參數）
///   4. 名字要完全一樣。Event 找不到函式時，Console 會出現
///      「AnimationEvent 'XXX' on animation 'YYY' has no receiver!」
///
/// ═══ Animator.SetTrigger ═══
///
///   void SetTrigger(string name)
///
///     「扣一下扳機」：Trigger 型別的 Parameter 會變成 true 一下，
///     被某條 Transition 用掉之後自己變回 false。適合攻擊、受傷這種「一次性」的動作。
///     這一章的 Animator Controller 已經有 Attack 這個 Trigger。
///
/// ═══ Physics2D.OverlapCircleAll ═══
///
///   Collider2D[] OverlapCircleAll(Vector2 point, float radius)
///
///     point    圓心的世界座標
///     radius   半徑
///     回傳     圓裡面「所有」碰撞器的陣列；什麼都沒有就是空陣列（長度 0），不是 null
///
///   拿到 Collider2D 之後，用 GetComponent<Breakable>() 問它「你是不是木箱」：
///   是的話拿到 Breakable，不是的話拿到 null。木箱有 Break() 可以呼叫。
///
/// ═══ 寫好給你用的東西 ═══
///
///   AttackCenter()                  劍打得到的位置（會依面向自動換左右）
///   attackRadius                    劍的攻擊範圍（半徑）
///   move.FeetPosition()             腳底的世界座標
///   GDCFx.Puff(位置, 顏色)          在那個位置冒一團煙
///   Keyboard.current.jKey.wasPressedThisFrame   J 鍵這一幀剛按下
/// </summary>
[RequireComponent(typeof(Animator))]
public class Ch6_HeroAttack : MonoBehaviour
{
    [Header("攻擊")]
    [Tooltip("劍打得到的距離（從身體中心往前）")]
    public float attackReach = 0.65f;
    [Tooltip("劍的攻擊範圍（半徑）")]
    public float attackRadius = 0.45f;

    [Header("腳步")]
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

        // ────────── TODO 6-A ──────────
        // 按下 J 的那一幀，讓 Animator 播攻擊動畫。
        //
        // 想一想：攻擊是「一次性」的動作，該用哪一種 Parameter？上面的說明有寫。

    }

    /// <summary>劍打得到的位置：面向右就在右邊，面向左（flipX）就在左邊。</summary>
    Vector2 AttackCenter()
    {
        float facing = sr.flipX ? -1f : 1f;
        return (Vector2)transform.position + new Vector2(facing * attackReach, 0f);
    }

    // ═══ 下面兩個函式由 Animation Event 呼叫 ═══

    /// <summary>攻擊動畫揮到前方的那一格呼叫。</summary>
    public void AttackHit()
    {
        // ────────── TODO 6-B ──────────
        // 找出 AttackCenter() 周圍 attackRadius 內所有的碰撞器，
        // 其中是木箱（有 Breakable）的，就把它打碎。
        //
        // 需要一個 foreach 跑過回傳的陣列。

    }

    /// <summary>跑步動畫腳著地的那幾格呼叫。</summary>
    public void Footstep()
    {
        // ────────── TODO 6-C ──────────
        // 在腳邊冒一團煙（顏色用 dustColor）。
        //
        // 加分：如果角色不在地上（例如跳起來的瞬間），就不要冒煙。

    }

    // 在 Scene 視窗畫出攻擊範圍（黃色圓圈），方便你調 attackReach 和 attackRadius
    void OnDrawGizmosSelected()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(AttackCenter(), attackRadius);
    }
}
