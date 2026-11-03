using UnityEngine;

/// <summary>
/// 打得碎的木箱（第 6 章起）。學生不用改。
/// 你的攻擊打到它時，呼叫 Break() 就會冒煙、消失、加一分。
/// </summary>
public class Breakable : MonoBehaviour
{
    [Tooltip("碎掉時冒出的煙的顏色")]
    public Color dustColor = new Color(0.79f, 0.55f, 0.37f, 0.9f);

    bool broken;

    /// <summary>把這個木箱打碎。</summary>
    public void Break()
    {
        if (broken) return;
        broken = true;
        GDCFx.Puff(transform.position, dustColor, 0.7f);
        if (GameManager.Instance != null) GameManager.Instance.AddScore(1);
        Destroy(gameObject);
    }
}
