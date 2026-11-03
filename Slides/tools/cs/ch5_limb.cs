UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch5_Skeleton_Answer.unity");
var go = Find("Arm_R_Limb"); Select(go); Expand(go, new[] { "LimbSolver2D" });
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
