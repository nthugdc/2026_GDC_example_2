using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>
    /// 產生磁磚圖、Tile / RuleTile / AnimatedTile 資產與 Tile Palette。
    /// 磁磚是彩色的（顏色來自 GDCPalette，產圖時燒進 PNG），每格 128 px = 1 單位。
    /// 資產已存在時就地更新，GUID 不變，場景裡的參照不會斷。
    /// </summary>
    public static class TileFactory
    {
        public const int PX = 128;

        // 給章節建置腳本用的資產路徑
        public static string TilePath(string name) => $"{GDCPaths.TileAssets}/{name}.asset";
        public static TileBase Get(string name) => GDCPaths.Load<TileBase>(TilePath(name));

        public static void Run()
        {
            GDCPaths.EnsureFolders();
            MakePhysicsMaterial();

            // ── 一般磁磚 ──
            Tile stone   = MakeTile("Stone",    DrawStone(),    Tile.ColliderType.Grid);
            Tile brick   = MakeTile("Brick",    DrawBrick(),    Tile.ColliderType.Grid);
            Tile ice     = MakeTile("Ice",      DrawIce(),      Tile.ColliderType.Grid);
            Tile wall    = MakeTile("BackWall", DrawBackWall(), Tile.ColliderType.None);
            Tile bush    = MakeTile("Bush",     DrawBush(),     Tile.ColliderType.None);
            Tile tuft    = MakeTile("Tuft",     DrawTuft(),     Tile.ColliderType.None);

            // ── 動畫磁磚：岩漿 ──
            var lavaFrames = new Sprite[4];
            for (int k = 0; k < 4; k++)
                lavaFrames[k] = SaveSprite($"Lava_{k}", DrawLava(k));
            AnimatedTile lava      = MakeAnimated("Lava",     lavaFrames, 4f);
            AnimatedTile lavaFrozen= MakeAnimated("Lava_Ch2", lavaFrames, 0f);   // 第 2 章：學生要自己調速度

            // ── 規則磁磚：草地 ──
            var ground = new Dictionary<string, Sprite>();
            foreach (bool t in new[] { false, true })
            foreach (bool l in new[] { false, true })
            foreach (bool r in new[] { false, true })
            {
                string key = GroundKey(t, l, r);
                ground[key] = SaveSprite("Ground_" + key, DrawGround(t, l, r));
            }
            RuleTile groundRule   = MakeGroundRule("Ground",     ground, full: true);
            RuleTile groundCh2    = MakeGroundRule("Ground_Ch2", ground, full: false);

            // ── 調色盤（Tile Palette）──
            MakePalette("Palette_Basic", new TileBase[] { stone, brick, wall });
            MakePalette("Palette_Ch2",   new TileBase[] { groundCh2, lavaFrozen, bush, tuft, stone });
            MakePalette("Palette_All",   new TileBase[] { groundRule, stone, brick, ice, lava, wall, bush, tuft });

            AssetDatabase.SaveAssets();
            Debug.Log("[GDC] 磁磚、規則磁磚、動畫磁磚與調色盤產生完成");
        }

        // ── 資產建立 ─────────────────────────────────────────────────────

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        static Sprite SaveSprite(string name, Painter p)
        {
            string path = $"{GDCPaths.Tiles}/{name}.png";
            p.Save(path, PX);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Tile MakeTile(string name, Painter p, Tile.ColliderType collider)
        {
            Tile t = LoadOrCreate<Tile>(TilePath(name));
            t.sprite = SaveSprite(name, p);
            t.colliderType = collider;
            t.color = Color.white;
            EditorUtility.SetDirty(t);
            return t;
        }

        static AnimatedTile MakeAnimated(string name, Sprite[] frames, float speed)
        {
            AnimatedTile t = LoadOrCreate<AnimatedTile>(TilePath(name));
            t.m_AnimatedSprites = frames;
            t.m_MinSpeed = speed;
            t.m_MaxSpeed = speed;
            t.m_TileColliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(t);
            return t;
        }

        static string GroundKey(bool top, bool left, bool right)
        {
            string k = (top ? "T" : "") + (left ? "L" : "") + (right ? "R" : "");
            return k == "" ? "Inner" : k;
        }

        /// <summary>
        /// 草地規則：只看上、左、右三個鄰居（下面不管）。
        /// 規則由上往下比對，第一條符合的生效，所以「條件越多」的要排越前面。
        /// full = false 時拿掉左右邊緣的規則，留給第 2 章的學生自己補。
        /// </summary>
        static RuleTile MakeGroundRule(string name, Dictionary<string, Sprite> s, bool full)
        {
            RuleTile rt = LoadOrCreate<RuleTile>(TilePath(name));
            rt.m_DefaultSprite = s["Inner"];
            rt.m_DefaultColliderType = Tile.ColliderType.Grid;
            rt.m_TilingRules = new List<RuleTile.TilingRule>();

            // (上, 左, 右)：1 = 有同種磁磚(This)，2 = 沒有(NotThis)，0 = 不管
            AddRule(rt, s["TLR"], 2, 2, 2);
            AddRule(rt, s["TL"],  2, 2, 0);
            AddRule(rt, s["TR"],  2, 0, 2);
            AddRule(rt, s["T"],   2, 0, 0);
            if (full)
            {
                AddRule(rt, s["LR"], 0, 2, 2);
                AddRule(rt, s["L"],  0, 2, 0);
                AddRule(rt, s["R"],  0, 0, 2);
            }
            EditorUtility.SetDirty(rt);
            return rt;
        }

        static void AddRule(RuleTile rt, Sprite sprite, int up, int left, int right)
        {
            var rule = new RuleTile.TilingRule();
            rule.m_Sprites = new[] { sprite };
            rule.m_ColliderType = Tile.ColliderType.Grid;
            rule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single;
            rule.m_NeighborPositions = new List<Vector3Int>();
            rule.m_Neighbors = new List<int>();
            void N(Vector3Int pos, int v) { if (v == 0) return; rule.m_NeighborPositions.Add(pos); rule.m_Neighbors.Add(v); }
            N(Vector3Int.up, up);
            N(Vector3Int.left, left);
            N(Vector3Int.right, right);
            rt.m_TilingRules.Add(rule);
        }

        static void MakePalette(string name, TileBase[] tiles)
        {
            string path = $"{GDCPaths.Palettes}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                GridPaletteUtility.CreateNewPalette(GDCPaths.Palettes, name, GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            Tilemap tm = root.GetComponentInChildren<Tilemap>();
            tm.ClearAllTiles();
            for (int i = 0; i < tiles.Length; i++)
                tm.SetTile(new Vector3Int(i % 4, -(i / 4), 0), tiles[i]);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void MakePhysicsMaterial()
        {
            string path = $"{GDCPaths.PhysicsMats}/Slippery.physicsMaterial2D";
            var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (m == null) { m = new PhysicsMaterial2D("Slippery"); AssetDatabase.CreateAsset(m, path); }
            m.friction = 0f;      // 角色貼著牆壁時才不會卡住
            m.bounciness = 0f;
            EditorUtility.SetDirty(m);
        }

        // ── 繪圖 ─────────────────────────────────────────────────────────

        static Color Shade(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);
        static Color A(Color c, float a) => new Color(c.r, c.g, c.b, a);
        const float TAU = Mathf.PI * 2f;

        static Painter DrawGround(bool top, bool left, bool right)
        {
            var p = new Painter(PX, PX);
            Color dirt = GDCPalette.Dirt;
            p.Rect(0, 0, PX, PX, dirt);

            // 泥土斑點（位置固定，拼起來才一致）
            float[,] dots = { { 22, 30, 6 }, { 70, 18, 5 }, { 104, 52, 7 }, { 40, 70, 5 }, { 88, 86, 6 }, { 16, 100, 4 }, { 60, 44, 4 } };
            for (int i = 0; i < dots.GetLength(0); i++)
                p.Circle(dots[i, 0], dots[i, 1], dots[i, 2], Shade(dirt, 0.8f));

            Color edge = A(GDCPalette.Outline, 0.6f);
            if (left)  p.Rect(0, 0, 8, PX, edge);
            if (right) p.Rect(PX - 8, 0, PX, PX, edge);

            if (top)
            {
                // 週期 64 px 的波浪，左右拼接剛好接得起來
                Color grass = GDCPalette.Grass;
                p.Fill(0, 70, PX, PX, (x, y) => y > 96f + 6f * Mathf.Sin(x / 64f * TAU), grass);
                p.Rect(0, PX - 10, PX, PX, Shade(grass, 1.15f));
                p.Fill(0, 70, PX, PX, (x, y) => y > 92f + 6f * Mathf.Sin(x / 64f * TAU) && y <= 96f + 6f * Mathf.Sin(x / 64f * TAU), Shade(grass, 0.7f));
                if (left)  p.Rect(0, 90, 8, PX, Shade(grass, 0.6f));
                if (right) p.Rect(PX - 8, 90, PX, PX, Shade(grass, 0.6f));
            }
            return p;
        }

        static Painter DrawStone()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Stone;
            p.Rect(0, 0, PX, PX, Shade(c, 0.7f));
            p.Rect(0, 10, PX - 10, PX, Shade(c, 1.2f));
            p.Rect(10, 10, PX - 10, PX - 10, c);
            p.Circle(40, 46, 6, Shade(c, 0.85f));
            p.Circle(86, 80, 4, Shade(c, 0.85f));
            return p;
        }

        static Painter DrawBrick()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Brick;
            p.Rect(0, 0, PX, PX, Shade(c, 0.5f));      // 灰縫
            for (int row = 0; row < 4; row++)
            {
                float y0 = row * 32 + 3, y1 = row * 32 + 29;
                float off = (row % 2 == 0) ? 0 : 32;
                for (int col = -1; col < 3; col++)
                {
                    float x0 = col * 64 + off + 3, x1 = col * 64 + off + 61;
                    p.Rect(x0, y0, x1, y1, c);
                    p.Rect(x0, y1 - 5, x1, y1, Shade(c, 1.2f));
                }
            }
            return p;
        }

        static Painter DrawIce()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Ice;
            p.Rect(0, 0, PX, PX, c);
            p.Rect(0, 0, PX, 8, Shade(c, 0.75f));
            p.Rect(0, PX - 10, PX, PX, Color.Lerp(c, Color.white, 0.6f));
            Color glint = new Color(1f, 1f, 1f, 0.45f);
            p.Polygon(new[] { new Vector2(20, 30), new Vector2(34, 30), new Vector2(70, 100), new Vector2(56, 100) }, glint);
            p.Polygon(new[] { new Vector2(66, 24), new Vector2(74, 24), new Vector2(100, 76), new Vector2(92, 76) }, glint);
            return p;
        }

        static Painter DrawLava(int k)
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Lava, hot = GDCPalette.LavaHot;
            p.Rect(0, 0, PX, PX, Shade(c, 0.85f));
            float ph = k / 4f;
            p.Fill(0, 0, PX, PX, (x, y) => y < 100f + 7f * Mathf.Sin((x / 64f + ph) * TAU), c);
            p.Fill(0, 80, PX, PX, (x, y) =>
            {
                float w = 100f + 7f * Mathf.Sin((x / 64f + ph) * TAU);
                return y < w && y > w - 10f;
            }, hot);
            // 泡泡：每一幀往上飄一點
            float[,] b = { { 30, 20, 7 }, { 84, 50, 5 }, { 58, 4, 6 } };
            for (int i = 0; i < b.GetLength(0); i++)
                p.Circle(b[i, 0], (b[i, 1] + k * 18) % 90, b[i, 2], A(hot, 0.85f));
            return p;
        }

        static Painter DrawBackWall()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.BackWall;
            p.Rect(0, 0, PX, PX, Shade(c, 0.8f));
            for (int row = 0; row < 2; row++)
            {
                float off = row == 0 ? 0 : 32;
                for (int col = -1; col < 2; col++)
                    p.Rect(col * 64 + off + 3, row * 64 + 3, col * 64 + off + 61, row * 64 + 61, c);
            }
            return p;
        }

        static Painter DrawBush()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Leaf;
            float[,] blobs = { { 0, 34, 34 }, { 40, 44, 36 }, { 86, 38, 34 }, { 128, 34, 34 }, { 64, 20, 30 } };
            for (int i = 0; i < blobs.GetLength(0); i++)
                p.Circle(blobs[i, 0], blobs[i, 1], blobs[i, 2], Shade(c, 0.8f));
            for (int i = 0; i < blobs.GetLength(0); i++)
                p.Circle(blobs[i, 0], blobs[i, 1] + 4, blobs[i, 2] - 8, c);
            p.Rect(0, 0, PX, 14, Shade(c, 0.8f));
            return p;
        }

        static Painter DrawTuft()
        {
            var p = new Painter(PX, PX);
            Color c = GDCPalette.Grass;
            float[] xs = { 30, 44, 58, 80, 96 };
            float[] hs = { 26, 40, 30, 36, 24 };
            for (int i = 0; i < xs.Length; i++)
                p.Polygon(new[] { new Vector2(xs[i] - 6, 0), new Vector2(xs[i] + 6, 0), new Vector2(xs[i] + 2 * (i % 2 == 0 ? 1 : -1), hs[i]) }, Shade(c, 0.9f + 0.05f * i));
            p.Circle(70, 30, 5, GDCPalette.Coin);
            return p;
        }
    }
}
