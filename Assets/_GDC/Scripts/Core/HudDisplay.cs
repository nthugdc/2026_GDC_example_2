using UnityEngine;
using TMPro;

/// <summary>
/// 畫面右上角的資訊列（章節名稱、分數、狀態、操作提示）。
/// 直接掛在攝影機底下的文字物件上，不需要 Canvas。
/// </summary>
[ExecuteAlways]
public class HudDisplay : MonoBehaviour
{
    [Header("固定顯示在最下方的操作提示")]
    [TextArea(1, 4)]
    [Tooltip("ChapterNavigator 在教師版場景會覆寫這段文字")]
    public string controlsHint = "R = 重玩本章   N = 下一章   P = 上一章（最後一章會繞回第 0 章）";

    TMP_Text label;

    void OnEnable() { label = GetComponent<TMP_Text>(); }

    void Update() { Refresh(); }

    /// <summary>重新組裝要顯示的內容。建置場景時也會呼叫一次。</summary>
    public void Refresh()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null) return;

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            label.text = controlsHint;
            return;
        }

        string s = gm.chapterTitle + "\n分數：" + gm.score;
        if (!string.IsNullOrEmpty(gm.statusMessage))
            s += "\n" + (gm.isCleared ? "<color=#06D6A0>" : "<color=#EF476F>") + gm.statusMessage + "</color>";
        s += "\n<size=70%><color=#8A97A6>" + controlsHint + "</color></size>";

        label.text = s;
    }
}
