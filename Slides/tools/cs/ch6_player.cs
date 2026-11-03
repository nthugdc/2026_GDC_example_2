UnmaxAll();
Open("Assets/_GDC/Scenes/Ch6_AnimEvent.unity");
var go = Find("Player"); Select(go); Expand(go, new[] { "Ch6_HeroAttack" });
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
