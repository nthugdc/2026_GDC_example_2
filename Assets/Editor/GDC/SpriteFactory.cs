using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GDCBuild
{
    /// <summary>
    /// 程序生成教材用的幾何 sprite。
    /// 全部畫成「純白 + 透明度」，實際顏色由 SpriteRenderer.color 決定，
    /// 所以換配色不必重新產圖。
    /// </summary>
    public static class SpriteFactory
    {
        const int PPU = 128;   // 128 像素 = 1 Unity 單位
        const int SS  = 4;     // 每個像素做 4x4 超取樣，邊緣才不會有鋸齒

        public static void Run()
        {
            GDCPaths.EnsureFolders();

            Make("Square",        128, 128, (x, y, w, h) => true);
            Make("Circle",        128, 128, Circle);
            Make("Ring",          128, 128, Ring);
            Make("RoundedSquare", 128, 128, (x, y, w, h) => RoundedRect(x, y, w, h, 22f));
            Make("Capsule",       128, 192, (x, y, w, h) => RoundedRect(x, y, w, h, w * 0.5f));
            Make("Triangle",      128, 128, Triangle);
            Make("Star",          128, 128, Star);

            AssetDatabase.Refresh();
            Debug.Log("[GDC] 美術素材產生完成：7 個 sprite");
        }

        // ── 形狀定義（座標為像素，原點在左下角）────────────────────────────

        static bool Circle(float x, float y, float w, float h)
        {
            float r = w * 0.5f;
            float dx = x - w * 0.5f, dy = y - h * 0.5f;
            return dx * dx + dy * dy <= r * r;
        }

        static bool Ring(float x, float y, float w, float h)
        {
            float ro = w * 0.5f, ri = w * 0.34f;
            float dx = x - w * 0.5f, dy = y - h * 0.5f;
            float d2 = dx * dx + dy * dy;
            return d2 <= ro * ro && d2 >= ri * ri;
        }

        /// <summary>圓角矩形：把點推回「內縮方框」再比對圓角半徑。</summary>
        static bool RoundedRect(float x, float y, float w, float h, float radius)
        {
            float dx = Mathf.Max(Mathf.Abs(x - w * 0.5f) - (w * 0.5f - radius), 0f);
            float dy = Mathf.Max(Mathf.Abs(y - h * 0.5f) - (h * 0.5f - radius), 0f);
            return dx * dx + dy * dy <= radius * radius;
        }

        static bool Triangle(float x, float y, float w, float h)
            => InPolygon(x, y, new[] { new Vector2(0, 0), new Vector2(w, 0), new Vector2(w * 0.5f, h) });

        static bool Star(float x, float y, float w, float h)
        {
            var pts = new Vector2[10];
            float cx = w * 0.5f, cy = h * 0.5f;
            float ro = w * 0.5f, ri = w * 0.21f;
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI / 2f + i * Mathf.PI / 5f;   // 從正上方開始
                float r = (i % 2 == 0) ? ro : ri;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            return InPolygon(x, y, pts);
        }

        /// <summary>射線法判斷點是否在多邊形內。</summary>
        static bool InPolygon(float x, float y, Vector2[] p)
        {
            bool inside = false;
            for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
            {
                if ((p[i].y > y) != (p[j].y > y) &&
                    x < (p[j].x - p[i].x) * (y - p[i].y) / (p[j].y - p[i].y) + p[i].x)
                    inside = !inside;
            }
            return inside;
        }

        // ── 產圖與匯入設定 ──────────────────────────────────────────────

        static void Make(string name, int w, int h, Func<float, float, float, float, bool> inside)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            float inv = 1f / (SS * SS);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                for (int sx = 0; sx < SS; sx++)
                    if (inside(x + (sx + 0.5f) / SS, y + (sy + 0.5f) / SS, w, h)) hit++;

                px[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(hit * inv * 255f));
            }

            tex.SetPixels32(px);
            tex.Apply();

            string path = $"{GDCPaths.Sprites}/{name}.png";
            File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName, path),
                               tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Configure(path);
        }

        static void Configure(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;

            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PPU;
            ti.filterMode          = FilterMode.Bilinear;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            ti.wrapMode            = TextureWrapMode.Clamp;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;   // 保持完整矩形，縮放才不會走鐘
            s.spriteGenerateFallbackPhysicsShape = false;
            ti.SetTextureSettings(s);

            ti.SaveAndReimport();
        }
    }
}
