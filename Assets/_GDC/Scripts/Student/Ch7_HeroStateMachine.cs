using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>主角可能處在的狀態（State）。同一時間只會是其中一個。</summary>
public enum HeroState { Idle, Run, Jump, Attack }

/// <summary>
/// 第 7 章練習：用程式寫 State Machine（狀態機）。
/// 掛在 Player 上。第 4 章你在 Animator 視窗「畫箭頭」決定什麼時候切動畫；
/// 這一章的 Animator Controller 只有四個 State、一條箭頭都沒有，改由這支腳本決定。
///
/// ═══ State Machine 的三個積木 ═══
///
///   1. enum HeroState { Idle, Run, Jump, Attack }
///        把「所有可能的狀態」列出來。state 這個變數永遠是其中一個。
///
///   2. switch (state) { case HeroState.Idle: ... break; }
///        每一幀只執行「目前那個 State」的那一段程式。
///        每一段只做一件事：檢查條件，決定要不要換到別的 State。
///        ⚠ C# 的每個 case 結尾一定要有 break，少了會編譯失敗。
///
///   3. ChangeState(HeroState.Run)
///        換 State 的唯一入口（已經寫好）：記下新的 State、播對應的動畫、更新頭上的字。
///        「剛進入某個 State 時要做的事」也寫在這裡。
///
/// ═══ 這章用到的東西 ═══
///
///   attackPressed   這一幀有沒有按 J（bool）
///   moving          有沒有按左右鍵（bool）
///   grounded        有沒有站在地上（bool）
///   falling         是不是在往下掉，或已經停住（垂直速度 <= 0）（bool）
///   move.enabled    HeroMove 這個元件開著嗎。設成 false，角色就不會理左右鍵和空白鍵
///   rb.linearVelocity  目前的速度（Vector2）
///
///   Idle 那一段是寫好的範例，照著它的樣子寫其他 State。
/// </summary>
[RequireComponent(typeof(Animator))]
public class Ch7_HeroStateMachine : MonoBehaviour
{
    [Header("目前的 State（執行時會自己變，可以在 Inspector 看）")]
    public HeroState state = HeroState.Idle;

    [Header("攻擊")]
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

        // 面向：攻擊中不轉身
        if (state != HeroState.Attack)
        {
            if (move.MoveInput > 0f) sr.flipX = false;
            if (move.MoveInput < 0f) sr.flipX = true;
        }

        switch (state)
        {
            case HeroState.Idle:
                // ── 範例（已經寫好）：站著不動時，看情況換到別的 State ──
                if (attackPressed) ChangeState(HeroState.Attack);
                else if (!grounded) ChangeState(HeroState.Jump);
                else if (moving) ChangeState(HeroState.Run);
                break;

            case HeroState.Run:
                // ────────── TODO 7-A ──────────
                // 跑步中：按 J → Attack；腳離地 → Jump；放開方向鍵 → Idle。
                //
                // 想一想：三個條件同時成立時，誰該優先？（看 Idle 的範例怎麼排）
                break;

            case HeroState.Jump:
                // ────────── TODO 7-B ──────────
                // 空中：「站到地上」而且「已經不是往上衝」時，回到 Idle。
                //
                // 想一想：為什麼不能只看 grounded？
                // （提示：跳上台階時，人還在往上衝，腳底的偵測線就可能先碰到台階邊緣）
                break;

            case HeroState.Attack:
                // 攻擊中什麼都不檢查：要等動畫播完，由 Animation Event 呼叫 AttackEnd() 才離開
                break;
        }
    }

    /// <summary>換 State 的唯一入口。</summary>
    void ChangeState(HeroState next)
    {
        state = next;
        anim.Play(next.ToString());              // Animator 裡的 State 名字剛好跟 enum 一樣
        if (label != null) label.Set(next.ToString());

        // ────────── TODO 7-C ──────────
        // 「剛進入」State 時要做的事：
        //   進入 Attack → 不准移動（move.enabled 設成 false），並把水平速度歸零（垂直速度保留）
        //   進入其他 State → 恢復移動
        //
        // 寫完故意把「恢復移動」那行拿掉試試看：打一下之後會發生什麼事？
    }

    // ═══ 下面兩個函式由 Animation Event 呼叫（Hero_Attack_Ch7 已經放好了）═══

    /// <summary>劍揮到前方的那一格：打碎範圍內的木箱。</summary>
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

    /// <summary>攻擊動畫的最後一格：攻擊結束，回到 Idle。</summary>
    public void AttackEnd()
    {
        ChangeState(HeroState.Idle);
    }
}
