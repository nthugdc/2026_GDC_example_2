using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;

namespace GDCBuild
{
    /// <summary>
    /// 角色美術（彩色，例外於「純白 + alpha」規則，因為要看得出身體部位）：
    ///   1. Hero_Sheet：側面角色的 sprite sheet，12 格 × 128 px（待機 4、跑步 6、跳躍 2）
    ///      - Art/Characters/Hero_Sheet.png          已切好，給第 0~3、6 章與教師版用
    ///      - Art/Characters/Hero_Sheet_Unsliced.png 沒切，第 4 章學生自己切
    ///   2. RigHero：正面角色的單張大圖，第 5 章綁骨用
    ///      - Art/Characters/RigHero.png             沒綁骨，學生自己綁
    ///      - Teacher/Art/RigHero_Rigged.png          已用程式寫入骨架、網格、權重
    ///   3. 動畫片段與 Animator Controller
    /// </summary>
    public static class CharacterFactory
    {
        public const int Cell = 128;
        public const int PPU  = 128;

        public const string SheetPath     = GDCPaths.Characters + "/Hero_Sheet.png";
        public const string UnslicedPath  = GDCPaths.Characters + "/Hero_Sheet_Unsliced.png";
        public const string RigPath       = GDCPaths.Characters + "/RigHero.png";
        public const string RigRiggedPath = GDCPaths.TeacherArt + "/RigHero_Rigged.png";

        public const string HeroController     = GDCPaths.Animations + "/Hero.controller";
        public const string Ch6Controller      = GDCPaths.Animations + "/Ch6/Hero_Ch6.controller";
        public const string Ch6AnswerController= GDCPaths.TeacherAnim + "/Hero_Ch6_Answer.controller";
        public const string RigIdleClip        = GDCPaths.TeacherAnim + "/RigHero_Idle.anim";
        public const string RigController      = GDCPaths.TeacherAnim + "/RigHero.controller";

        static readonly (string name, int count)[] Frames = { ("Idle", 4), ("Run", 6), ("Jump", 2) };

        public static void Run()
        {
            GDCPaths.EnsureFolders();
            EnsureFolder(GDCPaths.Animations + "/Ch6");

            Painter sheet = DrawSheet();
            sheet.Save(SheetPath, PPU, SpriteImportMode.Multiple);
            SliceSheet(SheetPath);
            sheet.Save(UnslicedPath, PPU, SpriteImportMode.Single);   // 故意不切，留給第 4 章

            Painter rig = DrawRigHero();
            rig.Save(RigPath, PPU, SpriteImportMode.Single, SpriteMeshType.Tight);
            rig.Save(RigRiggedPath, PPU, SpriteImportMode.Single, SpriteMeshType.Tight);
            RigCharacter(RigRiggedPath, rig);

            BuildHeroAnimations();
            Debug.Log("[GDC] 角色美術、切圖、綁骨與動畫產生完成");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        // ── Sprite sheet 角色（側面）──────────────────────────────────────

        struct Pose
        {
            public float bob, lean;
            public float thighF, kneeF, thighB, kneeB;     // 角度（度）：0 = 朝正下方，正值 = 往前（右）
            public float armF, elbowF, armB, elbowB;
        }

        static Vector2 Dir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(r), -Mathf.Cos(r));
        }

        static IEnumerable<Pose> Poses()
        {
            float[] bob = { 0, 1, 2, 1 };
            for (int k = 0; k < 4; k++)
                yield return new Pose { bob = -bob[k], thighF = 4, thighB = -4, armF = 6 - bob[k] * 2, elbowF = 12, armB = -6 + bob[k] * 2, elbowB = 12 };

            for (int k = 0; k < 6; k++)
            {
                float ph = k / 6f * Mathf.PI * 2f;
                float s = Mathf.Sin(ph);
                yield return new Pose
                {
                    bob = 3f * Mathf.Abs(Mathf.Sin(ph)) - 2f, lean = 10f,
                    thighF =  32f * s, kneeF = 8f + 50f * Mathf.Max(0f, -Mathf.Cos(ph)),
                    thighB = -32f * s, kneeB = 8f + 50f * Mathf.Max(0f, Mathf.Cos(ph)),
                    armF = -45f * s, elbowF = 70f, armB = 45f * s, elbowB = 70f,
                };
            }

            yield return new Pose { bob = 4, lean = 5, thighF = 50, kneeF = 70, thighB = -5, kneeB = 40, armF = 150, elbowF = 20, armB = 120, elbowB = 30 };
            yield return new Pose { bob = 2, lean = 0, thighF = 25, kneeF = 10, thighB = -25, kneeB = 25, armF = 100, elbowF = 10, armB = -80, elbowB = 10 };
        }

        static Painter DrawSheet()
        {
            List<Pose> poses = Poses().ToList();
            var sheet = new Painter(Cell * poses.Count, Cell);
            for (int i = 0; i < poses.Count; i++)
            {
                var p = new Painter(Cell, Cell);
                DrawSide(p, poses[i]);
                sheet.Blit(p, i * Cell, 0);
            }
            return sheet;
        }

        /// <summary>畫一個面向右邊的側面小人。先畫全部的黑色外框，再畫顏色，外框才會連成一體。</summary>
        static void DrawSide(Painter p, Pose q)
        {
            const float thigh = 21f, shin = 21f, upper = 16f, fore = 15f;
            Vector2 hip = new Vector2(64f, 50f + q.bob);
            Vector2 up = new Vector2(Mathf.Sin(q.lean * Mathf.Deg2Rad), Mathf.Cos(q.lean * Mathf.Deg2Rad));
            Vector2 neck = hip + up * 28f;
            Vector2 shoulder = hip + up * 24f;
            Vector2 head = neck + up * 16f;

            Vector2 kneeF = hip + Dir(q.thighF) * thigh, footF = kneeF + Dir(q.thighF - q.kneeF) * shin;
            Vector2 kneeB = hip + Dir(q.thighB) * thigh, footB = kneeB + Dir(q.thighB - q.kneeB) * shin;
            Vector2 elbF = shoulder + Dir(q.armF) * upper, handF = elbF + Dir(q.armF + q.elbowF) * fore;
            Vector2 elbB = shoulder + Dir(q.armB) * upper, handB = elbB + Dir(q.armB + q.elbowB) * fore;

            // 讓比較低的那隻腳踩在 y = 6
            float lowest = Mathf.Min(footF.y, footB.y) - 4f;
            if (q.lean > 0f || q.bob <= 0f)
            {
                float dy = 6f - lowest;
                Vector2 d = new Vector2(0f, dy);
                hip += d; neck += d; shoulder += d; head += d;
                kneeF += d; footF += d; kneeB += d; footB += d; elbF += d; handF += d; elbB += d; handB += d;
            }

            Color o = GDCPalette.Outline;
            const float ow = 3f;
            // 外框
            p.Capsule(shoulder, elbB, 5 + ow, o); p.Capsule(elbB, handB, 5 + ow, o);
            p.Capsule(hip, kneeB, 7 + ow, o); p.Capsule(kneeB, footB, 6 + ow, o); p.Capsule(footB, footB + new Vector2(7, 0), 4 + ow, o);
            p.Capsule(hip, neck, 14 + ow, o);
            p.Capsule(hip, kneeF, 7 + ow, o); p.Capsule(kneeF, footF, 6 + ow, o); p.Capsule(footF, footF + new Vector2(7, 0), 4 + ow, o);
            p.Circle(head.x, head.y, 17 + ow, o);
            p.Capsule(shoulder, elbF, 5 + ow, o); p.Capsule(elbF, handF, 5 + ow, o);

            // 後面的手腳顏色暗一點，看起來有前後
            p.Capsule(shoulder, elbB, 5, Dark(GDCPalette.Shirt)); p.Capsule(elbB, handB, 5, Dark(GDCPalette.Skin));
            p.Capsule(hip, kneeB, 7, Dark(GDCPalette.Pants)); p.Capsule(kneeB, footB, 6, Dark(GDCPalette.Pants));
            p.Capsule(footB, footB + new Vector2(7, 0), 4, o);
            p.Capsule(hip + up * 6f, neck, 14, GDCPalette.Shirt);
            p.Capsule(hip, hip + up * 6f, 13, GDCPalette.Pants);
            p.Capsule(hip, kneeF, 7, GDCPalette.Pants); p.Capsule(kneeF, footF, 6, GDCPalette.Pants);
            p.Capsule(footF, footF + new Vector2(7, 0), 4, o);
            p.Circle(head.x, head.y, 17, GDCPalette.Skin);
            Vector2 hc = head;
            p.Fill(hc.x - 18, hc.y - 18, hc.x + 18, hc.y + 18, (x, y) =>
                (x - hc.x) * (x - hc.x) + (y - hc.y) * (y - hc.y) <= 17f * 17f && (y > hc.y + 5f || x < hc.x - 6f), GDCPalette.Hair);
            p.Circle(hc.x + 8f, hc.y + 1f, 2.6f, o);
            p.Capsule(shoulder, elbF, 5, GDCPalette.Shirt); p.Capsule(elbF, handF, 5, GDCPalette.Skin);
        }

        static Color Dark(Color c) => new Color(c.r * 0.72f, c.g * 0.72f, c.b * 0.72f, c.a);

        /// <summary>
        /// 用 Sprite Editor 的資料介面切圖（等同學生在 Sprite Editor 按 Slice → Grid By Cell Size）。
        /// 已存在的同名 sprite 沿用原本的 ID，動畫片段的參照才不會斷。
        /// </summary>
        static void SliceSheet(string path)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider dp = factories.GetSpriteEditorDataProviderFromObject(ti);
            dp.InitSpriteEditorDataProvider();

            var old = dp.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var rects = new List<SpriteRect>();
            int i = 0;
            foreach (var (name, count) in Frames)
                for (int k = 0; k < count; k++, i++)
                {
                    string n = $"Hero_{name}_{k}";
                    rects.Add(new SpriteRect
                    {
                        name = n,
                        rect = new Rect(i * Cell, 0, Cell, Cell),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        spriteID = old.TryGetValue(n, out GUID id) ? id : GUID.Generate(),
                    });
                }

            dp.SetSpriteRects(rects.ToArray());
            var names = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            dp.Apply();
            ti.SaveAndReimport();
        }

        public static Sprite HeroFrame(string name)
        {
            foreach (Object o in AssetDatabase.LoadAllAssetRepresentationsAtPath(SheetPath))
                if (o is Sprite s && s.name == name) return s;
            Debug.LogError($"[GDC] 找不到 {name}，請先執行 GDC/建置/2. 產生美術素材");
            return null;
        }

        static Sprite[] HeroFrames(string anim, int count)
        {
            var list = new Sprite[count];
            for (int k = 0; k < count; k++) list[k] = HeroFrame($"Hero_{anim}_{k}");
            return list;
        }

        // ── 動畫片段與 Animator ─────────────────────────────────────────

        static void BuildHeroAnimations()
        {
            AnimationClip idle = SpriteClip(GDCPaths.Animations + "/Hero_Idle.anim", HeroFrames("Idle", 4), 6f, null);
            AnimationClip run  = SpriteClip(GDCPaths.Animations + "/Hero_Run.anim",  HeroFrames("Run", 6), 12f, null);
            AnimationClip jump = SpriteClip(GDCPaths.Animations + "/Hero_Jump.anim", HeroFrames("Jump", 2), 6f, null);
            HeroStateMachine(HeroController, idle, run, jump);

            // 第 6 章：跑步片段另外複製一份，學生在上面加 Animation Event 不會影響其他章
            AnimationClip run6 = SpriteClip(GDCPaths.Animations + "/Ch6/Hero_Run_Ch6.anim", HeroFrames("Run", 6), 12f, null);
            HeroStateMachine(Ch6Controller, idle, run6, jump);

            AnimationClip run6Answer = SpriteClip(GDCPaths.TeacherAnim + "/Hero_Run_Ch6_Answer.anim", HeroFrames("Run", 6), 12f,
                                                  new[] { 1, 4 });   // 第 1、4 格腳著地
            HeroStateMachine(Ch6AnswerController, idle, run6Answer, jump);

            AssetDatabase.SaveAssets();
        }

        static T LoadOrCreateAsset<T>(string path, System.Func<T> create) where T : Object
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            a = create();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        /// <summary>逐格換圖的片段：在 SpriteRenderer.m_Sprite 上放 ObjectReference 關鍵影格。</summary>
        static AnimationClip SpriteClip(string path, Sprite[] frames, float fps, int[] footstepFrames)
        {
            AnimationClip clip = LoadOrCreateAsset(path, () => new AnimationClip());
            clip.ClearCurves();
            clip.frameRate = fps;

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var events = new List<AnimationEvent>();
            if (footstepFrames != null)
                foreach (int f in footstepFrames)
                    events.Add(new AnimationEvent { time = f / fps, functionName = "Footstep" });
            AnimationUtility.SetAnimationEvents(clip, events.ToArray());

            EditorUtility.SetDirty(clip);
            return clip;
        }

        /// <summary>Idle / Run / Jump 三個狀態。轉換一律關掉 Has Exit Time，切換才不會延遲。</summary>
        static void HeroStateMachine(string path, AnimationClip idle, AnimationClip run, AnimationClip jump)
        {
            AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ac == null) ac = AnimatorController.CreateAnimatorControllerAtPath(path);

            ac.parameters = new AnimatorControllerParameter[0];
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ac.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            ac.parameters = ac.parameters.Select(p => { if (p.name == "Grounded") p.defaultBool = true; return p; }).ToArray();

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            foreach (ChildAnimatorState s in sm.states) sm.RemoveState(s.state);
            foreach (AnimatorStateTransition t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);

            AnimatorState sIdle = sm.AddState("Idle", new Vector3(300, 0));
            AnimatorState sRun  = sm.AddState("Run",  new Vector3(300, 120));
            AnimatorState sJump = sm.AddState("Jump", new Vector3(560, 60));
            sIdle.motion = idle; sRun.motion = run; sJump.motion = jump;
            sm.defaultState = sIdle;

            Quick(sIdle.AddTransition(sRun)).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            Quick(sRun.AddTransition(sIdle)).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            AnimatorStateTransition toJump = Quick(sm.AddAnyStateTransition(sJump));
            toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            toJump.canTransitionToSelf = false;
            Quick(sJump.AddTransition(sIdle)).AddCondition(AnimatorConditionMode.If, 0f, "Grounded");

            EditorUtility.SetDirty(ac);
        }

        static AnimatorStateTransition Quick(AnimatorStateTransition t)
        {
            t.hasExitTime = false;
            t.duration = 0f;
            return t;
        }

        // ── 骨骼角色（正面）──────────────────────────────────────────────

        public const int RigW = 320, RigH = 512;

        // 骨骼：名稱、父骨骼索引、起點、終點（像素，原點在圖片左下）
        public static readonly (string name, int parent, Vector2 from, Vector2 to)[] Bones =
        {
            ("Hip",        -1, new Vector2(160, 222), new Vector2(160, 295)),
            ("Chest",       0, new Vector2(160, 295), new Vector2(160, 370)),
            ("Head",        1, new Vector2(160, 370), new Vector2(160, 480)),
            ("UpperArm_R",  1, new Vector2(203, 345), new Vector2(248, 280)),
            ("LowerArm_R",  3, new Vector2(248, 280), new Vector2(280, 215)),
            ("Hand_R",      4, new Vector2(280, 215), new Vector2(292, 190)),
            ("UpperArm_L",  1, new Vector2(117, 345), new Vector2(72, 280)),
            ("LowerArm_L",  6, new Vector2(72, 280),  new Vector2(40, 215)),
            ("Hand_L",      7, new Vector2(40, 215),  new Vector2(28, 190)),
            ("Thigh_R",     0, new Vector2(182, 222), new Vector2(192, 125)),
            ("Shin_R",      9, new Vector2(192, 125), new Vector2(196, 36)),
            ("Thigh_L",     0, new Vector2(138, 222), new Vector2(128, 125)),
            ("Shin_L",     11, new Vector2(128, 125), new Vector2(124, 36)),
        };

        static Painter DrawRigHero()
        {
            var p = new Painter(RigW, RigH);
            Color o = GDCPalette.Outline;
            const float ow = 5f;

            Vector2 B(int i, bool end) => end ? Bones[i].to : Bones[i].from;

            // 外框
            for (int side = 0; side < 2; side++)
            {
                int ua = side == 0 ? 3 : 6, th = side == 0 ? 9 : 11;
                p.Capsule(B(ua, false), B(ua, true), 17 + ow, o);
                p.Capsule(B(ua + 1, false), B(ua + 1, true), 14 + ow, o);
                p.Circle(B(ua + 2, false).x, B(ua + 2, false).y - 6, 18 + ow, o);
                p.Capsule(B(th, false), B(th, true), 22 + ow, o);
                p.Capsule(B(th + 1, false), B(th + 1, true), 19 + ow, o);
                float fx = B(th + 1, true).x + (side == 0 ? 12 : -12);
                p.Capsule(B(th + 1, true), new Vector2(fx, 24), 15 + ow, o);
            }
            p.Capsule(new Vector2(160, 225), new Vector2(160, 345), 44 + ow, o);
            p.Capsule(new Vector2(160, 360), new Vector2(160, 392), 15 + ow, o);
            p.Circle(160, 432, 58 + ow, o);

            // 顏色
            for (int side = 0; side < 2; side++)
            {
                int ua = side == 0 ? 3 : 6, th = side == 0 ? 9 : 11;
                p.Capsule(B(th, false), B(th, true), 22, GDCPalette.Pants);
                p.Capsule(B(th + 1, false), B(th + 1, true), 19, GDCPalette.Pants);
                float fx = B(th + 1, true).x + (side == 0 ? 12 : -12);
                p.Capsule(B(th + 1, true), new Vector2(fx, 24), 15, o);
            }
            p.Capsule(new Vector2(160, 215), new Vector2(160, 250), 42, GDCPalette.Pants);
            p.Capsule(new Vector2(160, 262), new Vector2(160, 345), 44, GDCPalette.Shirt);
            p.Capsule(new Vector2(160, 360), new Vector2(160, 392), 15, GDCPalette.Skin);
            for (int side = 0; side < 2; side++)
            {
                int ua = side == 0 ? 3 : 6;
                p.Capsule(B(ua, false), B(ua, true), 17, GDCPalette.Shirt);
                p.Capsule(B(ua + 1, false), B(ua + 1, true), 14, GDCPalette.Skin);
                p.Circle(B(ua + 2, false).x, B(ua + 2, false).y - 6, 18, GDCPalette.Skin);
            }
            p.Circle(160, 432, 58, GDCPalette.Skin);
            p.Fill(100, 380, 220, 492, (x, y) =>
                (x - 160) * (x - 160) + (y - 432) * (y - 432) <= 58f * 58f && y > 448f + 0.12f * Mathf.Abs(x - 160), GDCPalette.Hair);
            p.Circle(140, 430, 7, o);
            p.Circle(180, 430, 7, o);
            p.Capsule(new Vector2(148, 402), new Vector2(172, 402), 3, o);
            return p;
        }

        /// <summary>
        /// 用程式寫入骨架、網格與權重（等同學生在 Skinning Editor 做的事）：
        ///   骨架 → ISpriteBoneDataProvider（子骨骼的位置與角度是相對於父骨骼）
        ///   網格 → ISpriteMeshDataProvider（每 8 px 一格，只保留有畫到的格子）
        ///   權重 → 每個頂點取最近的兩根骨頭，越近權重越大
        /// </summary>
        static void RigCharacter(string path, Painter art)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider dp = factories.GetSpriteEditorDataProviderFromObject(ti);
            dp.InitSpriteEditorDataProvider();
            GUID id = dp.GetSpriteRects()[0].spriteID;

            // 骨架
            var bones = new List<SpriteBone>();
            var worldRot = new float[Bones.Length];
            for (int i = 0; i < Bones.Length; i++)
            {
                var (name, parent, from, to) = Bones[i];
                Vector2 d = to - from;
                worldRot[i] = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

                Vector2 localPos = from;
                float localRot = worldRot[i];
                if (parent >= 0)
                {
                    Vector2 rel = from - Bones[parent].from;
                    localPos = Quaternion.Euler(0, 0, -worldRot[parent]) * rel;
                    localRot = worldRot[i] - worldRot[parent];
                }
                bones.Add(new SpriteBone
                {
                    name = name,
                    guid = GUID.Generate().ToString(),
                    parentId = parent,
                    position = localPos,
                    rotation = Quaternion.Euler(0, 0, localRot),
                    length = d.magnitude,
                    color = Color.white,
                });
            }
            dp.GetDataProvider<ISpriteBoneDataProvider>().SetBones(id, bones);

            // 網格：格點頂點，只保留「有畫到」的格子
            const int step = 8;
            int cols = RigW / step, rows = RigH / step;
            var vIndex = new Dictionary<Vector2Int, int>();
            var verts = new List<Vertex2DMetaData>();
            var indices = new List<int>();

            int V(int gx, int gy)
            {
                var key = new Vector2Int(gx, gy);
                if (vIndex.TryGetValue(key, out int vi)) return vi;
                Vector2 pos = new Vector2(gx * step, gy * step);
                vi = verts.Count;
                verts.Add(new Vertex2DMetaData { position = pos, boneWeight = Weigh(pos) });
                vIndex[key] = vi;
                return vi;
            }

            for (int gy = 0; gy < rows; gy++)
            for (int gx = 0; gx < cols; gx++)
            {
                bool used = false;
                for (int sy = 0; sy <= step && !used; sy += step / 2)
                for (int sx = 0; sx <= step && !used; sx += step / 2)
                    if (art.AlphaAt(gx * step + sx, gy * step + sy) > 0.02f) used = true;
                if (!used) continue;

                int a = V(gx, gy), b = V(gx + 1, gy), c = V(gx + 1, gy + 1), d = V(gx, gy + 1);
                indices.AddRange(new[] { a, c, b, a, d, c });
            }

            var mesh = dp.GetDataProvider<ISpriteMeshDataProvider>();
            mesh.SetVertices(id, verts.ToArray());
            mesh.SetIndices(id, indices.ToArray());
            mesh.SetEdges(id, new Vector2Int[0]);

            dp.Apply();
            ti.SaveAndReimport();
        }

        // 每根骨頭負責的身體部位粗細（像素），跟 DrawRigHero 畫的半徑一致
        static readonly float[] BoneRadius = { 42, 44, 58, 17, 14, 18, 17, 14, 18, 22, 19, 22, 19 };

        /// <summary>
        /// 權重：先算「點離這個身體部位的表面多遠」（在部位裡面就是 0），選最近的一根；
        /// 第二根只能是它的父或子骨骼，免得頭被手臂拉走這種跨部位的混合。
        /// </summary>
        static BoneWeight Weigh(Vector2 p)
        {
            float Eff(int i)
            {
                float d = Painter.SegDist(p, Bones[i].from, Bones[i].to);
                return Mathf.Max(0f, d - BoneRadius[i]) + 0.15f * d;
            }

            int best = 0;
            for (int i = 1; i < Bones.Length; i++) if (Eff(i) < Eff(best)) best = i;

            int second = -1;
            for (int i = 0; i < Bones.Length; i++)
            {
                bool related = Bones[i].parent == best || Bones[best].parent == i;
                if (related && (second < 0 || Eff(i) < Eff(second))) second = i;
            }
            if (second < 0) return new BoneWeight { boneIndex0 = best, weight0 = 1f };

            float w0 = 1f / Mathf.Pow(Eff(best) + 4f, 4f);
            float w1 = 1f / Mathf.Pow(Eff(second) + 4f, 4f);
            float sum = w0 + w1;
            return new BoneWeight { boneIndex0 = best, weight0 = w0 / sum, boneIndex1 = second, weight1 = w1 / sum };
        }

        /// <summary>
        /// 骨骼角色的待機動畫：胸口與頭微微擺動，左手揮手。
        /// path 是骨頭相對於角色根物件的路徑（Sprite Skin「Create Bones」產生的階層）。
        /// </summary>
        public static AnimationClip BuildRigIdle(Dictionary<string, string> bonePaths)
        {
            AnimationClip clip = LoadOrCreateAsset(RigIdleClip, () => new AnimationClip());
            clip.ClearCurves();
            clip.frameRate = 30f;

            void Swing(string bone, float amp, float period, float phase)
            {
                float rest = RestLocalAngle(bone);
                var curve = new AnimationCurve();
                for (int k = 0; k <= 8; k++)
                {
                    float t = k / 8f * period;
                    curve.AddKey(t, rest + amp * Mathf.Sin((k / 8f + phase) * Mathf.PI * 2f));
                }
                string path = bonePaths[bone];
                foreach (string axis in new[] { "x", "y" })
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw." + axis),
                                                    AnimationCurve.Constant(0f, period, 0f));
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), curve);
            }

            Swing("Chest", 3f, 2f, 0f);
            Swing("Head", 5f, 2f, 0.25f);
            Swing("UpperArm_L", 25f, 2f, 0f);
            Swing("LowerArm_L", 20f, 2f, 0.15f);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);

            AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(RigController);
            if (ac == null) ac = AnimatorController.CreateAnimatorControllerAtPath(RigController);
            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            foreach (ChildAnimatorState s in sm.states) sm.RemoveState(s.state);
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = clip;
            sm.defaultState = idle;
            EditorUtility.SetDirty(ac);
            AssetDatabase.SaveAssets();
            return clip;
        }

        static float RestLocalAngle(string bone)
        {
            int i = System.Array.FindIndex(Bones, b => b.name == bone);
            Vector2 d = Bones[i].to - Bones[i].from;
            float w = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            int p = Bones[i].parent;
            if (p < 0) return w;
            Vector2 pd = Bones[p].to - Bones[p].from;
            return Mathf.DeltaAngle(0f, w - Mathf.Atan2(pd.y, pd.x) * Mathf.Rad2Deg);
        }
    }
}
