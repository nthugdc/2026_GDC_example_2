using UnityEditor;
using UnityEngine;

namespace GDCBuild
{
    /// <summary>
    /// 建立教材需要的 Layer 與 Tag。
    /// 名稱先建好（否則 CompareTag 會直接丟例外，對初學者太不友善），
    /// 學生的練習是「把正確的 Tag / Layer 指派給正確的物件」，
    /// 「自己新增一個全新的 Tag / Layer」則放在各章的加分題。
    /// </summary>
    public static class GDCProjectSetup
    {
        static readonly (int index, string name)[] Layers =
        {
            (6, "Ground"),
            (7, "Player"),
            (8, "Hazard"),
            (9, "Goal"),
        };

        // Tag 先建好，免得 CompareTag 直接丟例外，對初學者太不友善
        static readonly string[] Tags = { "Player", "Ice", "Lava" };

        public static void Run()
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0) { Debug.LogError("[GDC] 讀不到 TagManager.asset"); return; }

            var so = new SerializedObject(asset[0]);
            SetupLayers(so);
            SetupTags(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            RemoveSampleSceneFromBuild();

            Debug.Log("[GDC] Layer 與 Tag 設定完成");
        }

        /// <summary>把 Unity 預設的 SampleScene 移出 Build Settings，
        /// 章節順序（N / P 切換）才會從第 0 章開始。</summary>
        static void RemoveSampleSceneFromBuild()
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int removed = list.RemoveAll(s => s.path == "Assets/Scenes/SampleScene.unity");
            if (removed > 0) EditorBuildSettings.scenes = list.ToArray();
        }

        static void SetupLayers(SerializedObject so)
        {
            var layers = so.FindProperty("layers");
            foreach (var (index, name) in Layers)
            {
                if (index >= layers.arraySize) continue;
                layers.GetArrayElementAtIndex(index).stringValue = name;
            }
        }

        // Unity 內建就有的 Tag，不能也不需要再新增一次
        static readonly string[] BuiltInTags =
            { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };

        static void SetupTags(SerializedObject so)
        {
            var tags = so.FindProperty("tags");

            // 先清掉先前可能誤加的內建 Tag 副本
            for (int i = tags.arraySize - 1; i >= 0; i--)
                if (System.Array.IndexOf(BuiltInTags, tags.GetArrayElementAtIndex(i).stringValue) >= 0)
                    tags.DeleteArrayElementAtIndex(i);

            foreach (var tag in Tags)
            {
                if (System.Array.IndexOf(BuiltInTags, tag) >= 0) continue;

                bool exists = false;
                for (int i = 0; i < tags.arraySize; i++)
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag) exists = true;
                if (exists) continue;

                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            }
        }
    }
}
