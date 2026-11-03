foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>()) if (o.maximized) o.Repaint();
return RMain();
