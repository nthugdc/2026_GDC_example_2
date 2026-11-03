var w = Existing("UnityEditor.AnimationWindow");
w.Focus(); w.maximized = true;
w.GetType().GetProperty("animationClip", BF).SetValue(w, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>("Assets/_GDC/Teacher/Animations/RigHero_Idle.anim"));
w.GetType().GetProperty("recording", BF).SetValue(w, true);
w.Repaint();
return RMain();
