foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll(T("UnityEditor.Graphs.AnimatorControllerTool"))) ((UnityEditor.EditorWindow)o).Close();
UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch4_SpriteSheet_Answer.unity");
Select(Find("Player"));
Dock("UnityEditor.Graphs.AnimatorControllerTool", "UnityEditor.SceneView", false);
return "ok";
