using System.IO;
using UnityEditor;
using UnityEngine;

namespace GDCBuild
{
    /// <summary>教材各種資產的存放路徑，以及讀取用的小工具。</summary>
    public static class GDCPaths
    {
        public const string Root          = "Assets/_GDC";
        public const string Sprites       = Root + "/Art/Sprites";
        public const string Fonts         = Root + "/Art/Fonts";
        public const string PhysicsMats   = Root + "/Physics";
        public const string Tiles         = Root + "/Art/Tiles";
        public const string TileAssets    = Root + "/Tiles";
        public const string Palettes      = Root + "/Tiles/Palettes";
        public const string Characters    = Root + "/Art/Characters";
        public const string Animations    = Root + "/Animations";
        public const string TeacherArt    = Root + "/Teacher/Art";
        public const string TeacherAnim   = Root + "/Teacher/Animations";
        public const string TeacherScripts= Root + "/Teacher/Scripts";
        public const string Prefabs       = Root + "/Prefabs";
        public const string Scenes        = Root + "/Scenes";
        public const string TeacherScenes = Root + "/Teacher/Scenes";
        public const string Answers       = Root + "/Teacher/Answers";
        public const string Docs          = Root + "/Docs";

        static readonly string[] All =
        {
            Root, Root + "/Art", Sprites, Fonts, Tiles, Characters, TileAssets, Palettes, Animations,
            PhysicsMats, Prefabs, Scenes, Root + "/Teacher", TeacherScenes, Answers, Docs,
            TeacherArt, TeacherAnim, TeacherScripts,
            Root + "/Scripts", Root + "/Scripts/Core", Root + "/Scripts/Student"
        };

        public static void EnsureFolders()
        {
            foreach (var p in All)
            {
                var abs = Path.Combine(Directory.GetParent(Application.dataPath).FullName, p);
                if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);
            }
            AssetDatabase.Refresh();
        }

        public static Sprite Sprite(string shape)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{Sprites}/{shape}.png");
            if (s == null) Debug.LogError($"[GDC] 找不到 sprite：{shape}，請先執行 GDC/建置/1. 產生美術素材");
            return s;
        }

        public static PhysicsMaterial2D Phys(string name)
            => AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>($"{PhysicsMats}/{name}.physicsMaterial2D");

        public static T Load<T>(string path) where T : Object
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) Debug.LogError($"[GDC] 找不到資產：{path}，請先執行 GDC/建置/2. 產生美術素材");
            return a;
        }
    }
}
