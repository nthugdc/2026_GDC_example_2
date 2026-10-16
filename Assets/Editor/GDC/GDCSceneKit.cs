using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.Animation;
using TMPro;

namespace GDCBuild
{
    /// <summary>建構章節場景時共用的小工具。</summary>
    public static class GDCSceneKit
    {
        // 統一的繪製順序（數字越大越前面）
        public const int OrderBackdrop = -100;
        public const int OrderPlatform = -50;
        public const int OrderObject   = 0;
        public const int OrderActor    = 10;
        public const int OrderBoard    = 50;
        public const int OrderHud      = 100;

        /// <summary>開一個全新的空場景，並放好 2D 專案必備的攝影機與全域燈光。</summary>
        public static Scene NewScene(Vector2 camPos, float camSize = 6.5f)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(camPos.x, camPos.y, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic      = true;
            cam.orthographicSize  = camSize;
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.backgroundColor   = GDCPalette.Background;
            camGo.AddComponent<AudioListener>();

            // URP 2D：沒有全域燈光的話畫面會是全黑的
            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            return scene;
        }

        public static Camera MainCamera() => Object.FindFirstObjectByType<Camera>();

        // ── 基本物件 ────────────────────────────────────────────────────

        /// <summary>放一個純色的形狀。</summary>
        public static GameObject Shape(string name, string sprite, Color color,
                                       Vector2 pos, Vector2 size, int order = OrderObject, float angle = 0f)
        {
            var go = new GameObject(name);
            go.transform.position   = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.transform.rotation   = Quaternion.Euler(0, 0, angle);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = GDCPaths.Sprite(sprite);
            sr.color        = color;
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>地面 / 平台：方塊 + BoxCollider2D，圖層設為 Ground。</summary>
        public static GameObject Platform(string name, Vector2 center, Vector2 size,
                                          float angle = 0f, string physicsMat = null)
        {
            var go = Shape(name, "Square", GDCPalette.Platform, center, size, OrderPlatform, angle);
            go.layer = LayerMask.NameToLayer("Ground");
            var col = go.AddComponent<BoxCollider2D>();
            if (physicsMat != null) col.sharedMaterial = GDCPaths.Phys(physicsMat);
            return go;
        }

        /// <summary>用「上緣 Y」來擺平台，排版時比用中心點直覺。</summary>
        public static GameObject PlatformTop(string name, float left, float right, float topY,
                                             float thickness = 0.7f, string physicsMat = null)
        {
            var center = new Vector2((left + right) * 0.5f, topY - thickness * 0.5f);
            return Platform(name, center, new Vector2(right - left, thickness), 0f, physicsMat);
        }

        // ── 玩家與攝影機 ────────────────────────────────────────────────

        /// <summary>建立玩家：膠囊外型 + Rigidbody2D + CapsuleCollider2D + 重生。</summary>
        public static GameObject Player(Vector2 pos, bool freezeRotation = false, float killY = -8f)
        {
            var go = Shape("Player", "Capsule", GDCPalette.Player, pos, new Vector2(0.8f, 0.8f), OrderActor);
            go.layer = LayerMask.NameToLayer("Player");
            go.tag   = "Player";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation         = freezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.linearDamping          = 0f;
            rb.gravityScale           = 2.5f;   // 重一點，跳起來比較俐落不會飄

            // Rigidbody2D 靜止超過 Physics2D.timeToSleep（預設 0.5 秒）就會進入休眠，
            // 休眠的剛體不再發出 OnTriggerStay2D —— 第 2 章站在感應板上不動時，
            // 計時會在 0.5 秒左右卡住，稍微走動又恢復。關掉休眠才不會有這個問題。
            rb.sleepMode              = RigidbodySleepMode2D.NeverSleep;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size      = new Vector2(1f, 1.5f);
            col.direction = CapsuleDirection2D.Vertical;
            col.sharedMaterial = GDCPaths.Phys("Slippery");   // 免得貼牆卡住

            go.AddComponent<Respawner>().killY = killY;
            return go;
        }

        /// <summary>讓攝影機跟隨玩家，並限制水平範圍。</summary>
        public static void FollowPlayer(GameObject player, float minX, float maxX, bool followY = false)
        {
            var cam = MainCamera();
            if (cam == null) return;

            var f = cam.gameObject.AddComponent<CameraFollow>();
            f.target  = player.transform;
            f.followY = followY;
            f.offset  = new Vector2(1.5f, 1f);
            f.min     = new Vector2(minX, 0f);
            f.max     = new Vector2(maxX, 0f);
        }

        /// <summary>終點：碰到就過關。</summary>
        public static GameObject Goal(Vector2 center, Rigidbody2D player, string message, Vector2 size)
        {
            var go = Shape("Goal", "Square", new Color(0.02f, 0.84f, 0.63f, 0.35f), center, size, OrderObject);
            go.layer = LayerMask.NameToLayer("Goal");
            go.AddComponent<BoxCollider2D>().isTrigger = true;

            var g = go.AddComponent<TouchGoal>();
            g.targetBody   = player;
            g.clearMessage = message;

            Text("GoalLabel", "終點", new Vector2(center.x, center.y + size.y * 0.5f + 0.5f),
                 new Vector2(3f, 0.8f), 4f, GDCPalette.Goal, OrderObject, TextAlignmentOptions.Center);
            return go;
        }

        // ── 文字 ────────────────────────────────────────────────────────

        /// <summary>世界空間的文字（中文靠 TMP 的 fallback 字型顯示）。</summary>
        public static TextMeshPro Text(string name, string content, Vector2 pos, Vector2 boxSize,
                                       float fontSize, Color color, int order = OrderBoard,
                                       TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text      = content;
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.rectTransform.sizeDelta = boxSize;

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = order;
            return tmp;
        }

        public const float BoardPadding = 0.5f;

        /// <summary>
        /// 帶底板的引導看板。底板與文字各自獨立，避免縮放互相影響。
        /// size.x 是寬度，size.y 只是「最低高度」—— 底板一定會撐到蓋得住文字為止。
        /// </summary>
        public static GuideBoard Board(string name, string content, Vector2 center, Vector2 size,
                                       float fontSize = 4f)
        {
            var root = new GameObject(name);
            root.transform.position = center;

            // 底板一律用純正方形：圓角圖被非等比放大時，四角會被拉成橢圓
            var bg = Shape(name + "_BG", "Square", GDCPalette.Board, center, size, OrderBoard - 1);
            bg.transform.SetParent(root.transform, true);

            var tmp = Text(name + "_Text", content, center,
                           new Vector2(size.x - BoardPadding * 2f, size.y - BoardPadding * 2f),
                           fontSize, GDCPalette.Text, OrderBoard);
            tmp.transform.SetParent(root.transform, true);

            var board = root.AddComponent<GuideBoard>();
            board.content   = content;
            board.textColor = GDCPalette.Text;
            board.fontSize  = fontSize;
            board.Apply();

            FitBoardHeight(board, size);
            return board;
        }

        /// <summary>
        /// 量出文字實際需要的高度，把底板撐到蓋得住為止。
        /// 手填高度很容易漏算（換行、字型 fallback 都會影響），一律用量的。
        /// </summary>
        public static float FitBoardHeight(GuideBoard board, Vector2 size)
        {
            var tmp = board.GetComponentInChildren<TMP_Text>(true);
            if (tmp == null) return size.y;

            float innerWidth = size.x - BoardPadding * 2f;

            // 先給一個超高的框讓 TMP 自由排版，才量得到真正需要的高度
            tmp.rectTransform.sizeDelta = new Vector2(innerWidth, 1000f);
            tmp.ForceMeshUpdate();

            float height = Mathf.Max(size.y, tmp.preferredHeight + BoardPadding * 2f);
            tmp.rectTransform.sizeDelta = new Vector2(innerWidth, height - BoardPadding * 2f);

            var bg = board.transform.Find(board.name + "_BG");
            if (bg != null) bg.localScale = new Vector3(size.x, height, 1f);

            return height;
        }

        /// <summary>
        /// 把引導看板釘在畫面上緣或下緣，跟著攝影機移動。
        /// 看板放在世界座標的話，攝影機一跟著玩家跑，看板就會飄到關卡中間擋住畫面。
        /// 高度依文字實際需要自動撐開，不用手動填。
        /// </summary>
        /// <summary>
        /// 世界座標的看板，用「上緣 Y」定位，高度自動往下長。
        /// 適合第 0 章這種攝影機不會移動、看板擺在旁邊空地的場景。
        /// </summary>
        public static GuideBoard BoardTopAt(string name, string content,
                                            float centerX, float topY, float width, float fontSize)
        {
            var size = new Vector2(width, 0f);
            var board = Board(name, content, new Vector2(centerX, topY), size, fontSize);
            float height = FitBoardHeight(board, size);
            board.transform.position = new Vector2(centerX, topY - height * 0.5f);
            return board;
        }

        public static GuideBoard BoardOnCamera(string name, string content,
                                               float fontSize = 3.4f, bool bottom = true)
        {
            var cam = MainCamera();
            float halfH = cam.orthographicSize;
            float halfW = halfH * 16f / 9f;
            float width = halfW * 2f * 0.94f;

            var board = Board(name, content, Vector2.zero, new Vector2(width, 0f), fontSize);
            float height = FitBoardHeight(board, new Vector2(width, 0f));

            float y = bottom ? -halfH + height * 0.5f + 0.2f
                             :  halfH - height * 0.5f - 0.2f;

            board.transform.SetParent(cam.transform, false);
            board.transform.localPosition = new Vector3(0f, y, 1f);
            return board;
        }

        /// <summary>
        /// 釘在攝影機「左上角」的看板，寬度自訂，高度依文字自動撐開。
        /// 這份教材的地形在畫面下半部，所以看板放上面；右上角留給 HUD。
        /// </summary>
        public static GuideBoard BoardTopLeft(string name, string content, float width, float fontSize = 3.0f)
        {
            var cam = MainCamera();
            float halfH = cam.orthographicSize;
            float halfW = halfH * 16f / 9f;

            // width 與 fontSize 以「攝影機 size = 6」為準，鏡頭拉近時等比縮小，畫面上看起來一樣大
            float k = halfH / 6f;
            width *= k;
            fontSize *= k;

            var board = Board(name, content, Vector2.zero, new Vector2(width, 0f), fontSize);
            float height = FitBoardHeight(board, new Vector2(width, 0f));

            board.transform.SetParent(cam.transform, false);
            board.transform.localPosition = new Vector3(-halfW + width * 0.5f + 0.25f, halfH - height * 0.5f - 0.25f, 1f);
            return board;
        }

        // ── Tilemap ─────────────────────────────────────────────────────

        public const int OrderBackground = -20;
        public const int OrderGround     = 0;
        public const int OrderHazard     = 1;
        public const int OrderDecor      = 2;
        public const int OrderForeground = 20;

        public static Grid NewGrid()
        {
            var go = new GameObject("Grid");
            var grid = go.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            return grid;
        }

        /// <summary>在 Grid 底下加一層 Tilemap。collider = 加上 TilemapCollider2D（Composite 合併）。</summary>
        public static Tilemap Layer(Grid grid, string name, int order, bool collider,
                                    bool trigger = false, string layer = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var map = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = order;
            if (layer != null) go.layer = LayerMask.NameToLayer(layer);
            if (collider) AddTileCollider(map, trigger);
            return map;
        }

        /// <summary>
        /// Tilemap 的標準碰撞組合：TilemapCollider2D 每格一個方塊，
        /// 再交給 CompositeCollider2D 合併成一整塊（需要一個 Static 的 Rigidbody2D）。
        /// </summary>
        public static void AddTileCollider(Tilemap map, bool trigger = false)
        {
            var go = map.gameObject;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            var cc = go.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            cc.isTrigger = trigger;
        }

        /// <summary>把矩形範圍（含兩端）填滿同一種磁磚。</summary>
        public static void Fill(Tilemap map, TileBase tile, int x0, int y0, int x1, int y1)
        {
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                map.SetTile(new Vector3Int(x, y, 0), tile);
        }

        public static void Clear(Tilemap map, int x0, int y0, int x1, int y1) => Fill(map, null, x0, y0, x1, y1);

        /// <summary>用一行文字放一排磁磚：非空白字元的位置就放 tile（方便寫零散的裝飾）。</summary>
        public static void Row(Tilemap map, TileBase tile, int x0, int y, string pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
                if (pattern[i] != ' ' && pattern[i] != '.') map.SetTile(new Vector3Int(x0 + i, y, 0), tile);
        }

        // ── 角色 ────────────────────────────────────────────────────────

        /// <summary>
        /// 主角：sprite sheet 角色 + Rigidbody2D + 膠囊碰撞 + HeroMove + 重生。
        /// sprite 不給的話用切好的 Hero_Idle_0。
        /// </summary>
        public static GameObject Hero(Vector2 pos, Sprite sprite = null, float killY = -6f)
        {
            var go = new GameObject("Player");
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Player");
            go.tag = "Player";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : CharacterFactory.HeroFrame("Hero_Idle_0");
            sr.sortingOrder = OrderActor;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 2.5f;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.42f, 0.86f);
            col.offset = new Vector2(0f, -0.06f);
            col.sharedMaterial = GDCPaths.Phys("Slippery");   // 貼著牆按方向鍵才不會卡住

            var move = go.AddComponent<HeroMove>();
            move.groundLayers = LayerMask.GetMask("Ground");
            move.feetOffset = 0.45f;

            go.AddComponent<Respawner>().killY = killY;
            return go;
        }

        /// <summary>
        /// 骨骼角色：SpriteRenderer + SpriteSkin，並依 sprite 內的骨架資料產生骨頭物件
        /// （等同在 Inspector 按 Sprite Skin 的「Create Bones」）。回傳「骨頭名稱 → 相對路徑」。
        /// </summary>
        public static GameObject RigCharacter(string name, Sprite sprite, Vector2 pos,
                                              System.Collections.Generic.Dictionary<string, string> bonePaths)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = OrderActor;

            if (bonePaths == null) return go;   // 學生版：只有圖，骨頭自己綁

            var skin = go.AddComponent<SpriteSkin>();
            var util = typeof(SpriteSkin).Assembly.GetType("UnityEngine.U2D.Animation.SpriteSkinUtility");
            util.GetMethod("CreateBoneHierarchy",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .Invoke(null, new object[] { skin });

            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                if (t != go.transform) bonePaths[t.name] = AnimationUtility.CalculateTransformPath(t, go.transform);
            return go;
        }

        // ── 章節基礎建設 ────────────────────────────────────────────────

        /// <summary>總管物件：GameManager + 章節切換 + 右上角 HUD + 過關橫幅。</summary>
        public static GameManager Managers(string chapterTitle)
        {
            var go = new GameObject("__GDC_System");   // 教材基礎建設，學生不用動
            var gm = go.AddComponent<GameManager>();
            gm.chapterTitle = chapterTitle;
            go.AddComponent<ChapterNavigator>();

            var cam = MainCamera();
            if (cam != null)
            {
                // HUD 掛在攝影機下、貼齊畫面右上角（左上角通常留給引導看板）
                float halfH = cam.orthographicSize;
                float halfW = halfH * 16f / 9f;
                float k = halfH / 6.5f;   // 依鏡頭大小等比縮放，畫面上看起來一樣大
                var box = new Vector2(8.5f, 3.0f) * k;

                var hud = Text("HUD", "", Vector2.zero, box, 4.4f * k, GDCPalette.Text, OrderHud,
                               TextAlignmentOptions.TopRight);
                hud.transform.SetParent(cam.transform, false);
                hud.transform.localPosition = new Vector3(halfW - 0.3f - box.x * 0.5f,
                                                          halfH - 0.3f - box.y * 0.5f, 1f);
                hud.gameObject.AddComponent<HudDisplay>().Refresh();   // 先填好，編輯模式看得到

                // 過關橫幅：平常空白，過關時才顯示
                var banner = Text("ClearBanner", "", Vector2.zero, new Vector2(halfW * 1.6f, 2f * k),
                                  9f * k, GDCPalette.Goal, OrderHud, TextAlignmentOptions.Center);
                banner.transform.SetParent(cam.transform, false);
                banner.transform.localPosition = new Vector3(0f, -halfH * 0.55f, 1f);
                banner.gameObject.AddComponent<ClearBanner>();
            }
            return gm;
        }

        // ── 存檔 ────────────────────────────────────────────────────────

        /// <summary>學生版存 Scenes/，教師解答版存 Teacher/Scenes/。</summary>
        public static string ScenePath(string name, bool answer)
            => answer ? $"{GDCPaths.TeacherScenes}/{name}_Answer.unity"
                      : $"{GDCPaths.Scenes}/{name}.unity";

        public static void Save(Scene scene, string path)
        {
            BakeTileColliders();
            StripTmpSubMeshes();
            EditorSceneManager.SaveScene(scene, path);
            AddToBuildSettings(path);
            Debug.Log($"[GDC] 場景已建立：{path}");
        }

        /// <summary>
        /// CompositeCollider2D 會把合併好的形狀「存進場景」，執行時直接用存的那份。
        /// 但 Tilemap 改了之後碰撞要等到下一幀才更新，建置腳本畫完馬上存檔的話，
        /// 存進去的會是空的形狀 —— 角色直接穿過地面掉下去。存檔前先強制算一次。
        /// </summary>
        static void BakeTileColliders()
        {
            foreach (var tc in Object.FindObjectsByType<TilemapCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                tc.ProcessTilemapChanges();
                var cc = tc.GetComponent<CompositeCollider2D>();
                if (cc != null) cc.GenerateGeometry();
            }
        }

        /// <summary>
        /// 清掉 TMP 為了 fallback 字型自動產生的 sub-mesh 子物件。
        /// 這些子物件的材質是執行期產生的，存進場景後會變成失效參照，
        /// 導致中文在編輯模式顯示成洋紅色方塊。刪掉之後 TMP 會在開啟場景時重建。
        /// </summary>
        static void StripTmpSubMeshes()
        {
            foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                for (int i = t.transform.childCount - 1; i >= 0; i--)
                {
                    var child = t.transform.GetChild(i);
                    if (child.GetComponent<TMP_SubMesh>() != null || child.GetComponent<TMP_SubMeshUI>() != null)
                        Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        static void AddToBuildSettings(string path)
        {
            // 教師解答版不進 Build Settings，才不會打亂 N / P 的章節順序
            if (path.Contains("/Teacher/")) return;

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
