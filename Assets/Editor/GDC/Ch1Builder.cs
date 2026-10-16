using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 1 章 · Tile Palette：自己畫地圖、加上碰撞。</summary>
    public static class Ch1Builder
    {
        const string BoardText =
@"第 1 章 · 用 Tile Palette 畫地圖
角色一直往下掉？因為地面沒有碰撞！
1. 選 Grid 底下的 Ground，Add Component：
   ・Tilemap Collider 2D，Composite
     Operation 改成 Merge
   ・Composite Collider 2D（會自動多一個
     Rigidbody 2D，Body Type 要改 Static）
2. Window > 2D > Tile Palette
   選 Palette_Basic，Active Tilemap 選 Ground
3. 筆刷 (B) 把坑補起來
4. 橡皮擦 (D) 擦掉擋路的高牆
5. 方框 (U) 疊出階梯，爬上終點";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.6f, 2.7f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            // 學生版的地面故意沒有碰撞，第一個任務就是加上去
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, answer, layer: "Ground");

            TileBase stone = TileFactory.Get("Stone");
            TileBase brick = TileFactory.Get("Brick");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 48, 12);

            GDCSceneKit.Fill(ground, stone, -12, -3, 11, -1);
            GDCSceneKit.Fill(ground, stone, 17, -3, 46, -1);
            GDCSceneKit.Fill(ground, stone, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, stone, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, stone, 31, 0, 46, 3);     // 終點平台，比地面高 4 格

            if (answer)
            {
                GDCSceneKit.Fill(ground, brick, 12, -1, 16, -1);  // 補好的坑
                for (int i = 0; i < 4; i++)                       // 階梯
                    GDCSceneKit.Fill(ground, brick, 27 + i, 0, 27 + i, i);
            }
            else
            {
                GDCSceneKit.Fill(ground, stone, 22, 0, 23, 5);    // 擋路的高牆（跳不過去）
            }

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(40f, 5.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！你畫出了自己的路", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 1 章 · Tile Palette");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch1_Palette", answer));
        }
    }
}
