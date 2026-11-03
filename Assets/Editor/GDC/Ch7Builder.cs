using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 7 章 · State Machine：用 enum + switch 寫主角的狀態機，取代 Animator 的箭頭。</summary>
    public static class Ch7Builder
    {
        const string BoardText =
@"第 7 章 · State Machine（狀態機）
這章的 Animator 一條 Transition 都沒有，
由程式決定現在是哪個 State！
1. 打開 Ch7_HeroStateMachine.cs
2. Idle 是寫好的範例，照著寫：
   7-A Run　7-B Jump
3. 7-C 進入 Attack 時鎖住移動
頭上的黃字 = 目前的 State
A / D 移動　空白鍵跳　J 攻擊";

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
            GDCSceneKit.Fill(ground, grass, -12, -3, 11, -1);
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 47, -3, 48, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 14, -3, 46, -1);   // 12、13 是小缺口：練習 Jump
            GDCSceneKit.Fill(ground, grass, 25, 0, 27, 0);
            GDCSceneKit.Fill(front, TileFactory.Get("Bush"), 37, 0, 38, 0);

            GDCSceneKit.CrateStack(7f, 3);
            GDCSceneKit.CrateStack(20f, 3);
            GDCSceneKit.CrateStack(32f, 3);

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f));
            player.GetComponent<HeroMove>().autoFlip = false;
            Animator anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterFactory.Ch7Controller);
            GDCSceneKit.AddStateLabel(player);
            if (answer) player.AddComponent<Ch7_HeroStateMachine_Answer>();
            else player.AddComponent<Ch7_HeroStateMachine>();

            GDCSceneKit.FollowPlayer(player, 6f, 32f);
            GDCSceneKit.Goal(new Vector2(42f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！你寫出了自己的 State Machine", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 7 章 · State Machine");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch7_StateMachine", answer));
        }
    }
}
