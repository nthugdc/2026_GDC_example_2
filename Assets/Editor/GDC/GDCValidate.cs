using UnityEditor;
using UnityEngine;
using TMPro;

namespace GDCBuild
{
    /// <summary>
    /// 教材的自我檢查工具。
    /// 目前檢查兩件事：場景文字有沒有缺字、看板底板有沒有蓋住文字。
    /// </summary>
    public static class GDCValidate
    {
        /// <summary>這個字在目前的字型鏈（預設字型 + 全域 fallback）裡找得到嗎？</summary>
        static bool HasGlyph(char c)
        {
            var main = TMP_Settings.defaultFontAsset;
            if (main != null && main.HasCharacter(c)) return true;

            var fallbacks = TMP_Settings.fallbackFontAssets;
            if (fallbacks != null)
                foreach (var f in fallbacks)
                    if (f != null && f.HasCharacter(c)) return true;

            return false;
        }

        /// <summary>
        /// 掃過所有章節場景，回報缺字與被文字撐破的看板。
        /// 缺字會在畫面上變成豆腐方塊（□），中文字型不含的符號最容易中招，
        /// 例如 ▶ ● ★ 在 Droid Sans Fallback 裡都沒有。
        /// </summary>
        public static void Run()
        {
            var missing  = new System.Collections.Generic.SortedDictionary<char, string>();
            int overflow = 0, scenes = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { GDCPaths.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                scenes++;

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                        foreach (char c in text.text)
                            if (c > 127 && !HasGlyph(c) && !missing.ContainsKey(c))
                                missing[c] = System.IO.Path.GetFileNameWithoutExtension(path) + " / " + text.name;

                    foreach (var board in root.GetComponentsInChildren<GuideBoard>(true))
                        if (!Covers(board))
                        {
                            overflow++;
                            Debug.LogWarning($"[GDC] 看板底板沒蓋住文字：{System.IO.Path.GetFileName(path)} / {board.name}");
                        }
                }
            }

            foreach (var kv in missing)
                Debug.LogWarning($"[GDC] 缺字 '{kv.Key}' (U+{((int)kv.Key):X4})，會顯示成豆腐方塊。首次出現：{kv.Value}");

            if (missing.Count == 0 && overflow == 0)
                Debug.Log($"[GDC] 檢查通過：{scenes} 個場景，無缺字、看板都蓋得住文字");
            else
                Debug.LogWarning($"[GDC] 檢查發現問題：缺字 {missing.Count} 種、看板溢出 {overflow} 個");
        }

        /// <summary>底板有沒有完全蓋住文字（用世界座標的 renderer 邊界比對）。</summary>
        static bool Covers(GuideBoard board)
        {
            var bg = board.transform.Find(board.name + "_BG");
            var tmp = board.GetComponentInChildren<TMP_Text>(true);
            if (bg == null || tmp == null) return true;

            var bgRenderer = bg.GetComponent<SpriteRenderer>();
            var textRenderer = tmp.GetComponent<MeshRenderer>();
            if (bgRenderer == null || textRenderer == null) return true;

            tmp.ForceMeshUpdate();
            Bounds b = bgRenderer.bounds;
            Bounds t = textRenderer.bounds;

            const float slack = 0.02f;
            return t.min.x >= b.min.x - slack && t.max.x <= b.max.x + slack
                && t.min.y >= b.min.y - slack && t.max.y <= b.max.y + slack;
        }
    }
}
