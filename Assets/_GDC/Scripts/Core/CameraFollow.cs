using UnityEngine;

/// <summary>攝影機平滑跟隨目標，並限制在指定範圍內。</summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟隨對象")]
    public Transform target;

    [Header("跟隨設定")]
    [Tooltip("數字越小跟得越慢、越滑順")]
    public float smooth = 5f;
    [Tooltip("相對目標的位移")]
    public Vector2 offset = new Vector2(0f, 1f);
    [Tooltip("取消勾選的話，攝影機的高度就固定不動")]
    public bool followY = true;

    [Header("水平移動範圍（min 與 max 相同表示不限制）")]
    public Vector2 min = new Vector2(0f, 0f);
    public Vector2 max = new Vector2(0f, 0f);

    float fixedY;

    void Awake() { fixedY = transform.position.y; }

    void LateUpdate()
    {
        if (target == null) return;

        float wantY = followY ? target.position.y + offset.y : fixedY;
        Vector3 want = new Vector3(target.position.x + offset.x, wantY, transform.position.z);
        if (!Mathf.Approximately(min.x, max.x)) want.x = Mathf.Clamp(want.x, min.x, max.x);
        if (followY && !Mathf.Approximately(min.y, max.y)) want.y = Mathf.Clamp(want.y, min.y, max.y);

        transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
