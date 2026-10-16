using UnityEngine;

/// <summary>
/// 全專案共用配色。改這裡就能一次換掉所有顏色。
/// 磁磚和角色的顏色是產圖時「燒」進 PNG 的，改完要重跑「GDC/建置/2. 產生美術素材」。
/// </summary>
public static class GDCPalette
{
    public static readonly Color Background = Hex("#1B2430"); // 背景
    public static readonly Color Platform   = Hex("#4A5A6A"); // 一般平台
    public static readonly Color Player     = Hex("#FFD166"); // 玩家
    public static readonly Color Coin       = Hex("#FFC93C"); // 金幣、星星
    public static readonly Color Hazard     = Hex("#EF476F"); // 危險物
    public static readonly Color Goal       = Hex("#06D6A0"); // 目標
    public static readonly Color Text       = Hex("#F5F7FA"); // 一般文字
    public static readonly Color TextDim    = Hex("#8A97A6"); // 次要文字
    public static readonly Color Board      = Hex("#2F3D4F"); // 看板底色

    // ── 磁磚 ──
    public static readonly Color Grass      = Hex("#7BC96F"); // 草地表層
    public static readonly Color Dirt       = Hex("#8B6B4E"); // 泥土
    public static readonly Color Stone      = Hex("#7A8798"); // 石磚
    public static readonly Color Brick      = Hex("#C0694E"); // 可挖的紅磚
    public static readonly Color Ice        = Hex("#8FD3F4"); // 冰
    public static readonly Color Lava       = Hex("#FF7B39"); // 岩漿
    public static readonly Color LavaHot    = Hex("#FFD23F"); // 岩漿亮部
    public static readonly Color BackWall   = Hex("#2A3646"); // 背景牆
    public static readonly Color Leaf       = Hex("#3E9E5B"); // 前景草叢

    // ── 角色 ──
    public static readonly Color Skin       = Hex("#F6C89F");
    public static readonly Color Shirt      = Hex("#3A86FF");
    public static readonly Color Pants      = Hex("#33415C");
    public static readonly Color Hair       = Hex("#5C3B28");
    public static readonly Color Outline    = Hex("#1B1B24");

    public static Color Hex(string hex)
    {
        Color c;
        return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
    }
}
