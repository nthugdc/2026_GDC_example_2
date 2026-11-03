foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll(T("UnityEditor.AnimationWindow"))) ((UnityEditor.EditorWindow)o).Close();
UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch4_SpriteSheet_Answer.unity");
Select(Find("Player"));
Dock("UnityEditor.AnimationWindow", "UnityEditor.ProjectBrowser", false);
return "ok";
