using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 本教材共用的角色移動（學生不用改）。
/// A / D 或方向鍵移動，空白鍵跳躍。Update 讀輸入、FixedUpdate 改物理。
///
/// acceleration 決定「速度追上目標有多快」：數字大 = 一按就到全速；
/// 數字小 = 慢慢加速、放開還會滑一段（第 3 章的冰面就是把它調小）。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HeroMove : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 5f;
    public float jumpForce = 11f;
    [Tooltip("每秒最多改變多少水平速度。60 = 很靈敏，3 = 像在冰上")]
    public float acceleration = 60f;

    [Header("面向")]
    [Tooltip("勾選時，往左走會自動左右翻轉圖片。第 4 章會關掉，改由你的腳本控制")]
    public bool autoFlip = true;

    [Header("地面偵測")]
    [Tooltip("從角色中心往下多遠是腳底")]
    public float feetOffset = 0.45f;
    public float checkDistance = 0.12f;
    public LayerMask groundLayers;

    /// <summary>目前的左右輸入：-1、0 或 1。其他腳本可以讀。</summary>
    public float MoveInput { get; private set; }

    Rigidbody2D rb;
    SpriteRenderer sr;
    bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float input = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input += 1f;
        MoveInput = input;

        // wasPressedThisFrame 只有一幀是 true，FixedUpdate 不一定剛好在那一幀執行，所以用旗標傳過去
        if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            jumpRequested = true;

        if (autoFlip && sr != null && input != 0f) sr.flipX = input < 0f;
    }

    void FixedUpdate()
    {
        float target = MoveInput * moveSpeed;
        float x = Mathf.MoveTowards(rb.linearVelocity.x, target, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

        if (jumpRequested)
        {
            jumpRequested = false;
            if (IsGrounded()) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    /// <summary>腳底下有沒有踩著 groundLayers 的東西。</summary>
    public bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * feetOffset;
        return Physics2D.Raycast(origin, Vector2.down, checkDistance, groundLayers).collider != null;
    }

    /// <summary>腳底正下方一點點的世界座標（第 3 章用來查「踩在哪一格」）。</summary>
    public Vector3 FeetPosition()
    {
        return transform.position + Vector3.down * (feetOffset + 0.1f);
    }
}
