UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch5_Skeleton_Answer.unity");
var go = Find("RigHero"); Select(go); Expand(go, new[] { "SpriteSkin", "IKManager2D" });
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
