using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.IK;

namespace GDCBuild
{
    /// <summary>第 5 章 · 2D 骨骼動畫：綁骨、錄動畫、IK 讓手跟著滑鼠。</summary>
    public static class Ch5Builder
    {
        const string BoardText =
@"第 5 章 · 2D 骨骼動畫與 IK
1. 選 Art/Characters/RigHero，按 Open
   Sprite Editor，左上角切到 Skinning Editor
2. Create Bone：從屁股往上點出身體、頭；
   再從肩膀點出手臂（上臂、前臂、手）
3. Auto Geometry > Generate
   Auto Weights > Generate > Apply
4. 選場景的 RigHero，Add Component：
   Sprite Skin > 按 Create Bones
5. Animation 視窗錄一段 Idle（轉動骨頭）
6. RigHero 加 IK Manager 2D > + > Limb
   Effector 選手的骨頭，Target 選 HandTarget
7. 完成 Ch5_MouseTarget.cs，用手碰星星！";

        public static void Build(bool answer = false)
        {
            var scene = GDCSceneKit.NewScene(new Vector2(0f, 0f), 4.6f);
            GDCSceneKit.BoardTopLeft("GuideBoard", BoardText, 10.4f, 2.85f);

            Grid grid = GDCSceneKit.NewGrid();
            Tilemap bg     = GDCSceneKit.Layer(grid, "Background", GDCSceneKit.OrderBackground, false);
            Tilemap ground = GDCSceneKit.Layer(grid, "Ground", GDCSceneKit.OrderGround, true, layer: "Ground");
            GDCSceneKit.Fill(bg, TileFactory.Get("BackWall"), -10, -6, 10, 6);
            GDCSceneKit.Fill(ground, TileFactory.Get("Ground"), -10, -6, 10, -4);

            // 角色 4 單位高（512 px / 128），腳踩在 y = -3
            Vector2 pos = new Vector2(2.6f, -1f);
            var bonePaths = answer ? new Dictionary<string, string>() : null;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(answer ? CharacterFactory.RigRiggedPath : CharacterFactory.RigPath);
            GameObject hero = GDCSceneKit.RigCharacter("RigHero", sprite, pos, bonePaths);

            // 右肩在角色中心往右上 (43, 89) px 的地方，手臂全長約 1.4 單位
            Vector2 shoulder = pos + new Vector2(43f, 89f) / CharacterFactory.PPU;
            Vector2 handRest = pos + (CharacterFactory.Bones[5].from - new Vector2(160f, 256f)) / CharacterFactory.PPU;

            GameObject target = GDCSceneKit.Shape("HandTarget", "Ring", GDCPalette.Goal, handRest, new Vector2(0.4f, 0.4f), GDCSceneKit.OrderHud);
            if (answer) target.AddComponent<Ch5_MouseTarget_Answer>();
            else target.AddComponent<Ch5_MouseTarget>();

            Vector2[] starOffsets = { new Vector2(1.25f, 0.3f), new Vector2(0.55f, 1.15f), new Vector2(1.1f, -0.65f) };
            var stars = new Transform[starOffsets.Length];
            for (int i = 0; i < stars.Length; i++)
                stars[i] = GDCSceneKit.Shape("Star" + i, "Star", GDCPalette.Coin, shoulder + starOffsets[i],
                                             new Vector2(0.45f, 0.45f), GDCSceneKit.OrderObject).transform;
            var catcher = new GameObject("Stars").AddComponent<StarCatcher>();
            catcher.stars = stars;

            if (answer) SetupAnswerRig(hero, bonePaths, target.transform);

            GDCSceneKit.Managers("第 5 章 · 骨骼動畫");
            GDCSceneKit.Save(scene, GDCSceneKit.ScenePath("Ch5_Skeleton", answer));
        }

        /// <summary>教師版：掛上待機動畫，並幫右手加 Limb IK，目標是 HandTarget。</summary>
        static void SetupAnswerRig(GameObject hero, Dictionary<string, string> bonePaths, Transform target)
        {
            CharacterFactory.BuildRigIdle(bonePaths);
            Animator anim = hero.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterFactory.RigController);

            IKManager2D ik = hero.AddComponent<IKManager2D>();
            var solverGo = new GameObject("Arm_R_Limb");
            solverGo.transform.SetParent(hero.transform, false);
            LimbSolver2D limb = solverGo.AddComponent<LimbSolver2D>();
            IKChain2D chain = limb.GetChain(0);
            chain.effector = hero.transform.Find(bonePaths["Hand_R"]);
            chain.target = target;
            limb.flip = true;   // 手肘往外彎
            ik.AddSolver(limb);
        }
    }
}
