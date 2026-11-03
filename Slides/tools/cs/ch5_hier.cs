UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch5_Skeleton_Answer.unity");
var go = Find("RigHero"); HierExpand(go); Select(go);
Win("UnityEditor.SceneHierarchyWindow");
return RectOf("UnityEditor.SceneHierarchyWindow");
