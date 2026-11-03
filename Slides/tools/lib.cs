// 截圖腳本共用的小工具（eval_file 的方法本體，會被接在每個 cs/*.cs 前面）
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
System.Func<string, System.Type> T = n => { foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()) { var t = a.GetType(n); if (t != null) return t; } return null; };
System.Action<string> Open = p => { if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != p) UnityEditor.SceneManagement.EditorSceneManager.OpenScene(p); };
System.Func<string, UnityEngine.GameObject> Find = n => UnityEngine.GameObject.Find(n);
System.Func<string, UnityEditor.EditorWindow> Win = n => { var w = UnityEditor.EditorWindow.GetWindow(T(n)); w.Show(); w.Focus(); w.Repaint(); return w; };
System.Action<UnityEngine.Object> Select = o => { UnityEditor.Selection.activeObject = o; };
System.Func<string, UnityEngine.Object> Asset = p => UnityEditor.AssetDatabase.LoadMainAssetAtPath(p);
System.Action<UnityEngine.GameObject, string[]> Expand = (go, keep) => {
    foreach (var c in go.GetComponents<UnityEngine.Component>())
        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(c, System.Array.IndexOf(keep, c.GetType().Name) >= 0);
    UnityEditor.ActiveEditorTracker.sharedTracker.ForceRebuild();
};
System.Action<UnityEngine.GameObject> HierExpand = go => {
    var hw = UnityEditor.EditorWindow.GetWindow(T("UnityEditor.SceneHierarchyWindow"));
    hw.GetType().GetMethod("SetExpandedRecursive", BF).Invoke(hw, new object[] { go.GetInstanceID(), true });
};
System.Func<UnityEditor.EditorWindow, string> R = w => { var r = w.position; return r.x + "," + r.y + "," + r.width + "," + r.height; };
// 停靠視窗回報的 y 有時是相對座標（明顯偏小），補回去
System.Func<string, string> RectOf = n => { foreach (var w in UnityEngine.Resources.FindObjectsOfTypeAll(T(n))) { var r = ((UnityEditor.EditorWindow)w).position; if (r.y < 100) r.y += 117; return r.x + "," + r.y + "," + r.width + "," + r.height; } return "full"; };
System.Func<string> RMain = () => { var r = UnityEditor.EditorGUIUtility.GetMainWindowPosition(); return r.x + "," + (r.y + 22) + "," + r.width + "," + (r.height - 44); };
System.Action UnmaxAll = () => { foreach (var w in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>()) if (w.maximized) w.maximized = false; };
// 一律停靠在主視窗裡（COSMIC 是平鋪式視窗管理，浮動視窗會把版面擠亂）
System.Func<string, string, bool, UnityEditor.EditorWindow> Dock = (n, nextTo, max) => {
    System.Reflection.MethodInfo gm = null;
    foreach (var m in typeof(UnityEditor.EditorWindow).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        if (m.Name == "GetWindow" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(System.Type[])) gm = m;
    var w = (UnityEditor.EditorWindow)gm.MakeGenericMethod(T(n)).Invoke(null, new object[] { new System.Type[] { T(nextTo) } });
    w.Show(); w.Focus(); if (max) w.maximized = true; w.Repaint(); return w;
};
System.Func<string, UnityEditor.EditorWindow> Existing = n => { foreach (var w in UnityEngine.Resources.FindObjectsOfTypeAll(T(n))) return (UnityEditor.EditorWindow)w; return null; };
