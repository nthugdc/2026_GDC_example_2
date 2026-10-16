using UnityEngine;

/// <summary>碰到就送回起點（岩漿、尖刺）。掛在有 Trigger Collider 的物件或 Tilemap 上。學生不用改。</summary>
public class KillZone : MonoBehaviour
{
    [Tooltip("碰到時 HUD 顯示的訊息")]
    public string message = "碰到岩漿了！回到起點";

    void OnTriggerEnter2D(Collider2D other)
    {
        Respawner r = other.GetComponentInParent<Respawner>();
        if (r == null) return;
        r.Respawn();
        if (GameManager.Instance != null) GameManager.Instance.Fail(message);
    }
}
