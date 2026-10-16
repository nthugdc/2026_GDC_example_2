using UnityEngine;

/// <summary>指定的物件碰到這個區域就過關。用直接指定物件，不依賴 Tag。</summary>
[RequireComponent(typeof(Collider2D))]
public class TouchGoal : MonoBehaviour
{
    [Header("過關條件")]
    [Tooltip("碰到這裡就算過關的物件")]
    public Rigidbody2D targetBody;

    [Header("過關訊息")]
    public string clearMessage = "過關！";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetBody == null || other.attachedRigidbody != targetBody) return;
        if (GameManager.Instance != null) GameManager.Instance.Clear(clearMessage);
    }
}
