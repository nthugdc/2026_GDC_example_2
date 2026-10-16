using UnityEngine;

/// <summary>
/// 簡單的特效小工具（學生不用改，直接呼叫就好）。
/// 例：GDCFx.Puff(transform.position, Color.white);
/// </summary>
public static class GDCFx
{
    static Sprite circle;

    /// <summary>在 position 冒出一團會長大、變淡、然後消失的煙。</summary>
    public static void Puff(Vector3 position, Color color, float size = 0.35f)
    {
        if (circle == null) circle = MakeCircle();

        GameObject go = new GameObject("Puff");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circle;
        sr.color = color;
        sr.sortingOrder = 20;
        PuffFade fade = go.AddComponent<PuffFade>();
        fade.startSize = size;
    }

    static Sprite MakeCircle()
    {
        const int n = 32;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            float dx = x + 0.5f - n * 0.5f, dy = y + 0.5f - n * 0.5f;
            float a = Mathf.Clamp01(n * 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
    }
}
