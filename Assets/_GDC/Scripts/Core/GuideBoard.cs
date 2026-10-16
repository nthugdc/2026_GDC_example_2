using UnityEngine;
using TMPro;

/// <summary>
/// 場景中的引導看板。改 content 後，編輯模式下也會即時更新。
/// 這是教材的基礎建設，學生不需要修改這支腳本。
/// </summary>
[ExecuteAlways]
public class GuideBoard : MonoBehaviour
{
    [Header("看板內容")]
    [TextArea(3, 20)]
    [Tooltip("要顯示在看板上的文字，支援換行")]
    public string content = "";

    [Header("外觀")]
    [Tooltip("文字顏色")]
    public Color textColor = new Color(0.96f, 0.97f, 0.98f);
    [Tooltip("文字大小")]
    public float fontSize = 2.4f;

    TMP_Text label;

    void OnEnable()  { Apply(); }
    void OnValidate(){ Apply(); }

    /// <summary>把目前設定套用到子物件的文字上。</summary>
    public void Apply()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;

        label.text     = content;
        label.color    = textColor;
        label.fontSize = fontSize;
    }
}
