UnmaxAll();
var ac = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/_GDC/Animations/Hero.controller");
UnityEditor.Animations.AnimatorStateTransition tr = null;
foreach (var s in ac.layers[0].stateMachine.states) if (s.state.name == "Idle") tr = s.state.transitions[0];
Select(tr);
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
