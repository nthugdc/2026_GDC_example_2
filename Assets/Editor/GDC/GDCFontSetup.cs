using System.Reflection;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace GDCBuild
{
    /// <summary>
    /// 中文字型 pipeline：匯入 TMP Essentials → 建立 dynamic SDF → 掛成全域 fallback。
    /// 主字型維持 LiberationSans（英數字），CJK 字型只當後備。
    /// </summary>
    public static class GDCFontSetup
    {
        const string FontPath  = GDCPaths.Fonts + "/GDC_CJK.ttf";
        const string AssetPath = GDCPaths.Fonts + "/GDC_CJK SDF.asset";

        [MenuItem("GDC/建置/0. 設定中文字型", false, 9)]
        public static void Run()
        {
            if (TMP_Settings.instance == null) ImportEssentials();
            if (TMP_Settings.instance == null)
            {
                Debug.LogError("[GDC] TMP Essentials 匯入後仍找不到 TMP_Settings，請 refresh 後再執行一次");
                return;
            }

            TMP_FontAsset fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (fa == null) fa = CreateFontAsset();
            AddFallback(fa);
            Verify(fa);
        }

        static void ImportEssentials()
        {
            System.Type t = null;
            foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType("TMPro.TMP_PackageResourceImporter");
                if (t != null) break;
            }
            if (t == null) { Debug.LogError("[GDC] 找不到 TMP_PackageResourceImporter"); return; }

            MethodInfo mi = t.GetMethod("ImportResources",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            object target = mi.IsStatic ? null : System.Activator.CreateInstance(t, true);
            mi.Invoke(target, new object[] { true, false, false });   // 只匯入 Essentials
            AssetDatabase.Refresh();
        }

        static TMP_FontAsset CreateFontAsset()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, 60, 5,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            fa.name = "GDC_CJK SDF";
            AssetDatabase.CreateAsset(fa, AssetPath);

            // 圖集貼圖與材質要存成子資產，否則重開專案會遺失
            fa.atlasTextures[0].name = fa.name + " Atlas";
            if (!AssetDatabase.Contains(fa.atlasTextures[0]))
                AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = fa.name + " Material";
            if (!AssetDatabase.Contains(fa.material))
                AssetDatabase.AddObjectToAsset(fa.material, fa);
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            return fa;
        }

        static void AddFallback(TMP_FontAsset fa)
        {
            SerializedObject so = new SerializedObject(TMP_Settings.instance);
            SerializedProperty list = so.FindProperty("m_fallbackFontAssets");
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == fa) return;

            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = fa;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        static void Verify(TMP_FontAsset cjk)
        {
            // 不要用 TryAddCharacters 的 missing 輸出判斷：Unity 6.3 的 TMP 即使加成功也會把字全列成 missing。
            // 逐字用 HasCharacter(c, false, true)（第三個參數 = 沒有就動態加入）才準。
            string missing = "";
            foreach (char c in "繁體中文測試：物理、射線、標籤、圖層、磁磚地圖、動畫、骨骼")
                if (!cjk.HasCharacter(c, false, true)) missing += c;
            TMP_FontAsset lib = TMP_Settings.defaultFontAsset;
            bool latin = lib != null && lib.HasCharacter('0') && lib.HasCharacter('R');
            Debug.Log($"[GDC] 字型驗收：CJK 缺字=[{missing}] 英數字在主字型={latin}");
        }
    }
}
