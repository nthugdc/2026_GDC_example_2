using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 教師工具（不是教材的一部分）：自動往右跑、遇到牆或坑就跳。
/// 用來快速驗證關卡能不能通，也可以上課時示範「這關是過得去的」。
/// 在 Play Mode 把它加到 Player 上就會開始跑，移除就停。
/// 它是透過 Input System 模擬按鍵，所以走的是跟玩家完全一樣的程式路徑。
/// </summary>
public class GDCAutopilot : MonoBehaviour
{
    [Tooltip("1 = 往右，-1 = 往左")]
    public float direction = 1f;
    [Tooltip("前方多遠沒有地面就起跳")]
    public float gapLookAhead = 0.55f;
    [Tooltip("這些磁磚雖然踩得上去，但要當成坑跳過（例如第 3 章的岩漿）")]
    public UnityEngine.Tilemaps.Tilemap avoidMap;
    public UnityEngine.Tilemaps.TileBase[] avoidTiles;

    HeroMove move;
    bool jumpedLastFrame;

    void Awake()
    {
        move = GetComponent<HeroMove>();
    }

    void OnDisable()
    {
        if (Keyboard.current != null) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || move == null) return;

        List<Key> keys = new List<Key>();
        keys.Add(direction > 0f ? Key.D : Key.A);

        bool wantJump = false;
        if (move.IsGrounded())
        {
            Vector2 pos = transform.position;
            Vector2 dir = new Vector2(direction, 0f);
            bool wall = Physics2D.Raycast(pos + Vector2.down * 0.3f, dir, 0.5f, move.groundLayers).collider != null;
            Vector2 ahead = pos + dir * gapLookAhead;
            // 往下看 2.5 格：只是往下一階不算坑；看到危險物（Hazard 圖層）也算坑
            int mask = move.groundLayers | LayerMask.GetMask("Hazard");
            RaycastHit2D hit = Physics2D.Raycast(ahead, Vector2.down, 2.5f, mask);
            bool gap = hit.collider == null || hit.collider.gameObject.layer == LayerMask.NameToLayer("Hazard");
            if (!gap && avoidMap != null)
            {
                UnityEngine.Tilemaps.TileBase t = avoidMap.GetTile(avoidMap.WorldToCell(hit.point + Vector2.down * 0.1f));
                if (System.Array.IndexOf(avoidTiles, t) >= 0) gap = true;
            }
            wantJump = wall || gap;
        }

        // 放開一幀再按，wasPressedThisFrame 才會再次成立
        if (wantJump && !jumpedLastFrame) keys.Add(Key.Space);
        jumpedLastFrame = wantJump && !jumpedLastFrame;

        InputSystem.QueueStateEvent(kb, new KeyboardState(keys.ToArray()));
    }
}
