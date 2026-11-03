UnmaxAll();
foreach (var o in UnityEngine.Resources.FindObjectsOfTypeAll(T("UnityEditor.U2D.Sprites.SpriteEditorWindow"))) ((UnityEditor.EditorWindow)o).Close();
Select(Asset("Assets/_GDC/Teacher/Art/RigHero_Rigged.png"));
var w = Dock("UnityEditor.U2D.Sprites.SpriteEditorWindow", "UnityEditor.SceneView", true);
var dd = (UnityEngine.UIElements.DropdownField)w.GetType().GetField("m_ModuleDropDownUI", BF).GetValue(w);
dd.value = "Skinning Editor";
w.Repaint();
return RMain();
