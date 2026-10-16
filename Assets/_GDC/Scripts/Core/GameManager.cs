using UnityEngine;

/// <summary>
/// 每個章節場景裡的總管：記分數、記過關狀態。
/// 學生的腳本可以用 GameManager.Instance.AddScore(1) 來加分。
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>方便任何腳本存取，例如 GameManager.Instance.AddScore(1)</summary>
    public static GameManager Instance { get; private set; }

    [Header("章節資訊")]
    [Tooltip("顯示在畫面左上角的章節名稱")]
    public string chapterTitle = "第 ? 章";

    [Header("狀態（執行時會自動變動）")]
    public int score;
    public bool isCleared;
    public string statusMessage = "";

    void Awake()
    {
        Instance = this;
        score = 0;
        isCleared = false;
    }

    /// <summary>加分。</summary>
    public void AddScore(int amount)
    {
        score += amount;
    }

    /// <summary>過關！只會生效一次。</summary>
    public void Clear(string message = "過關！")
    {
        if (isCleared) return;
        isCleared = true;
        statusMessage = message;
        Debug.Log("[GDC] " + chapterTitle + " " + message);
    }

    /// <summary>失敗訊息（不會鎖定狀態，可重複顯示）。</summary>
    public void Fail(string message)
    {
        if (isCleared) return;
        statusMessage = message;
    }
}
