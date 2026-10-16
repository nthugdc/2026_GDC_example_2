using UnityEngine;
using TMPro;

/// <summary>過關時在畫面中央顯示的大字橫幅，平常是隱藏的。</summary>
[ExecuteAlways]
public class ClearBanner : MonoBehaviour
{
    TMP_Text label;

    void OnEnable() { label = GetComponent<TMP_Text>(); }

    void Update()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null) return;

        GameManager gm = GameManager.Instance;
        label.text = (gm != null && gm.isCleared) ? gm.statusMessage : "";
    }
}
