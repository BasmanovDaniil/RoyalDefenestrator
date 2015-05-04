using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.EditorEnum;

public class ProBuilderMenuItems : EditorWindow
{
#region WINDOW

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/About", false, 0)]
	public static void MenuInitAbout()
	{
		pc_AboutWindow.Init("Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/About/pc_AboutEntry_ProBuilder.txt", true);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/" + pb_Constant.PRODUCT_NAME + " Window", false, pb_Constant.MENU_WINDOW + 0)]
	public static pb_Editor OpenEditorWindow()
	{
		if(EditorPrefs.HasKey(pb_Constant.pbDefaultOpenInDockableWindow) && !EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow))
			return (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), true, pb_Constant.PRODUCT_NAME, true);			// open as floating window
		else
			return (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), false, pb_Constant.PRODUCT_NAME, true);			// open as dockable window
	}

	#if !PROTOTYPE
	[MenuItem("Tools/ProBuilder/Texture Window", false, pb_Constant.MENU_WINDOW + 1)]
	public static void OpenTextureWindow()
	{
		EditorWindow.GetWindow<pb_Texture_Editor>().Show();
	}
	#endif

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Shape Window %#k", false, pb_Constant.MENU_WINDOW + 2)]
	public static void ShapeMenu()
	{
		EditorWindow.GetWindow(typeof(pb_Geometry_Interface), true, "Shape Menu", true);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors Window", false, pb_Constant.MENU_WINDOW + 3)]
	public static void Init()
	{
		bool openInDockableWindow = !pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		EditorWindow.GetWindow<pb_VertexColorInterface>(openInDockableWindow, "Vertex Colors", true);
	}

	public static void ForceCloseEditor()
	{
		EditorWindow.GetWindow<pb_Editor>().Close();
	}
#endregion

#region ProBuilder/Edit

	#if UNITY_STANDALONE_OSX // unity can't figure out how to implement single char shortcuts.  this breaks uppercase input on windows unity 4+, but not mac
	[MenuItem("Tools/ProBuilder/Texture Window _j", true, pb_Constant.MENU_WINDOW + 1)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Edit Level _g", true, pb_Constant.MENU_SELECTION + 0)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Selection Mode _h", true, pb_Constant.MENU_SELECTION + 1)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Handle Pivot _p", true, pb_Constant.MENU_SELECTION + 1)]
	public static bool ValidateToggleSelectMode()
	{
		EditorWindow window = EditorWindow.focusedWindow;
		return window != null && (window.GetType() == typeof(SceneView) || window.GetType() == typeof(pb_Editor));
	}
	#endif

	#if UNITY_STANDALONE_OSX
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Edit Level _g", false, pb_Constant.MENU_EDITOR + 0)]
	#else
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Edit Level", false, pb_Constant.MENU_EDITOR + 0)]
	#endif
	public static void ToggleEditLevel()
	{
		pb_Editor.instance.ToggleEditLevel();
		switch(pb_Editor.instance.editLevel)
		{
			case EditLevel.Top:
				pb_Editor_Utility.ShowNotification("Top Level Editing");
				break;

			case EditLevel.Geometry:
				pb_Editor_Utility.ShowNotification("Geometry Editing");
				break;
		}
	}

	#if UNITY_STANDALONE_OSX
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Selection Mode _h", false, pb_Constant.MENU_EDITOR + 1)]
	#else
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Selection Mode", false, pb_Constant.MENU_EDITOR + 1)]
	#endif
	public static void ToggleSelectMode()
	{
		pb_Editor.instance.ToggleSelectionMode();
		switch(pb_Editor.instance.selectionMode)
		{
			case SelectMode.Face:
				pb_Editor_Utility.ShowNotification("Editing Faces");
				break;

			case SelectMode.Vertex:
				pb_Editor_Utility.ShowNotification("Editing Vertices");
				break;

			case SelectMode.Edge:
				pb_Editor_Utility.ShowNotification("Editing Edges\n(Beta!)");
				break;
		}
	}

	#if UNITY_STANDALONE_OSX
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Handle Pivot _p", false, pb_Constant.MENU_EDITOR + 2)]
	#else
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Handle Pivot", false, pb_Constant.MENU_EDITOR + 2)]
	#endif
	public static void ToggleHandleAlignment()
	{
		pb_Editor.instance.ToggleHandleAlignment();		
		pb_Editor_Utility.ShowNotification("Handle Alignment: " + ((HandleAlignment)pb_Editor.instance.handleAlignment).ToString());
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Lightmap Settings Window", false, pb_Constant.MENU_EDITOR + 3)]
	public static void LightmapWindowInit()
	{
		pb_Lightmap_Editor.Init(pb_Editor.instance);
	}
#endregion

#region VERTEX COLORS

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &1", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset1() {
		pb_VertexColorInterface.SetFaceColors(1);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &2", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset2() {
		pb_VertexColorInterface.SetFaceColors(2);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &3", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset3() {
		pb_VertexColorInterface.SetFaceColors(3);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &4", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset4() {
		pb_VertexColorInterface.SetFaceColors(4);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &5", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset5() {
		pb_VertexColorInterface.SetFaceColors(5);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &6", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset6() {
		pb_VertexColorInterface.SetFaceColors(6);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &7", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset7() {
		pb_VertexColorInterface.SetFaceColors(7);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &8", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset8() {
		pb_VertexColorInterface.SetFaceColors(8);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &9", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset9() {
		pb_VertexColorInterface.SetFaceColors(9);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &0", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset0() {
		pb_VertexColorInterface.SetFaceColors(0);
	}
#endregion
}