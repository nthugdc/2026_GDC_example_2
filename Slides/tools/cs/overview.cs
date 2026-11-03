UnmaxAll();
Open("Assets/_GDC/Scenes/Ch6_AnimEvent.unity");
var go = Find("Player"); Select(go); Expand(go, new[] { "Animator", "Ch6_HeroAttack" });
var r = UnityEditor.EditorGUIUtility.GetMainWindowPosition();
return r.x + "," + r.y + "," + r.width + "," + r.height;
