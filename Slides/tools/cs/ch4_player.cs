UnmaxAll();
Open("Assets/_GDC/Scenes/Ch4_SpriteSheet.unity");
var go = Find("Player"); Select(go); Expand(go, new[] { "Animator", "Ch4_HeroAnimator" });
Win("UnityEditor.InspectorWindow");
return RectOf("UnityEditor.InspectorWindow");
