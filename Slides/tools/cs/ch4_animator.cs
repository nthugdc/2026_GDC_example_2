foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll(T("UnityEditor.Graphs.AnimatorControllerTool"))) ((UnityEditor.EditorWindow)o).Close();
UnmaxAll();
Open("Assets/_GDC/Teacher/Scenes/Ch4_SpriteSheet_Answer.unity");
Select(Find("Player"));
var w = Dock("UnityEditor.Graphs.AnimatorControllerTool", "UnityEditor.SceneView", true);
var vt = w.GetType().GetField("m_ViewTransforms", BF);
vt.SetValue(w, System.Activator.CreateInstance(vt.FieldType));   // 清掉存下來的縮放與位置，回到預設
w.GetType().GetField("m_CurrentEditor", BF).SetValue(w, 1);
w.Repaint();
var cv = w.GetType().GetMethod("CenterView", BF); if (cv != null) cv.Invoke(w, null);
return RMain();
