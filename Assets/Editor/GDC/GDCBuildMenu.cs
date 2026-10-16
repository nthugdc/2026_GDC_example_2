using UnityEditor;
using UnityEngine;

namespace GDCBuild
{
    /// <summary>
    /// 教材建置選單。學生完全不需要用到這裡，
    /// 但如果場景被改壞了，可以用「重建章節」把該章還原。
    /// </summary>
    public static class GDCBuildMenu
    {
        const string M = "GDC/";

        [MenuItem(M + "建置/1. 專案設定（Layer 與 Tag）", false, 10)]
        public static void SetupProject() => GDCProjectSetup.Run();

        [MenuItem(M + "建置/2. 產生美術素材", false, 11)]
        public static void BuildArt()
        {
            SpriteFactory.Run();
            TileFactory.Run();
            CharacterFactory.Run();
        }

        [MenuItem(M + "重建章節/第 0 章 · Grid 與 Tilemap", false, 20)]
        public static void BuildCh0() => Ch0Builder.Build();

        [MenuItem(M + "重建章節/第 1 章 · Tile Palette", false, 21)]
        public static void BuildCh1() => Ch1Builder.Build();

        [MenuItem(M + "重建章節/第 2 章 · Rule Tile", false, 22)]
        public static void BuildCh2() => Ch2Builder.Build();

        [MenuItem(M + "重建章節/第 3 章 · 程式讀寫 Tilemap", false, 23)]
        public static void BuildCh3() => Ch3Builder.Build();

        [MenuItem(M + "重建章節/第 4 章 · Sprite Sheet 動畫", false, 24)]
        public static void BuildCh4() => Ch4Builder.Build();

        [MenuItem(M + "重建章節/第 5 章 · 骨骼動畫", false, 25)]
        public static void BuildCh5() => Ch5Builder.Build();

        [MenuItem(M + "重建章節/第 6 章 · 自由創作", false, 26)]
        public static void BuildCh6() => Ch6Builder.Build();

        [MenuItem(M + "建置/3. 重建全部章節（學生版 + 教師版）", false, 100)]
        public static void BuildAllChapters()
        {
            foreach (bool answer in new[] { false, true })
            {
                Ch0Builder.Build(answer);
                Ch1Builder.Build(answer);
                Ch2Builder.Build(answer);
                Ch3Builder.Build(answer);
                Ch4Builder.Build(answer);
                Ch5Builder.Build(answer);
                Ch6Builder.Build(answer);
            }
            Debug.Log("[GDC] 所有章節重建完成");
        }

        [MenuItem(M + "建置/全部重建（含字型、專案設定與美術）", false, 110)]
        public static void BuildAll()
        {
            GDCFontSetup.Run();
            SetupProject();
            BuildArt();
            BuildAllChapters();
        }

        [MenuItem(M + "工具/檢查場景文字（缺字與看板溢出）", false, 200)]
        public static void Validate() => GDCValidate.Run();
    }
}
