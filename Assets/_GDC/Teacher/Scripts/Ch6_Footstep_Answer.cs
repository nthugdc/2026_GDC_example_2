using UnityEngine;

/// <summary>
/// 第 6 章參考做法（教師版場景使用）：給 Animation Event 呼叫的腳步聲效果。
/// 說明見 Teacher/Answers/Ch6_Footstep.txt
/// </summary>
public class Ch6_Footstep_Answer : MonoBehaviour
{
    public Color dustColor = new Color(0.9f, 0.85f, 0.75f, 0.8f);

    HeroMove move;

    void Awake()
    {
        move = GetComponent<HeroMove>();
    }

    // 名字要跟動畫事件的 Function 欄位一模一樣，而且要是 public
    public void Footstep()
    {
        if (move != null && !move.IsGrounded()) return;   // 空中不冒煙
        GDCFx.Puff(move.FeetPosition() + Vector3.up * 0.1f, dustColor, 0.25f);
    }
}
