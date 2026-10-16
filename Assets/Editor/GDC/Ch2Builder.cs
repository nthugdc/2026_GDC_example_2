using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 2 章 · Rule Tile、Animated Tile 與多個圖層。</summary>
    public static class Ch2Builder
    {
        const string BoardText =
@"第 2 章 · Rule Tile 與動畫磁磚
1. 岩漿不會動？在 Project 選 _GDC/Tiles/
   Lava_Ch2，Min / Max Speed 都改成 4
2. Tile Palette 選 Palette_Ch2，Active
   Tilemap 選 Ground，用草地在岩漿上畫幾塊
   踏腳石 —— 草皮會自動長在上面
3. 草地的左右兩側沒有黑邊？選 Ground_Ch2，
   Tiling Rules 按 + 新增兩條規則：
   ・左邊格子設成「X」，圖用 Ground_L
   ・右邊格子設成「X」，圖用 Ground_R
4. 走到樹叢後面看看：Foreground 圖層的
   Order in Layer 比玩家大，所以擋在前面";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.8f, 2.7f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap hazard = GDCSceneKit.Layer(grid, "Hazard", GDCSceneKit.OrderHazard, true, trigger: true, layer: "Hazard");
            Tilemap decor  = GDCSceneKit.Layer(grid, "Decor", GDCSceneKit.OrderDecor, false);
            Tilemap front  = GDCSceneKit.Layer(grid, "Foreground", GDCSceneKit.OrderForeground, false);
            hazard.gameObject.AddComponent<KillZone>();

            TileBase grass = TileFactory.Get(answer ? "Ground" : "Ground_Ch2");
            TileBase lava  = TileFactory.Get(answer ? "Lava" : "Lava_Ch2");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 48, 12);

            GDCSceneKit.Fill(ground, grass, -12, -3, 10, -1);
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 3, 0, 5, 0);        // 小山丘
            GDCSceneKit.Fill(ground, grass, 7, 0, 7, 0);        // 一格寬的柱子（離岩漿遠一點，跳過去才不會直接掉進去）
            GDCSceneKit.Fill(ground, grass, 11, -3, 20, -2);    // 岩漿池底
            GDCSceneKit.Fill(ground, grass, 21, -3, 46, -1);
            GDCSceneKit.Fill(ground, grass, 27, 0, 31, 1);
            GDCSceneKit.Fill(hazard, lava, 11, -1, 20, -1);     // 10 格寬，跳不過去

            if (answer)
            {
                // 全速跳一次大約前進 4 格，踏腳石放在落地點附近
                GDCSceneKit.Fill(ground, grass, 13, 0, 15, 0);
                GDCSceneKit.Fill(ground, grass, 19, 0, 20, 0);
            }

            TileBase bush = TileFactory.Get("Bush");
            GDCSceneKit.Fill(front, bush, 0, 0, 1, 0);
            GDCSceneKit.Fill(front, bush, 23, 0, 25, 0);
            GDCSceneKit.Row(decor, TileFactory.Get("Tuft"), 32, 0, "x   x  x");

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(40f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！Rule Tile 幫你自動接好邊了", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 2 章 · Rule Tile");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch2_RuleTile", answer));
        }
    }
}
