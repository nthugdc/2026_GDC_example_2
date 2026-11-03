var w = Existing("UnityEditor.AnimationWindow");
w.Focus(); w.maximized = true;
w.GetType().GetProperty("animationClip", BF).SetValue(w, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>("Assets/_GDC/Animations/Hero_Run.anim"));
w.GetType().GetProperty("recording", BF).SetValue(w, false);
w.Repaint();
return RMain();
