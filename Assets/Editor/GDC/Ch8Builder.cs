using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDCBuild
{
    /// <summary>第 8 章 · 自由創作：自己畫關卡，用上攻擊（Animation Event）與 State Machine。</summary>
    public static class Ch8Builder
    {
        const string BoardText =
@"第 8 章 · 自由創作
用學到的東西，做一個自己的關卡！
必做：
1. 用 Palette_All 畫出從起點到終點的路
   （Ground 有碰撞；岩漿畫在 Hazard）
2. 擺幾堆木箱擋路（Prefab：拖 Crate 進場景），
   用 J 攻擊打開一條路
3. 這章的主角沿用你第 7 章寫的
   State Machine（頭上會顯示 State）
加分：自己的 Rule Tile、冰、挖磚、新的 Event…";

        public const string CratePrefab = GDCPaths.Prefabs + "/Crate.prefab";

        /// <summary>給學生拖進場景用的木箱 Prefab。</summary>
        static void EnsureCratePrefab()
        {
            GameObject go = GDCSceneKit.Crate(Vector2.zero);
            PrefabUtility.SaveAsPrefabAsset(go, CratePrefab);
            Object.DestroyImmediate(go);
        }

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(6f, 3f), 5f);
            EnsureCratePrefab();
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 9.4f, 2.8f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            Tilemap hazard = GDCSceneKit.Layer(grid, "Hazard", GDCSceneKit.OrderHazard, true, trigger: true, layer: "Hazard");
            Tilemap decor  = GDCSceneKit.Layer(grid, "Decor", GDCSceneKit.OrderDecor, false);
            Tilemap front  = GDCSceneKit.Layer(grid, "Foreground", GDCSceneKit.OrderForeground, false);
            hazard.gameObject.AddComponent<KillZone>();

            TileBase grass = TileFactory.Get("Ground");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -12, -3, 56, 12);
            GDCSceneKit.Fill(ground, grass, -12, -3, 6, -1);     // 起點
            GDCSceneKit.Fill(ground, grass, -4, 0, -3, 12);
            GDCSceneKit.Fill(ground, grass, 55, -3, 56, 12);   // 右牆
            GDCSceneKit.Fill(ground, grass, 38, -3, 54, -1);     // 終點島

            if (answer) BuildSampleLevel(ground, hazard, decor, front);
            // 終點島上的木箱：用 Prefab 擺，示範學生可以怎麼擋路
            for (int i = 0; i < 3; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CratePrefab));
                c.transform.position = new Vector3(42f, 0.5f + i, 0f);
            }

            GameObject player = GDCSceneKit.Hero(new Vector2(-1f, 1f), null, -8f);
            player.GetComponent<HeroMove>().autoFlip = false;
            Animator anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterFactory.Ch7Controller);
            GDCSceneKit.AddStateLabel(player);
            if (answer) player.AddComponent<Ch7_HeroStateMachine_Answer>();
            else player.AddComponent<Ch7_HeroStateMachine>();   // 沿用你第 7 章寫的狀態機

            GDCSceneKit.FollowPlayer(player, 6f, 40f);
            GDCSceneKit.Goal(new Vector2(46f, 1.2f), player.GetComponent<Rigidbody2D>(),
                             "過關！這是你自己的關卡", new Vector2(1.6f, 2.4f));

            GDCSceneKit.Managers("第 8 章 · 自由創作");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch8_Free", answer));
        }

        /// <summary>教師版的範例關卡：給學生參考「可以怎麼做」。</summary>
        static void BuildSampleLevel(Tilemap ground, Tilemap hazard, Tilemap decor, Tilemap front)
        {
            TileBase grass = TileFactory.Get("Ground");
            TileBase stone = TileFactory.Get("Stone");
            TileBase ice   = TileFactory.Get("Ice");

            GDCSceneKit.Fill(ground, grass, 7, -3, 37, -2);
            GDCSceneKit.Fill(hazard, TileFactory.Get("Lava"), 7, -1, 37, -1);
            // 全速跳一次大約前進 4 格，每個落腳點都放在上一個邊緣往前 4 格左右
            GDCSceneKit.Fill(ground, stone, 9, 0, 11, 0);
            GDCSceneKit.Fill(ground, grass, 15, 0, 17, 0);
            GDCSceneKit.Fill(ground, ice, 20, 0, 24, 0);
            GDCSceneKit.Fill(ground, grass, 27, 1, 29, 1);
            GDCSceneKit.Fill(ground, stone, 32, 0, 34, 0);
            GDCSceneKit.Fill(front, TileFactory.Get("Bush"), 40, 0, 41, 0);
            GDCSceneKit.Row(decor, TileFactory.Get("Tuft"), -1, 0, "x   x  x");
            GDCSceneKit.Row(decor, TileFactory.Get("Tuft"), 43, 0, "x  x");
        }
    }
}
