using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 6 章 · Animation Event：讓動畫在對的那一格呼叫腳本函式（攻擊打碎木箱、跑步揚塵）。</summary>
    public static class Ch6Builder
    {
        const string BoardText =
@"第 6 章 · Animation Event
木箱擋路！按 J 揮劍把它打碎
1. 完成 Ch6_HeroAttack.cs 的 TODO 6-A ~ 6-C
2. 選 Player，打開 Animation 視窗
   左上選單選 Hero_Attack_Ch6
3. 時間軸移到第 2 格（劍揮到前方）
   按 Add Event，Function 選 AttackHit
4. 換到 Hero_Run_Ch6，在第 1、4 格
   加 Event，Function 選 Footstep
A / D 移動　空白鍵跳　J 攻擊";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.4f, 2.7f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap decor  = GDCSceneKit.Layer(grid, "Decor", GDCSceneKit.OrderDecor, false);

            TileBase grass = TileFactory.Get("Ground");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 48, 12);
            GDCSceneKit.Fill(ground, grass, -12, -3, 46, -1);
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 16, 0, 18, 0);     // 小台階：跳上去看 Jump 動畫
            GDCSceneKit.Row(decor, TileFactory.Get("Tuft"), 1, 0, "x   x     x");

            // 三堆 3 層高的木箱（跳不過去，一定要打碎）
            GDCSceneKit.CrateStack(9f, 3);
            GDCSceneKit.CrateStack(24f, 3);
            GDCSceneKit.CrateStack(34f, 3);

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            player.GetComponent<HeroMove>().autoFlip = false;
            Animator anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                answer ? CharacterFactory.Ch6AnswerController : CharacterFactory.Ch6Controller);
            player.AddComponent<HeroAnimDriver>();   // 第 4 章的參考做法：負責 Speed、Grounded、面向
            if (answer) player.AddComponent<Ch6_HeroAttack_Answer>();
            else player.AddComponent<Ch6_HeroAttack>();

            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(41f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！劍在對的那一格打中了", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 6 章 · Animation Event");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch6_AnimEvent", answer));
        }
    }
}
