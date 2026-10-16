using UnityEngine;

/// <summary>
/// 第 4 章練習：把「角色現在在做什麼」告訴 Animator，讓它切換動畫。
/// 掛在 Player 上。Animator 的狀態與轉換你在 Animator 視窗裡設定，
/// 這支腳本只負責每一幀把參數更新進去。
///
/// ═══ Animator 參數 ═══
///
///   void SetFloat(string name, float value)    設定 Float 參數
///   void SetBool(string name, bool value)      設定 Bool 參數
///
///     name    參數名稱，要跟 Animator 視窗裡的「一字不差」（大小寫也算）
///     value   要設成的值
///
///   ⚠ 名字打錯不會編譯失敗，只會在 Console 出現黃色警告
///     「Parameter 'xxx' does not exist」，動畫就是不會切換。
///
/// ═══ 這章會用到的數值 ═══
///
///   rb.linearVelocity.x        水平速度。往右是正、往左是負
///   move.IsGrounded()          有沒有站在地上（bool）
///   move.MoveInput             玩家按的方向：-1（左）、0、1（右）
///   sr.flipX                   true = 圖片左右翻轉
///   Mathf.Abs(數字)            絕對值：-3 變 3，3 還是 3
///
///   這張 sprite sheet 的角色本來就面向右邊。
/// </summary>
[RequireComponent(typeof(Animator))]
public class Ch4_HeroAnimator : MonoBehaviour
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
        // ────────── TODO 4-A ──────────
        // 把 Animator 的 Speed 參數設成「水平速度有多快」。
        //
        // 想一想：往左跑的時候水平速度是負的。
        //         如果 Idle → Run 的條件是 Speed > 0.1，負的速度會發生什麼事？


        // ────────── TODO 4-B ──────────
        // 把 Animator 的 Grounded 參數設成「現在有沒有站在地上」。


        // ────────── TODO 4-C ──────────
        // 讓角色面向移動方向：按左就翻轉圖片，按右就不翻。
        //
        // 沒按方向鍵的時候呢？應該維持原本的方向，不要改它。

    }
}
