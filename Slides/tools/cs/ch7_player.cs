UnmaxAll();
Open("Assets/_GDC/Scenes/Ch7_StateMachine.unity");
var go = Find("Player"); Select(go); Expand(go, new[] { "Ch7_HeroStateMachine" });
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
