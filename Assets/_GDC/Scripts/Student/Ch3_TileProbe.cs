using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

// TODO 還沒寫完時，under 會出現「指定了值卻沒用到」的黃色警告，先關掉免得嚇到人
#pragma warning disable CS0219

/// <summary>
/// 第 3 章練習：用程式讀寫 Tilemap。
/// 掛在 Player 上。腳下踩到冰會變滑、踩到岩漿會回起點；
/// 滑鼠左鍵挖紅磚、右鍵放紅磚。HUD 的「分數」就是你手上有幾塊磚。
///
/// ═══ 格子座標 vs 世界座標 ═══
///
///   世界座標是 Vector3（有小數，例如 3.7, 0.2, 0）。
///   Tilemap 的每一格用 Vector3Int 表示（整數，例如 3, 0, 0）。
///   要問「這個位置是哪一格」，就得先換算。
///
/// ═══ Tilemap.WorldToCell ═══
///
///   Vector3Int WorldToCell(Vector3 worldPosition)
///
///     worldPosition   世界座標
///     回傳            那個位置落在哪一格
///
/// ═══ Tilemap.GetTile ═══
///
///   TileBase GetTile(Vector3Int position)
///
///     position   格子座標
///     回傳       那格放的磁磚資產；空格回傳 null
///
///   回傳的是「磁磚資產」本身，所以可以直接拿去跟 iceTile、lavaTile 比較是不是同一個（==）。
///
/// ═══ Tilemap.SetTile ═══
///
///   void SetTile(Vector3Int position, TileBase tile)
///
///     position   格子座標
///     tile       要放的磁磚。傳 null 就是「清空這格」
///
///   改完之後碰撞會自動更新，不用另外處理。
///
/// ═══ Tilemap.HasTile ═══
///
///   bool HasTile(Vector3Int position)     這格有沒有東西
///
/// ═══ 滑鼠（新版 Input System）═══
///
///   Mouse mouse = Mouse.current;               沒有接滑鼠時是 null
///   mouse.position.ReadValue()                 滑鼠在「螢幕」上的位置（Vector2，單位是像素）
///   mouse.leftButton.wasPressedThisFrame       左鍵這一幀剛按下
///   mouse.rightButton.wasPressedThisFrame      右鍵這一幀剛按下
///
/// ═══ Camera.ScreenToWorldPoint ═══
///
///   Vector3 ScreenToWorldPoint(Vector3 screenPosition)
///
///     把螢幕像素座標換成世界座標。
///
///   ⚠ 換出來的 z 不是 0，而是攝影機附近的 z（大約 -10）。
///     Tilemap 的格子在 z = 0 那一層，z 不對的話 WorldToCell 會算到別層，
///     GetTile 永遠拿到 null。換算完記得把 z 歸零。
///
/// ═══ 其他寫好給你用的東西 ═══
///
///   move.FeetPosition()        腳底正下方一點點的世界座標（Vector3）
///   move.acceleration          角色加速有多快：大 = 靈敏，小 = 很滑
///   respawner.Respawn()        回到起點
///   GameManager.Instance.AddScore(數字)   加減分數（傳負數就是減）
///   GameManager.Instance.score             目前分數
/// </summary>
public class Ch3_TileProbe : MonoBehaviour
{
    [Header("要讀的地圖")]
    public Tilemap groundMap;

    [Header("認得哪些磁磚")]
    public TileBase iceTile;
    public TileBase lavaTile;
    public TileBase brickTile;

    [Header("加速度")]
    [Tooltip("平常的加速度")]
    public float normalAcceleration = 60f;
    [Tooltip("在冰上的加速度。越小越滑")]
    public float iceAcceleration = 3f;

    HeroMove move;
    Respawner respawner;
    Camera cam;

    void Awake()
    {
        move = GetComponent<HeroMove>();
        respawner = GetComponent<Respawner>();
        cam = Camera.main;
    }

    void Update()
    {
        if (groundMap == null) return;

        Vector3Int feetCell = Vector3Int.zero;
        TileBase under = null;

        // ────────── TODO 3-A ──────────
        // 算出「腳底那一格」的格子座標，存進 feetCell；
        // 再查出那一格放的是什麼磁磚，存進 under。
        //
        // 腳底的世界座標上面有寫好的函式可以拿。


        // ────────── TODO 3-B ──────────
        // 如果 under 是冰，把 move.acceleration 設成 iceAcceleration；
        // 不是冰的話，設回 normalAcceleration。
        //
        // 想一想：如果只寫「是冰就變滑」而沒有「否則變回來」，離開冰面之後會怎樣？


        // ────────── TODO 3-C ──────────
        // 如果 under 是岩漿，讓角色回到起點，
        // 並用 GameManager.Instance.Fail("…") 顯示一句提示。


        // ── 滑鼠：挖磚與放磚 ──
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        bool dig = mouse.leftButton.wasPressedThisFrame;
        bool place = mouse.rightButton.wasPressedThisFrame;
        if (!dig && !place) return;

        Vector3Int mouseCell = Vector3Int.zero;

        // ────────── TODO 3-D ──────────
        // 算出滑鼠指著的是哪一格，存進 mouseCell。
        // 需要三步：讀滑鼠的螢幕座標 → 換成世界座標 → 換成格子座標。
        //
        // 中間那步有個 z 的陷阱，上面的說明有寫。


        // ────────── TODO 3-E ──────────
        // 按左鍵（dig）時：如果滑鼠那格是紅磚，就把它清空，並把分數 +1。
        // 只有紅磚能挖，草地和冰不行。


        // ────────── TODO 3-F ──────────
        // 按右鍵（place）時：如果滑鼠那格是空的、而且手上還有磚（分數 > 0），
        // 就在那格放一塊紅磚，並把分數 -1。

    }
}
