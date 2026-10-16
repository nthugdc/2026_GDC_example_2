using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 0 章 · 認識 Grid 與 Tilemap：不用寫程式，只調 Inspector。</summary>
    public static class Ch0Builder
    {
        const string BoardText =
@"第 0 章 · 認識 Grid 與 Tilemap
不用寫程式，只要改 Inspector！
【先按停止再改，播放中改的會被還原】

1. 地面看不見？選 Grid 底下的 Ground，
   Tilemap Renderer 的 Order in Layer
   改成比 Background（-20）大，例如 0
2. 坑過不去？選 Grid 底下的 Bridge：
   ・Tilemap 的 Color，A 拉到 255
   ・勾選 Tilemap Collider 2D
3. 加分：Grid 的 Cell Size 改成 0.5 看看
   （看完記得改回 1）

A / D 移動　空白鍵跳";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.4f, 2.8f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", answer ? GDCSceneKit.OrderGround : -30, true, layer: "Ground");
            Tilemap bridge = GDCSceneKit.Layer(grid, "Bridge", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap decor  = GDCSceneKit.Layer(grid, "Decor", GDCSceneKit.OrderDecor, false);

            TileBase stone = TileFactory.Get("Stone");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 48, 12);

            GDCSceneKit.Fill(ground, stone, -12, -3, 12, -1);
            GDCSceneKit.Fill(ground, stone, 19, -3, 46, -1);
            GDCSceneKit.Fill(ground, stone, -4, 0, -3, 12);      // 左牆
            GDCSceneKit.Fill(ground, stone, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, stone, 6, 0, 7, 0);         // 小台階
            GDCSceneKit.Fill(ground, stone, 25, 0, 26, 0);
            GDCSceneKit.Fill(ground, stone, 27, 0, 46, 1);

            GDCSceneKit.Fill(bridge, stone, 13, -1, 18, -1);
            if (!answer)
            {
                // 學生版：橋是透明的，而且沒有碰撞
                bridge.color = new Color(1f, 1f, 1f, 0f);
                bridge.GetComponent<TilemapCollider2D>().enabled = false;
            }

            TileBase tuft = TileFactory.Get("Tuft");
            GDCSceneKit.Row(decor, tuft, -2, 0, "x   x      x");
            GDCSceneKit.Row(decor, tuft, 20, 0, "x  x");
            GDCSceneKit.Row(decor, tuft, 29, 2, "x    x   x");

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(40f, 3.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！你已經會調 Tilemap 了", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 0 章 · Grid 與 Tilemap");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch0_Grid", answer));
        }
    }
}
