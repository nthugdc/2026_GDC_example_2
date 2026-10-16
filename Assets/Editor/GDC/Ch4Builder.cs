using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 4 章 · Sprite Sheet 切圖與 Animator 狀態機。</summary>
    public static class Ch4Builder
    {
        const string BoardText =
@"第 4 章 · Sprite Sheet 動畫
角色怎麼是一整排？因為圖還沒切！
1. 選 Art/Characters/Hero_Sheet_Unsliced
   Sprite Mode 改 Multiple，按 Apply
2. Sprite Editor > Slice > Grid By Cell
   Size，128 x 128 > Slice > Apply
3. Player 的 Sprite 換成切好的第 0 格
4. 選 Player，開 Animation 視窗，Create：
   Idle（第 0~3 格）Run（4~9）Jump（10、11）
5. Animator 視窗：加參數 Speed（Float）、
   Grounded（Bool），拉轉換、設條件，
   並取消勾選 Has Exit Time
6. 完成 Ch4_HeroAnimator.cs 的 TODO";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.4f, 2.7f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap front  = GDCSceneKit.Layer(grid, "Foreground", GDCSceneKit.OrderForeground, false);

            TileBase grass = TileFactory.Get("Ground");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 48, 12);
            GDCSceneKit.Fill(ground, grass, -12, -3, 9, -1);
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 12, -3, 17, -1);     // 小缺口，跳過去看跳躍動畫
            GDCSceneKit.Fill(ground, grass, 20, -3, 46, -1);
            GDCSceneKit.Fill(ground, grass, 24, 1, 26, 1);       // 浮空平台
            GDCSceneKit.Fill(ground, grass, 29, 0, 31, 0);
            GDCSceneKit.Fill(front, TileFactory.Get("Bush"), 34, 0, 35, 0);

            Sprite sprite;
            if (answer) sprite = CharacterFactory.HeroFrame("Hero_Idle_0");
            else sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CharacterFactory.UnslicedPath);   // 整條沒切的圖

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f), sprite);
            player.GetComponent<HeroMove>().autoFlip = false;   // 面向改由學生的腳本控制
            Animator anim = player.AddComponent<Animator>();
            if (answer)
            {
                anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterFactory.HeroController);
                player.AddComponent<Ch4_HeroAnimator_Answer>();
            }
            else
            {
                player.AddComponent<Ch4_HeroAnimator>();
            }

            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(40f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！角色動起來了", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 4 章 · Sprite Sheet 動畫");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch4_SpriteSheet", answer));
        }
    }
}
