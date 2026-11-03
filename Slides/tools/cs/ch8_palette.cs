UnmaxAll();
Open("Assets/_GDC/Scenes/Ch8_Free.unity");
UnityEditor.Tilemaps.GridPaintingState.palette = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/_GDC/Tiles/Palettes/Palette_All.prefab");
UnityEditor.Tilemaps.GridPaintingState.scenePaintTarget = Find("Ground");
var ir = RectOf("UnityEditor.InspectorWindow");
Dock("UnityEditor.Tilemaps.GridPaintPaletteWindow", "UnityEditor.InspectorWindow", false);
return ir;
