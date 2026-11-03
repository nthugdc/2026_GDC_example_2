foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll(T("UnityEditor.AnimationWindow"))) ((UnityEditor.EditorWindow)o).Close();
UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch5_Skeleton_Answer.unity");
Select(Find("RigHero"));
Dock("UnityEditor.AnimationWindow", "UnityEditor.ProjectBrowser", false);
return "ok";
