using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorEnum;

public class pb_Preferences
{
	private static bool prefsLoaded = false;

	static SelectMode pbDefaultSelectionMode;
	static Color _faceColor;
	static Color pbDefaultSelectedVertexColor;
	static Color pbDefaultVertexColor;
	static bool defaultOpenInDockableWindow;
	static bool defaultHideFaceMask;
	static Material _defaultMaterial;
	static Vector2 settingsScroll = Vector2.zero;
	static int defaultColliderType = 2;
	static bool _showNotifications;
	static bool pbForceConvex = false;
	static bool pbDragCheckLimit = false;
	static bool pbForceVertexPivot = true;
	static bool pbForceGridPivot = true;
	static bool pbPerimeterEdgeExtrusionOnly;
	static bool pbPerimeterEdgeBridgeOnly;
	static float pbVertexHandleSize;
	static bool pbPBOSelectionOnly;

	static pb_Shortcut[] defaultShortcuts;

	[PreferenceItem (pb_Constant.PRODUCT_NAME)]
	public static void PreferencesGUI () 
	{
		// Load the preferences
		if (!prefsLoaded) {
			LoadPrefs();
			prefsLoaded = true;
			OnWindowResize();
		}
		
		settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.MaxHeight(136));
		// Geometry Settings
		GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

		pbDefaultSelectionMode = (SelectMode)EditorGUILayout.EnumPopup("Default Selection Mode", pbDefaultSelectionMode);
		
		_faceColor = EditorGUILayout.ColorField("Selected Face Color", _faceColor);

		pbDefaultVertexColor = EditorGUILayout.ColorField("Vertex Color", pbDefaultVertexColor);
		pbDefaultSelectedVertexColor = EditorGUILayout.ColorField("Selected Vertex Color", pbDefaultSelectedVertexColor);

		pbVertexHandleSize = EditorGUILayout.FloatField("Vertex Handle Size", pbVertexHandleSize);

		_defaultMaterial = (Material) EditorGUILayout.ObjectField("Default Material", _defaultMaterial, typeof(Material), false);

		defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);

		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Default Collider");
		defaultColliderType = (int)((ColliderType)EditorGUILayout.EnumPopup( (ColliderType)defaultColliderType ));
		GUILayout.EndHorizontal();

		if((ColliderType)defaultColliderType == ColliderType.MeshCollider)
			pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);

		_showNotifications = EditorGUILayout.Toggle("Show Editor Notifications", _showNotifications);

		pbDragCheckLimit = EditorGUILayout.Toggle(new GUIContent("Limit Drag Check to Selection", "If true, when drag selecting faces, only currently selected pb-Objects will be tested for matching faces.  If false, all pb_Objects in the scene will be checked.  The latter may be slower in large scenes."), pbDragCheckLimit);

		pbForceVertexPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Vertex Point", "If true, new objects will automatically have their pivot point set to a vertex instead of the center."), pbForceVertexPivot);
		pbForceGridPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Grid", "If true, newly instantiated pb_Objects will be snapped to the nearest point on grid.  If ProGrids is present, the snap value will be used, otherwise decimals are simply rounded to whole numbers."), pbForceGridPivot);
		
		pbPerimeterEdgeExtrusionOnly = EditorGUILayout.Toggle(new GUIContent("Edge Extrude from Perimeters Only", "If true, only edges on the perimeters of an object may be extruded.  If false, you may extrude any edge you like (for those who like to live dangerously)."), pbPerimeterEdgeExtrusionOnly);
		pbPerimeterEdgeBridgeOnly = EditorGUILayout.Toggle(new GUIContent("Bridge Perimeter Edges Only", "If true, only edges on the perimeters of an object may be bridged.  If false, you may bridge any between any two edges you like."), pbPerimeterEdgeBridgeOnly);

		pbPBOSelectionOnly = EditorGUILayout.Toggle(new GUIContent("Only PBO are Selectable", "If true, you will not be able to select non probuilder objects in Geometry and Texture mode"), pbPBOSelectionOnly);

		GUILayout.Space(4);

		GUILayout.Label("Texture Editing Settings", EditorStyles.boldLabel);

		defaultHideFaceMask = EditorGUILayout.Toggle("Hide face mask", defaultHideFaceMask);

		EditorGUILayout.EndScrollView();

		GUILayout.Space(4);

		GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

		if(GUI.Button(resetRect, "Use defaults"))
			ResetToDefaults();

		ShortcutSelectPanel();
		ShortcutEditPanel();

		// Save the preferences
		if (GUI.changed)
			SetPrefs();
	}

	public static void OnWindowResize()
	{
		int pad = 10, buttonWidth = 100, buttonHeight = 20;
		resetRect = new Rect(Screen.width-pad-buttonWidth, Screen.height-pad-buttonHeight, buttonWidth, buttonHeight);
	}

	public static void ResetToDefaults()
	{
		if(EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?", "Are you sure you want to delete these?, this action cannot be undone.", "Yes", "No")) {
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultEditMode);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultSelectionMode);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultFaceColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultOpenInDockableWindow);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultShortcuts);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultMaterial);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultHideFaceMask);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultCollider);
			EditorPrefs.DeleteKey(pb_Constant.pbForceConvex);
			EditorPrefs.DeleteKey(pb_Constant.pbShowEditorNotifications);
			EditorPrefs.DeleteKey(pb_Constant.pbDragCheckLimit);
			EditorPrefs.DeleteKey(pb_Constant.pbForceVertexPivot);
			EditorPrefs.DeleteKey(pb_Constant.pbForceGridPivot);
			EditorPrefs.DeleteKey(pb_Constant.pbPerimeterEdgeExtrusionOnly);
			EditorPrefs.DeleteKey(pb_Constant.pbPerimeterEdgeBridgeOnly);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultSelectedVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbVertexHandleSize);
			EditorPrefs.DeleteKey(pb_Constant.pbPBOSelectionOnly);
		}

		LoadPrefs();
	}

	public static int shortcutIndex = 0;
	static Rect selectBox = new Rect(130, 207, 179, 185);

	static Rect resetRect = new Rect(0,0,0,0);
	static Vector2 shortcutScroll = Vector2.zero;
	static int CELL_HEIGHT = 20;
	public static void ShortcutSelectPanel()
	{
		GUILayout.Space(4);
		GUI.contentColor = Color.white;
		GUI.Box(selectBox, "");

		GUIStyle labelStyle = GUIStyle.none;

		if(EditorGUIUtility.isProSkin)
			labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.contentOffset = new Vector2(4f, 0f);

		shortcutScroll = EditorGUILayout.BeginScrollView(shortcutScroll, false, true, GUILayout.MaxWidth(183), GUILayout.MaxHeight(186));

		for(int n = 1; n < defaultShortcuts.Length; n++)
		{
			if(n == shortcutIndex) {
				GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
					labelStyle.normal.background = EditorGUIUtility.whiteTexture;
					Color oc = labelStyle.normal.textColor;
					labelStyle.normal.textColor = Color.white;
					GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT));
					labelStyle.normal.background = null;
					labelStyle.normal.textColor = oc;
				GUI.backgroundColor = Color.white;
	
				// if(GUILayout.Button(defaultShortcuts[n].action, EditorStyles.whiteLabel))
				// 	shortcutIndex = n;
			}
			else
			{

				if(GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT)))
					shortcutIndex = n;
			}
		}

		EditorGUILayout.EndScrollView();

	}

	static Rect keyRect = new Rect(324, 210, 168, 18);
	static Rect keyInputRect = new Rect(356, 210, 133, 18);

	static Rect descriptionTitleRect = new Rect(324, 270, 168, 200);
	static Rect descriptionRect = new Rect(324, 290, 168, 200);

	static Rect modifiersRect = new Rect(324, 240, 168, 18);
	static Rect modifiersInputRect = new Rect(383, 240, 107, 18);

	public static void ShortcutEditPanel()
	{
		// descriptionTitleRect = EditorGUI.RectField(new Rect(240,150,200,50), descriptionTitleRect);

		string keyString = defaultShortcuts[shortcutIndex].key.ToString();
	
		GUI.Label(keyRect, "Key");
		keyString = EditorGUI.TextField(keyInputRect, keyString);
		defaultShortcuts[shortcutIndex].key = pbUtil.ParseEnum(keyString, KeyCode.None);

		GUI.Label(modifiersRect, "Modifiers");
		defaultShortcuts[shortcutIndex].eventModifiers = 
			(EventModifiers)EditorGUI.EnumMaskField(modifiersInputRect, defaultShortcuts[shortcutIndex].eventModifiers);

		GUI.Label(descriptionTitleRect, "Description", EditorStyles.boldLabel);

		GUI.Label(descriptionRect, defaultShortcuts[shortcutIndex].description, EditorStyles.wordWrappedLabel);
	}

	public static void LoadPrefs()
	{
		_faceColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultFaceColor );
		
		pbDefaultSelectedVertexColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultSelectedVertexColor );
		pbDefaultVertexColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultVertexColor );

		if(!EditorPrefs.HasKey( pb_Constant.pbDefaultOpenInDockableWindow))
			EditorPrefs.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, true);

		defaultHideFaceMask = (EditorPrefs.HasKey(pb_Constant.pbDefaultHideFaceMask)) ? EditorPrefs.GetBool(pb_Constant.pbDefaultHideFaceMask) : false;
			
		defaultOpenInDockableWindow = EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);

		pbDefaultSelectionMode = pb_Preferences_Internal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
		defaultColliderType = (int)pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);
		
		pbDragCheckLimit 	= pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);
		pbForceConvex 		= pb_Preferences_Internal.GetBool(pb_Constant.pbForceConvex);
		pbForceGridPivot 	= pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot);
		pbForceVertexPivot 	= pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot);
		
		pbPerimeterEdgeExtrusionOnly = pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeExtrusionOnly);
		pbPerimeterEdgeBridgeOnly = pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);

		pbPBOSelectionOnly = pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly);

		pbVertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);

		if(EditorPrefs.HasKey(pb_Constant.pbDefaultMaterial))
		{
			_defaultMaterial = (Material) Resources.LoadAssetAtPath(pb_Constant.pbDefaultMaterial, typeof(Material));
			if(_defaultMaterial == null)
				_defaultMaterial = pb_Constant.DefaultMaterial;
		}

		defaultShortcuts = EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ? 
			pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts)) : 
			pb_Shortcut.DefaultShortcuts();

		_showNotifications = EditorPrefs.HasKey(pb_Constant.pbShowEditorNotifications) ?
			EditorPrefs.GetBool(pb_Constant.pbShowEditorNotifications) : true;
	}

	public static void SetPrefs()
	{
		EditorPrefs.SetInt		(pb_Constant.pbDefaultSelectionMode, (int)pbDefaultSelectionMode);
		EditorPrefs.SetString	(pb_Constant.pbDefaultFaceColor, _faceColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultSelectedVertexColor, pbDefaultSelectedVertexColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultVertexColor, pbDefaultVertexColor.ToString());
		EditorPrefs.SetBool  	(pb_Constant.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow);
		EditorPrefs.SetBool  	(pb_Constant.pbDefaultHideFaceMask, defaultHideFaceMask);
		EditorPrefs.SetString	(pb_Constant.pbDefaultShortcuts, pb_Shortcut.ShortcutsToString(defaultShortcuts));
		EditorPrefs.SetString	(pb_Constant.pbDefaultMaterial, AssetDatabase.GetAssetPath(_defaultMaterial));	
		EditorPrefs.SetInt 		(pb_Constant.pbDefaultCollider, defaultColliderType);	
		EditorPrefs.SetBool  	(pb_Constant.pbShowEditorNotifications, _showNotifications);
		EditorPrefs.SetBool  	(pb_Constant.pbForceConvex, pbForceConvex);
		EditorPrefs.SetBool  	(pb_Constant.pbDragCheckLimit, pbDragCheckLimit);
		EditorPrefs.SetBool  	(pb_Constant.pbForceVertexPivot, pbForceVertexPivot);
		EditorPrefs.SetBool  	(pb_Constant.pbForceGridPivot, pbForceGridPivot);
		EditorPrefs.SetBool		(pb_Constant.pbPerimeterEdgeExtrusionOnly, pbPerimeterEdgeExtrusionOnly);
		EditorPrefs.SetBool		(pb_Constant.pbPerimeterEdgeBridgeOnly, pbPerimeterEdgeBridgeOnly);
		EditorPrefs.SetFloat	(pb_Constant.pbVertexHandleSize, pbVertexHandleSize);
		EditorPrefs.SetBool		(pb_Constant.pbPBOSelectionOnly, pbPBOSelectionOnly);
		// pb_Editor.instance.LoadPrefs();
	}
}