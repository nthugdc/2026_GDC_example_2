using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GDCBuild
{
    /// <summary>
    /// 程序繪圖用的小畫布：彩色、有抗鋸齒（4×4 超取樣），原點在左下角、單位是像素。
    /// 每個形狀都帶一個外框範圍，只在範圍內取樣，大張圖也畫得快。
    /// </summary>
    public class Painter
    {
        const int SS = 4;

        public readonly int W, H;
        readonly Color[] px;

        public Painter(int w, int h)
        {
            W = w; H = h;
            px = new Color[w * h];   // 全透明
        }

        // ── 基本：任意形狀 ───────────────────────────────────────────────

        /// <summary>在 [x0,x1)×[y0,y1) 範圍內，把 inside 為 true 的部分塗上 c（alpha 疊加）。</summary>
        public void Fill(float x0, float y0, float x1, float y1, Func<float, float, bool> inside, Color c)
        {
            int ix0 = Mathf.Max(0, Mathf.FloorToInt(x0)), iy0 = Mathf.Max(0, Mathf.FloorToInt(y0));
            int ix1 = Mathf.Min(W, Mathf.CeilToInt(x1)), iy1 = Mathf.Min(H, Mathf.CeilToInt(y1));
            const float inv = 1f / (SS * SS);

            for (int y = iy0; y < iy1; y++)
            for (int x = ix0; x < ix1; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                for (int sx = 0; sx < SS; sx++)
                    if (inside(x + (sx + 0.5f) / SS, y + (sy + 0.5f) / SS)) hit++;
                if (hit == 0) continue;
                Blend(x, y, c, hit * inv);
            }
        }

        void Blend(int x, int y, Color c, float cover)
        {
            int i = y * W + x;
            float a = c.a * cover;
            Color d = px[i];
            float outA = a + d.a * (1f - a);
            if (outA <= 0f) return;
            Color o = (c * a + d * d.a * (1f - a)) / outA;
            o.a = outA;
            px[i] = o;
        }

        // ── 常用形狀 ─────────────────────────────────────────────────────

        public void Rect(float x0, float y0, float x1, float y1, Color c)
            => Fill(x0, y0, x1, y1, (x, y) => x >= x0 && x < x1 && y >= y0 && y < y1, c);

        public void Circle(float cx, float cy, float r, Color c)
            => Fill(cx - r, cy - r, cx + r, cy + r, (x, y) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r, c);

        /// <summary>膠囊：線段 a→b 加上半徑 r。畫四肢最好用。</summary>
        public void Capsule(Vector2 a, Vector2 b, float r, Color c)
        {
            float x0 = Mathf.Min(a.x, b.x) - r, x1 = Mathf.Max(a.x, b.x) + r;
            float y0 = Mathf.Min(a.y, b.y) - r, y1 = Mathf.Max(a.y, b.y) + r;
            Fill(x0, y0, x1, y1, (x, y) => SegDist(new Vector2(x, y), a, b) <= r, c);
        }

        public void Polygon(Vector2[] p, Color c)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (Vector2 v in p) { x0 = Mathf.Min(x0, v.x); y0 = Mathf.Min(y0, v.y); x1 = Mathf.Max(x1, v.x); y1 = Mathf.Max(y1, v.y); }
            Fill(x0, y0, x1, y1, (x, y) => InPolygon(x, y, p), c);
        }

        public static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude < 1e-6f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

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

        public float AlphaAt(int x, int y) => (x < 0 || y < 0 || x >= W || y >= H) ? 0f : px[y * W + x].a;

        /// <summary>把另一張畫布整張貼到 (ox, oy)。</summary>
        public void Blit(Painter src, int ox, int oy)
        {
            for (int y = 0; y < src.H; y++)
            for (int x = 0; x < src.W; x++)
            {
                Color c = src.px[y * src.W + x];
                if (c.a <= 0f) continue;
                int tx = x + ox, ty = y + oy;
                if (tx < 0 || ty < 0 || tx >= W || ty >= H) continue;
                Blend(tx, ty, new Color(c.r, c.g, c.b, 1f), c.a);
            }
        }

        // ── 存檔 ─────────────────────────────────────────────────────────

        /// <summary>存成 PNG 並匯入成 sprite。mode = Single 或 Multiple（Multiple 的切圖另外做）。</summary>
        public void Save(string assetPath, int ppu, SpriteImportMode mode = SpriteImportMode.Single,
                         SpriteMeshType mesh = SpriteMeshType.FullRect, bool readable = false)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            string abs = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllBytes(abs, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var ti = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = mode;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode          = FilterMode.Bilinear;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            ti.wrapMode            = TextureWrapMode.Clamp;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.isReadable          = readable;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteMeshType = mesh;
            s.spriteGenerateFallbackPhysicsShape = false;
            ti.SetTextureSettings(s);
            ti.SaveAndReimport();
        }
    }
}
