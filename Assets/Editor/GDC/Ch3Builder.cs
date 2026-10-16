using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 3 章 · 用程式讀寫 Tilemap：踩到什麼、挖磚、放磚。</summary>
    public static class Ch3Builder
    {
        const string BoardText =
@"第 3 章 · 用程式讀寫 Tilemap
打開 Scripts/Student/Ch3_TileProbe.cs
照順序完成 TODO：
3-A 算出腳下是哪一格、放的是什麼
3-B 踩在冰上變滑
3-C 踩到岩漿回起點
3-D 算出滑鼠指著哪一格
3-E 左鍵挖紅磚（磚數 +1）
3-F 右鍵放紅磚（磚數 -1）
HUD 的「分數」就是你手上的磚塊數";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 8.8f, 2.8f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap decor  = GDCSceneKit.Layer(grid, "Decor", GDCSceneKit.OrderDecor, false);

            TileBase grass = TileFactory.Get("Ground");
            TileBase ice   = TileFactory.Get("Ice");
            TileBase lava  = TileFactory.Get("Lava");
            TileBase brick = TileFactory.Get("Brick");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 56, 12);

            GDCSceneKit.Fill(ground, grass, -12, -3, 8, -1);
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 55, -3, 56, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 9, -3, 20, -2);
            GDCSceneKit.Fill(ground, ice, 9, -1, 17, -1);        // 冰面
            GDCSceneKit.Fill(ground, lava, 18, -1, 20, -1);      // 岩漿（在 Ground 圖層，靠程式判斷）
            GDCSceneKit.Fill(ground, grass, 21, -3, 30, -1);
            GDCSceneKit.Fill(ground, brick, 25, 0, 26, 4);       // 紅磚牆：要挖
            // 31 ~ 36 是深谷：要放磚搭橋
            GDCSceneKit.Fill(ground, grass, 37, -3, 54, -1);
            GDCSceneKit.Row(decor, TileFactory.Get("Tuft"), 40, 0, "x   x  x");

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            if (answer)
            {
                var p = player.AddComponent<Ch3_TileProbe_Answer>();
                p.groundMap = ground; p.iceTile = ice; p.lavaTile = lava; p.brickTile = brick;
            }
            else
            {
                var p = player.AddComponent<Ch3_TileProbe>();
                p.groundMap = ground; p.iceTile = ice; p.lavaTile = lava; p.brickTile = brick;
            }

            GDCSceneKit.FollowPlayer(player, 6f, 40f);
            GDCSceneKit.Goal(new Vector2(46f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！你用程式改寫了地圖", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 3 章 · 程式讀寫 Tilemap");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch3_TileCode", answer));
        }
    }
}
