using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 第 5 章練習：讓這個物件（HandTarget）跟著滑鼠走。
/// IK 會把角色的手伸向 HandTarget，所以它跟著滑鼠，手就跟著滑鼠。
///
/// ═══ 滑鼠位置 ═══
///
///   Mouse.current                        目前的滑鼠，沒接滑鼠時是 null
///   Mouse.current.position.ReadValue()   滑鼠在螢幕上的位置（Vector2，單位是像素，左下角是 0,0）
///
/// ═══ Camera.ScreenToWorldPoint ═══
///
///   Vector3 ScreenToWorldPoint(Vector3 screenPosition)
///
///     screenPosition   螢幕座標。Vector2 可以直接傳進去（會自動補 z = 0）
///     回傳             對應的世界座標
///
///   ⚠ 回傳的 z 是攝影機附近的值（大約 -10），不是 0。
///     2D 遊戲的東西都在 z = 0，物件被放到 z = -10 會跑到攝影機後面，畫面上就看不到了。
///
/// ═══ Vector3.Lerp ═══
///
///   Vector3 Lerp(Vector3 a, Vector3 b, float t)
///
///     回傳 a 和 b 之間的一點。t = 0 是 a，t = 1 是 b，t = 0.2 是「從 a 往 b 走 20%」。
///     每一幀都往目標走一小段，就會有「慢慢跟上」的平滑效果。
/// </summary>
public class Ch5_MouseTarget : MonoBehaviour
{
    [Tooltip("跟上滑鼠的速度。越大越即時，越小越飄")]
    public float followSpeed = 15f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        Vector3 goal = transform.position;

        // ────────── TODO 5-A ──────────
        // 讀出滑鼠的螢幕座標，換成世界座標，存進 goal。
        // 記得處理 z（上面的說明有寫為什麼）。


        // ────────── TODO 5-B ──────────
        // 讓這個物件往 goal 移動。
        //
        // 先直接把位置設成 goal 試試看，可以動了再改成平滑跟隨：
        // 每一幀走 followSpeed * Time.deltaTime 這個比例。

    }
}
