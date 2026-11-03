using UnityEngine;
using TMPro;

/// <summary>
/// 角色頭上的小字，用來顯示目前的 State（第 7 章）。學生不用改。
/// 呼叫 Set("Run") 就會顯示 Run。
/// </summary>
public class StateLabel : MonoBehaviour
{
    TMP_Text label;

    void Awake() { label = GetComponent<TMP_Text>(); }

    /// <summary>改變顯示的文字。</summary>
    public void Set(string text)
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label != null) label.text = text;
    }

    // 角色左右翻轉時，文字不要跟著鏡像
    void LateUpdate() { transform.rotation = Quaternion.identity; }
}
