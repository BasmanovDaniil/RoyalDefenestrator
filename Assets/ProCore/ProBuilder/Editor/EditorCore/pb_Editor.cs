#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5 || UNITY_4_3_6 || UNITY_4_3_7 || UNITY_4_3_8 || UNITY_4_3_9 || UNITY_4_4 || UNITY_4_4_0 || UNITY_4_4_1 || UNITY_4_4_2 || UNITY_4_4_3 || UNITY_4_4_4 || UNITY_4_4_5 || UNITY_4_4_6 || UNITY_4_4_7 || UNITY_4_4_8 || UNITY_4_4_9 || UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9
#define UNITY_4_3
#endif

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4
#endif

#undef UNITY_4

#if BUGGER
using Parabox.Bugger;
#endif

// TODO
// udpate preferences version

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using ProCore.Common;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Math;
using ProBuilder2.EditorEnum;

[InitializeOnLoad]
public class pb_Editor : EditorWindow {

#if PROFILE_TIMES
	public pb_Profiler profiler = new pb_Profiler();
#endif

#region STATIC REFERENCES

	public static pb_Editor instanceIfExists { get { return me; } }

	public static pb_Editor instance { get { 
		return (EditorPrefs.HasKey(pb_Constant.pbDefaultOpenInDockableWindow) && !EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow)) ?
			(pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), true, pb_Constant.PRODUCT_NAME, true)			// open as floating window
		:
			(pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), false, pb_Constant.PRODUCT_NAME, true);			// open as dockable window
		} }
#endregion

#region MENU TEXTURES
	
	private Texture2D plane_Graphic;
	private Texture2D local_Graphic;
	private Texture2D global_Graphic;
	
	private Texture2D hgraphic;
	
	// private Texture2D acg_Graphic;
	private Texture2D extrude_Graphic;
	private Texture2D colorPanel_Graphic;
	
	// private Texture2D prefab_Graphic;
	private Texture2D select_Graphic;
	private Texture2D face_Graphic;
	private Texture2D vertex_Graphic;
	private Texture2D edge_Graphic;
	private Texture2D shape_Graphic;
	
	private Texture2D modeGraphic;
	
	private Texture2D detail_OnGraphic;
	private Texture2D mover_OnGraphic; 
	private Texture2D collision_OnGraphic;
	private Texture2D trigger_OnGraphic;
	
	private Texture2D detail_OffGraphic;
	private Texture2D mover_OffGraphic;
	private Texture2D collision_OffGraphic;
	private Texture2D trigger_OffGraphic;
	
	private Texture2D dgraphic;
	private Texture2D wgraphic;
	private Texture2D mgraphic;
	private Texture2D cgraphic;
	private Texture2D tgraphic;
	private Texture2D ndgraphic;

	#if !PROTOTYPE
	private Texture2D lightmap_Graphic;
	private Texture2D mirror_Graphic;
	private Texture2D flipNormals_Graphic;
	private Texture2D setPivot_Graphic;
	private Texture2D texture_Graphic;
	private Texture2D world_OnGraphic; 
	private Texture2D nodraw_OnGraphic;
	private Texture2D merge_Graphic;
	private Texture2D world_OffGraphic;
	private Texture2D nodraw_OffGraphic;
	private Texture2D subdivide_Graphic;
	#endif
#endregion

#region CONSTANT & GUI MEMEBRS
	
	// because editor prefs can change, or shortcuts may be added, certain EditorPrefs need to be force reloaded.
	// adding to this const will force update on updating packages.
	const int EDITOR_PREF_VERSION = 19;

	// private Rect MODE_RECT = new Rect(3,50,46,46);
	#if !PROTOTYPE
	const int EDITLEVEL_TOOLBAR_WIDTH = 333;
	#else
	const int EDITLEVEL_TOOLBAR_WIDTH = 222;
	#endif
	int EDITLEVEL_TOOLBAR_START_Y = 0;

	private int BUTTON_WIDTH = 46;
	const string SHARED_GUI = "Assets/6by7/Shared/GUI";
	const float HANDLE_DRAW_DISTANCE = 15f;
	
	#if PROTOTYPE
	static Vector2 FLOATING_SCREEN_SIZE = new Vector2(52, 362);
	static Vector2 DOCKED_SCREEN_SIZE = new Vector2(52, 361);
	#else
	static Vector2 FLOATING_SCREEN_SIZE = new Vector2(52, 645);	// 674
	static Vector2 DOCKED_SCREEN_SIZE = new Vector2(52, 643);	// 673
	#endif

	const int SELECT_MODE_LENGTH = 3;	// There are actually 3, but edges aren't quite working yet.

	Color pbButtonColor = new Color(.35f, .35f, .35f, 1f);
	Color SCENE_TOOLBAR_ACTIVE_COLOR;// Set in OnEnable because it depends on pro/free skin

	GUIStyle pbStyle;
	GUIStyle VertexTranslationInfoStyle;
	Color VertexTranslationInfoBackgroundColor = new Color(.35f, .35f, .35f, .4f);
	// private int UNITY_VERSION;
#endregion

#region DEBUG

	#if SVN_EXISTS && !RELEASE
	string revisionNo = " no svn found";
	#endif
#endregion

#region LOCAL MEMBERS && EDITOR PREFS
	private static pb_Editor me;

	MethodInfo IntersectRayMesh;
	MethodInfo findNearestVertex;

	public EditLevel editLevel = EditLevel.Top;
	public SelectMode selectionMode { get; private set; }

	public HandleAlignment handleAlignment = HandleAlignment.World;

	public bool drawHandles = true;

	pb_Shortcut[] shortcuts;

	public bool vertexSelectionMask = true;	///< If true, in EditMode.ModeBased && SelectionMode.Vertex only vertices will be selected when dragging.
	public bool drawVertexNormals = false;
	public bool drawFaceNormals = false;

	private bool limitFaceDragCheckToSelection = true;

#endregion

#region INITIALIZATION AND ONDISABLE

	public void OnEnable()
	{
		me = this;

		#if UNITY_4_3
			Undo.undoRedoPerformed += this.UndoRedoPerformed;
		#endif

		SharedProperties.PushToGridEvent += PushToGrid;

		HookSceneViewDelegate();

		// make sure load prefs is called first, because other methods depend on the preferences set here
		LoadPrefs();

		InitGUI();

		// checks for duplicate meshes created while probuilder was not open
		SceneWideDuplicateCheck();

		// EditorUtility.UnloadUnusedAssets();
		ToggleEntityVisibility(EntityType.Detail, true);

		UpdateSelection();

		IntersectRayMesh = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
		findNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
		
		// EDITLEVEL_TOOLBAR_START_Y = PlayerSettings.renderingPath == RenderingPath.DeferredLighting ? 36 : 0;
	}

	private Color selectedVertexColor = Color.green;
	private Color defaultVertexColor = Color.blue;
	public void InitGUI()
	{	
		selectedVertexColor = pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultSelectedVertexColor);
		defaultVertexColor = pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultVertexColor);
 		
 		SCENE_TOOLBAR_ACTIVE_COLOR = EditorGUIUtility.isProSkin ? new Color(.35f, .35f, .35f, 1f) : new Color(.8f, .8f, .8f, 1f);

 		pbStyle = new GUIStyle();

		VertexTranslationInfoStyle = new GUIStyle();
		VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
		VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
		VertexTranslationInfoStyle.padding = new RectOffset(3,3,3,0);

		plane_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Handle-Plane", typeof(Texture2D)));
		local_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Handle-Local", typeof(Texture2D)));
		global_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Handle-Global", typeof(Texture2D)));
		
		hgraphic = global_Graphic;
		
		// acg_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_ACG", typeof(Texture2D)));
		#if !PROTOTYPE
		lightmap_Graphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Lightmap", typeof(Texture2D)));
		merge_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Merge", typeof(Texture2D)));
		texture_Graphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mode-Texture", typeof(Texture2D)));
		#endif
		face_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mode-Face", typeof(Texture2D)));
		select_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mode-Select", typeof(Texture2D)));
		vertex_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mode-Vertex", typeof(Texture2D)));
		edge_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mode-Edge", typeof(Texture2D)));
		shape_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Shape", typeof(Texture2D)));
		
		extrude_Graphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Extrude", typeof(Texture2D)));
		#if !PROTOTYPE
		mirror_Graphic 		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Mirror", typeof(Texture2D)));
		flipNormals_Graphic = (Texture2D)(Resources.Load("GUI/ProBuilderGUI_FlipNormals", typeof(Texture2D)));
		setPivot_Graphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_SetPivot", typeof(Texture2D)));
		#endif
		colorPanel_Graphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_ColorPanel", typeof(Texture2D)));
		
		detail_OnGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_detail-on", typeof(Texture2D)));
		mover_OnGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_mover-on", typeof(Texture2D)));
		#if !PROTOTYPE
		world_OnGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_world-on", typeof(Texture2D)));
		nodraw_OnGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_nodraw-on", typeof(Texture2D)));
		#endif
		collision_OnGraphic = (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_collision-on", typeof(Texture2D)));
		trigger_OnGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_trigger-on", typeof(Texture2D)));
		
		detail_OffGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_detail-off", typeof(Texture2D)));
		mover_OffGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_mover-off", typeof(Texture2D)));
		#if !PROTOTYPE
		world_OffGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_world-off", typeof(Texture2D)));
		nodraw_OffGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_nodraw-off", typeof(Texture2D)));
		subdivide_Graphic	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_Subdivide", typeof(Texture2D)));
		#endif
		collision_OffGraphic= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_collision-off", typeof(Texture2D)));
		trigger_OffGraphic 	= (Texture2D)(Resources.Load("GUI/ProBuilderToggles/ProBuilderGUI_trigger-off", typeof(Texture2D)));
		
		dgraphic = detail_OnGraphic;
		mgraphic = mover_OnGraphic;
		#if !PROTOTYPE
		wgraphic = world_OnGraphic;
		ndgraphic = nodraw_OnGraphic;
		#endif
		cgraphic = collision_OnGraphic;
		tgraphic = trigger_OnGraphic;
		
		modeGraphic = select_Graphic;
		
		show_NoDraw = true;
		
		show_Detail = true;
		show_Occluder = true;
		show_Mover = true;
		show_Collider = true;
		show_Trigger = true;

		if((EditorPrefs.HasKey(pb_Constant.pbDefaultOpenInDockableWindow) && 
			!EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow)))
		{
			this.minSize = FLOATING_SCREEN_SIZE;
			this.maxSize = FLOATING_SCREEN_SIZE;
		}
		else
		{
			this.minSize = DOCKED_SCREEN_SIZE;
			this.maxSize = DOCKED_SCREEN_SIZE;
		}
	}

	public void OnFocus()
	{
		// SVN
		#if SVN_EXISTS
		revisionNo = SvnManager.GetRevisionNumber();
		#endif
	}
	
	public void LoadPrefs()
	{
		// this exists to force update preferences when updating packages
		if(!EditorPrefs.HasKey(pb_Constant.pbEditorPrefVersion) || EditorPrefs.GetInt(pb_Constant.pbEditorPrefVersion) < EDITOR_PREF_VERSION ) {
			EditorPrefs.SetInt(pb_Constant.pbEditorPrefVersion, EDITOR_PREF_VERSION);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultShortcuts);
			Debug.LogWarning("ProBuilder: Clearing shortcuts. There were some internal changes in this release that required we rebuild this cache.  This will only happen once, and everything else is okay.\n\nWell, except whatever custom shortcut keys you set.  Those are nuked.");
		}

		editLevel 			= pb_Preferences_Internal.GetEnum<EditLevel>(pb_Constant.pbDefaultEditLevel);
		selectionMode		= pb_Preferences_Internal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
		handleAlignment		= pb_Preferences_Internal.GetEnum<HandleAlignment>(pb_Constant.pbHandleAlignment);
		
		shortcuts 			= pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts));
		limitFaceDragCheckToSelection = pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);
	}

	public void OnDisable()
	{
		ClearFaceSelection();
		UpdateSelection();

		SharedProperties.PushToGridEvent -= PushToGrid;

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

		#if UNITY_4_3
			Undo.undoRedoPerformed -= this.UndoRedoPerformed;
		#endif

		EditorPrefs.SetInt(pb_Constant.pbHandleAlignment, (int)handleAlignment);
		// pbUtil.ParseEnum(EditorPrefs.GetString(pb_Constant.pbHandleAlignment), handleAlignment);

		pb_Editor_Graphics.OnDisable();
		// EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();	// this was fixed in 4.1.2
	}

	public void OnDestroy()
	{
		// SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}
#endregion

#region EVENT HANDLERS
	
	public delegate void OnSelectionUpdateEventHandler(pb_Object[] selection);
	public static event OnSelectionUpdateEventHandler OnSelectionUpdate;

	public delegate void OnVertexMovementFinishedEventHandler(pb_Object[] selection);
	public static event OnVertexMovementFinishedEventHandler OnVertexMovementFinished;

	public void HookSceneViewDelegate()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;	// fuuuuck yooou lightmapping window
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
	}
#endregion

#region ONGUI

	public void OnInspectorUpdate()
	{
		if(EditorWindow.focusedWindow != this)
			Repaint();
	}

	Color bgColor;
	bool guiInitialized = false;
	void OnGUI()
	{
		if(!guiInitialized) 
		{
			GUISkin t_skin = EditorGUIUtility.GetBuiltinSkin(EditorGUIUtility.isProSkin ? EditorSkin.Scene : EditorSkin.Inspector);
			pbStyle.padding = new RectOffset(2,2,1,1);
			pbStyle.border = new RectOffset(6, 6, 4, 4);
			pbStyle.normal.background = t_skin.GetStyle("Button").normal.background;
			pbStyle.margin = new RectOffset(4,2,2,2);
			pbStyle.contentOffset = new Vector2(0,0);
		}
		
		if(!EditorGUIUtility.isProSkin)
		{
			bgColor = GUI.backgroundColor;
			GUI.backgroundColor = pbButtonColor;
		}

		GUILayout.Space(4);
		GUILayout.BeginVertical();
		
		if(GUILayout.Button(new GUIContent(hgraphic, "Toggle between Global, Local, and Plane Coordinates"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			ToggleHandleAlignment();
			pb_Editor_Utility.ShowNotification("Handle Alignment: " + ((HandleAlignment)handleAlignment).ToString());
		}
				
		if(GUILayout.Button(new GUIContent(modeGraphic, "Toggles between vertex and face selection mode"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			ToggleSelectionMode();

			switch(selectionMode)
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

		if(GUILayout.Button(new GUIContent(shape_Graphic, "Create New Shape"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
			OpenGeometryInterface();

		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif
		
		#if !PROTOTYPE
		if(GUILayout.Button(new GUIContent(merge_Graphic, "Merge Selection"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			int option = EditorUtility.DisplayDialogComplex(
				"Save or Delete Originals?",
				"Saved originals will be deactivated and hidden from the Scene, but available in the Hierarchy.",
				"Merge Delete",
				"Merge Save",
				"Cancel");

			pb_Object pb = null;

			if(option == 2) return;

			if( pbMeshOps.CombineObjects(selection, out pb) )
			{
				pb_Editor_Utility.SetEntityType(EntityType.Detail, pb.gameObject);
				pb_Lightmap_Editor.SetObjectUnwrapParamsToDefault(pb);			
				pb.gameObject.AddComponent<MeshCollider>().convex = false;
				pb.GenerateUV2(true);

				switch(option)
				{
					case 0: 	// Delete donor objects
						for(int i = 0; i < selection.Length; i++)
						{
							#if UNITY_4_3
					        Undo.DestroyObjectImmediate (selection[i].gameObject );
							#else
					        Undo.RegisterSceneUndo("Delete Merged Objects");
					        GameObject.DestroyImmediate(selection[i].gameObject);
							#endif
						}

						break;

					case 1: 	// Deactivate objects
						System.Type pbVB = Assembly.Load("Assembly-CSharp").GetTypes().First(t => t.Name == "pbVersionBridge");
						MethodInfo setActive = pbVB.GetMethod("SetActive");
						
						foreach(pb_Object sel in selection)
							setActive.Invoke(null, new object[2]{sel.gameObject, false});
						break;
				}
	
			}

			if(pb != null)
			Selection.activeTransform = pb.transform;

			UpdateSelection();
		}
		#endif

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif

		#if !PROTOTYPE
		if(GUILayout.Button(new GUIContent(lightmap_Graphic, "Adjust UV2 generation settings per object"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
			OpenLightmappingInterface();
		#endif

		if(GUILayout.Button(new GUIContent(extrude_Graphic, "Extrude"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			pb_Object[] pbs = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Extrude selected.");

			int extrudedFaceCount = 0;
			foreach(pb_Object pb in pbs)
			{
				switch(selectionMode)
				{
					case SelectMode.Face:
						if(pb.SelectedFaces.Length < 1) continue;
						
						extrudedFaceCount += pb.SelectedFaces.Length;
						
						pb.Extrude(pb.SelectedFaces, .5f);
						
						pb.SetSelectedFaces(pb.SelectedFaces);
						break;

					case SelectMode.Edge:
						if(pb.SelectedEdges.Length < 1) continue;
						extrudedFaceCount = pb.SelectedEdges.Length;
						pb_Edge[] newEdges = pb.Extrude(pb.SelectedEdges, .5f, pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeExtrusionOnly));
						if(newEdges == null)
							extrudedFaceCount = 0;
						else
							pb.SetSelectedEdges(newEdges);
						break;
				}
	
				pb.GenerateUV2(show_NoDraw);
				pb.Refresh();
			}

			if(extrudedFaceCount > 0)
			{
				string val = "";
				if(selectionMode == SelectMode.Edge)
					val = (extrudedFaceCount > 1 ? extrudedFaceCount + " Edges" : "Edge");
				else
					val = (extrudedFaceCount > 1 ? extrudedFaceCount + " Faces" : "Face");
				pb_Editor_Utility.ShowNotification("Extrude " + val, "Extrudes the selected faces / edges.");
			}

			UpdateSelection();
		}

		#if !PROTOTYPE
		if(GUILayout.Button(new GUIContent(mirror_Graphic, "Mirror Selected Objects"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
			EditorWindow.GetWindow<pb_MirrorTool>(true, "Mirror Tool", true);

		if(GUILayout.Button(new GUIContent(flipNormals_Graphic, "Flip Normals"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			pbUndo.RecordObjects(selection, "Flip Face Normals.");

			if(selectedVertexCount > 0)
			{
				foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
					pb.ReverseWindingOrder(pb.SelectedFaces.ToArray());
			}
			else
			{
				foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
					pb.ReverseWindingOrder(pb.faces);

			}
		}

		if(GUILayout.Button(new GUIContent(subdivide_Graphic, "Subdivide"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			int success = 0;
			foreach(pb_Object pb in selection)
			{
				pbUndo.RecordObject(pb, "Subdivide Face");


				if(pb.SelectedFaces.Length > 0)
				{
					pb_Face[] faces;
					if( pb.SubdivideFace(pb.SelectedFaces, out faces) )
					{
						success++;
						pb.SetSelectedFaces(faces);
					}
				}
				else
				{
					if( pb.Subdivide() )
						success++;
				}

				pb.Refresh();
				pb.GenerateUV2(show_NoDraw);
			}

			if(success > 0)
			{
				pb_Editor_Utility.ShowNotification("Subdivide");
				UpdateSelection();
			}
		}

		if(GUILayout.Button(new GUIContent(setPivot_Graphic, "Set Pivot"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			pb_Editor_Utility.ShowNotification("Set Pivot", "Center pivot around current selection.");

	        if (selection.Length > 0)
	        {
				foreach (pb_Object pbo in selection)
				{
					pbUndo.RecordObject(pbo, "Set Pivot");

					if (pbo.SelectedTriangles.Length > 0)
					{
						pbo.CenterPivot(pbo.SelectedTriangles);
					}
					else
					{
						pbo.CenterPivot(null);
					}
				}
		
				SceneView.RepaintAll();
			}
		}
		#endif

		if(GUILayout.Button(new GUIContent(colorPanel_Graphic, "Open Color Panel"), pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
			EditorWindow.GetWindow<pb_VertexColorInterface>(true, "Vertex Colors", true);


		#if FREE
			GUI.enabled = false;
		#endif
		
		if(GUILayout.Button(dgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_Detail = !show_Detail;
			if(show_Detail)
				dgraphic = detail_OnGraphic;
			else
				dgraphic = detail_OffGraphic;
			
			ToggleEntityVisibility(EntityType.Detail, show_Detail);
			// ToggleDetailVisibility(show_Detail);
		}
		
		#if TORNADO_TWINS
			GUI.enabled = false;
		#endif

		#if !PROTOTYPE
		if(GUILayout.Button(wgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_Occluder = !show_Occluder;
			if(show_Occluder)
				wgraphic = world_OnGraphic;
			else
				wgraphic = world_OffGraphic;
			ToggleEntityVisibility(EntityType.Occluder, show_Occluder);
		}
		#endif
		
		if(GUILayout.Button(mgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_Mover = !show_Mover;
			if(show_Mover)
				mgraphic = mover_OnGraphic;
			else
				mgraphic = mover_OffGraphic;
			
			ToggleEntityVisibility(EntityType.Mover, show_Mover);
		}

		if(GUILayout.Button(cgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_Collider = !show_Collider;
			if(show_Collider)
				cgraphic = collision_OnGraphic;
			else
				cgraphic = collision_OffGraphic;
			ToggleEntityVisibility(EntityType.Collider, show_Collider);
		}

		if(GUILayout.Button(tgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_Trigger = !show_Trigger;
			if(show_Trigger)
				tgraphic = trigger_OnGraphic;
			else
				tgraphic = trigger_OffGraphic;
			ToggleEntityVisibility(EntityType.Trigger, show_Trigger);
		}

		#if TORNADO_TWINS
			GUI.enabled = true;
		#endif
		#if !PROTOTYPE
		if(GUILayout.Button(ndgraphic, pbStyle, GUILayout.MaxWidth(BUTTON_WIDTH)))
		{
			show_NoDraw = !show_NoDraw;

			ToggleNoDrawVisibility(show_NoDraw);
		}
		#endif
		#if FREE
			// GUI.skin = null;
			GUI.enabled = true;
		#endif

		// PROFILE && DEBUG
		#if PROFILE_TIMES
		GUI.skin = null;

		if(GUILayout.Button("times",GUILayout.MinWidth(20)))
		{
			updateSelectionProfiler.DumpTimes();
			profiler.DumpTimes();
		}

		if(GUILayout.Button("reset",GUILayout.MinWidth(20)))
		{
			updateSelectionProfiler.ClearValues();
			profiler.ClearValues();
		}
		#endif

		GUI.skin = null;
		GUI.backgroundColor = bgColor;
	}
#endregion

#region ONSCENEGUI

	// GUI Caches
	public pb_Object[] selection = new pb_Object[0];							// All selected pb_Objects
	
	int selectedVertexCount = 0;										// Sum of all vertices sleected

	// the mouse vertex selection box
	private Rect mouseRect = new Rect(0,0,0,0);

	// Constant
	Quaternion HANDLE_ROTATION = new Quaternion(0f, 0f, 0f, 1f);
	
	// Handles
	Tool currentHandle = Tool.Move;

	// Dragging
	Vector2 mousePosition_initial;
	Rect selectionRect;
	Color dragRectColor = new Color(.313f, .8f, 1f, 1f);
	private bool dragging = false;
	private bool doubleClicked = false;	// prevents leftClickUp from stealing focus after double click

	// vertex handles
	Vector3 newPosition, cachedPosition;
	bool movingVertices = false;

	// top level caching
	bool scaling = false;

	private Tool previousTool = (Tool)1;
	private bool rightMouseDown = false;
	public void OnSceneGUI(SceneView scnView)
	{	
		Event e = Event.current;
		
		if(e.Equals(Event.KeyboardEvent("v")))
		{
			e.Use();
			snapToVertex = true;
		}

		/**
		 * Snap stuff
		 */
		if(e.type == EventType.KeyUp)
			snapToVertex = false;

		if(e.type == EventType.MouseDown && e.button == 1)
			rightMouseDown = true;

		if(e.type == EventType.MouseUp && e.button == 1 || e.type == EventType.Ignore)
			rightMouseDown = false;

		#if PROFILE_TIMES
		profiler.LogStart("OnSceneGUI");
		#endif

		#if !UNITY_4_3
		if(Event.current.type == EventType.ValidateCommand)
		{
			OnValidateCommand(Event.current.commandName);
			#if PROFILE_TIMES
			profiler.LogFinish("OnSceneGUI");
			#endif
			return;
		}
		#endif

		#if PROFILE_TIMES
		profiler.LogStart("DrawHandleGUI");
		#endif

		DrawHandleGUI();

		#if PROFILE_TIMES
		profiler.LogFinish("DrawHandleGUI");
		#endif

		if(!rightMouseDown && getKeyUp != KeyCode.None)
		{
			if(ShortcutCheck())
			{
				e.Use();
				#if PROFILE_TIMES
				profiler.LogFinish("OnSceneGUI");
				#endif
				return;
			}
		}

		// Listen for top level movement
		#if PROFILE_TIMES
		profiler.LogStart("ListenForTopLevelMovement");
		#endif
		
		ListenForTopLevelMovement();
		
		#if PROFILE_TIMES
		profiler.LogFinish("ListenForTopLevelMovement");
		#endif

		#if PROFILE_TIMES
		profiler.LogStart("OnFinishedVertexModification");
		#endif

		// Finished moving vertices, scaling, or adjusting uvs
		#if PROTOTYPE
		if( (movingVertices || scaling) && GUIUtility.hotControl < 1)
		{
			OnFinishedVertexModification();
		}
		#else
		if( (movingVertices || movingPictures || scaling) && GUIUtility.hotControl < 1)
		{
			OnFinishedVertexModification();
			if(movingPictures)
				UpdateTextureHandles();
		}
		#endif
			
		#if PROFILE_TIMES
		profiler.LogFinish("OnFinishedVertexModification");
		#endif


		// Check mouse position in scene and determine if we should highlight something
		#if PROFILE_TIMES
		profiler.LogStart("UpdateMouse");
		#endif
		UpdateMouse(e.mousePosition);
		#if PROFILE_TIMES
		profiler.LogFinish("UpdateMouse");
		#endif

		#if PROFILE_TIMES
		profiler.LogStart("DrawHandles");
		#endif

		// Draw GUI Handles
		if(drawHandles && editLevel != EditLevel.Top)
			DrawHandles();

		// // Draw Dimensions
		// foreach(pb_Object pb in selection)
		// 	pb_Editor_GUI.DrawDimensions(pb);

		#if PROFILE_TIMES
		profiler.LogFinish("DrawHandles");
		#endif

		if(drawVertexNormals)
			DrawVertexNormals();

		if(drawFaceNormals)
			DrawFaceNormals();

		if( editLevel != EditLevel.Top && Tools.current != Tool.View)
		{
			if( Selection.transforms.Length > 0 ) 
			{
				if(editLevel == EditLevel.Geometry)
				{
					switch(currentHandle)
					{
						case Tool.Move:
							#if PROFILE_TIMES
							profiler.LogStart("VertexMoveTool");
							#endif
							VertexMoveTool();
							#if PROFILE_TIMES
							profiler.LogFinish("VertexMoveTool");
							#endif
							break;
						case Tool.Scale:
							VertexScaleTool();
							break;
						case Tool.Rotate:
							#if PROFILE_TIMES
							profiler.LogStart("VertexRotateTool");
							#endif
							VertexRotateTool();
							#if PROFILE_TIMES
							profiler.LogFinish("VertexRotateTool");
							#endif
							break;
					}
				}
				#if !PROTOTYPE	// TEXTURE HANDLES
				else if(editLevel == EditLevel.Texture && Selection.transforms.Length > 0)
				{
					switch(currentHandle)
					{
						case Tool.Move:
						#if PROFILE_TIMES
							profiler.LogStart("TextureMoveTool");
						#endif
							TextureMoveTool();
						#if PROFILE_TIMES
							profiler.LogFinish("TextureMoveTool");
						#endif
							break;
						case Tool.Rotate:
							TextureRotateTool();
							break;
						case Tool.Scale:
							TextureScaleTool();
							break;
					}
		 		}
		 		#endif
		 	}
		}
		else
		if(/*editMode == EditMode.ModeBased &&*/ editLevel == EditLevel.Top)
		{
			#if PROFILE_TIMES
			profiler.LogFinish("OnSceneGUI");
			#endif


			return;
		}

		// altClick || Tools.current == Tool.View || GUIUtility.hotControl > 0 || middleClick
		// Tools.viewTool == ViewTool.FPS || Tools.viewTool == ViewTool.Orbit
		if(earlyOut || Event.current.isKey)
		{
			dragging = false;

			#if PROFILE_TIMES
			profiler.LogFinish("OnSceneGUI");
			#endif
			
			return;
		}

		/* * * * * * * * * * * * * * * * * * * * *
		 *	 Vertex & Quad Wranglin' Ahead! 	 *
		 * 	 Everything from this point below	 *
		 *	 overrides something Unity related.  *
		 * * * * * * * * * * * * * * * * * * * * */

		// This prevents us from selecting other objects in the scene,
		// and allows for the selection of faces / vertices.
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);

		// If selection is made, don't use default handle -- set it to Tools.None
		if(selectedVertexCount > 0) {
			if(Tools.current != Tool.None) {
				previousTool = Tools.current;
				Tools.current = Tool.None;
				if(previousTool != Tool.View)
					SetTool(previousTool);
			}
		} else {	// If nothing is selected, revert back to previous handle
			if(Tools.current == Tool.None) {
				Tools.current = previousTool;
			}
		}

		if(leftClick) {
			// double clicking object
			if(Event.current.clickCount > 1)
			{
				pb_Object pb = RaycastCheck(e.mousePosition);
				if(pb != null)
				{
					pb.SetSelectedFaces(pb.faces);
					UpdateSelection();
					SceneView.RepaintAll();
					doubleClicked = true;
				}
			}

			mousePosition_initial = mousePosition;
		}

		if(mouseDrag)
			dragging = true;

		if(ignore)
		{
			if(dragging)
				DragCheck();
			if(doubleClicked)
				doubleClicked = false;
		}

		if(leftClickUp)
		{
			if(doubleClicked)
			{
				doubleClicked = false;
			}
			else
			{
				if(!dragging)
					RaycastCheck(e.mousePosition);
				else
				{
					dragging = false;
					DragCheck();
				}
			}
		}

		if(GUI.changed) {
			foreach(pb_Object pb in selection)
				EditorUtility.SetDirty(pb);
		}

		#if PROFILE_TIMES
		profiler.LogFinish("OnSceneGUI");
		#endif
	}
#endregion

#region RAYCASTING AND DRAGGING

	public const float MAX_EDGE_SELECT_DISTANCE = 7;
	int nearestEdgeObjectIndex = -1;
	int nearestEdgeIndex = -1;
	pb_Edge nearestEdge;	

	private void UpdateMouse(Vector3 mousePosition)
	{
		if(selectionLength < 1) return;

		switch(selectionMode)
		{
			// default:
			case SelectMode.Edge:

				// TODO
				float bestDistance = MAX_EDGE_SELECT_DISTANCE;				
				int bestEdgeIndex = -1;
				int objIndex = -1;

				for(int i = 0; i < selectionLength; i++)
				{
					pb_Edge[] edges = selected_uniqueEdges_all[i];
					
					for(int j = 0; j < edges.Length; j++)
					{
						// Undo operation doesn't refresh objects fast enough
						if(selection[i].sharedIndices.Length <= edges[j].x || selection[i].sharedIndices.Length <= edges[j].y)
							return;
						
						int x = selection[i].sharedIndices[edges[j].x][0];
						int y = selection[i].sharedIndices[edges[j].y][0];

						float d = HandleUtility.DistanceToLine(selected_verticesInWorldSpace_all[i][x], selected_verticesInWorldSpace_all[i][y]);
						
						if(d < bestDistance)
						{
							objIndex = i;
							bestEdgeIndex = j;
							bestDistance = d;
						}
					}
				}
				
				if(bestEdgeIndex != nearestEdgeIndex || objIndex != nearestEdgeObjectIndex)
				{
					nearestEdgeObjectIndex = objIndex;
					nearestEdgeIndex = bestEdgeIndex;

					if(nearestEdgeIndex > -1)
						nearestEdge = new pb_Edge(
							selection[objIndex].sharedIndices[selected_uniqueEdges_all[objIndex][nearestEdgeIndex].x][0],
							selection[objIndex].sharedIndices[selected_uniqueEdges_all[objIndex][nearestEdgeIndex].y][0]);

					SceneView.RepaintAll();
				}
				break;
		}
	}

	// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
	// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
	private pb_Object RaycastCheck(Vector3 mousePosition)
	{
		GameObject nearestGameObject = HandleUtility.PickGameObject(mousePosition, false);
		pb_Object pb;
		if(nearestGameObject)
		{
			// if we clicked a pb_Object
			pb = nearestGameObject.GetComponent<pb_Object>();
			if(pb && pb.isSelectable)
			{
				// check for shift key.  if not, change selection to clicked object
				if(!shiftKey && !ctrlKey)
				{
					// assuming that it is not the currently selected
					if(nearestGameObject != Selection.activeGameObject)
					{
						Exit(nearestGameObject);
						// Uncomment to require object selection before editing
						// 	return pb;
					}
					else
					{
					// otherwise, set the selection to just the nearest gameObject
					// and continue
						SetSelection(nearestGameObject);
					}
				}
				else
				// if shift key is held, and we haven't already selected this object, add it to the selection
				if(shiftKey || ctrlKey)
				{
					// if adding, don't allow for face selection.  if it's already selected, move on to face selection
					if(!Selection.Contains(nearestGameObject))
					{
						AddToSelection(nearestGameObject);
					}
				}
			}
			else 
			{
			  // Check prefrences to see if only Probuilder objects are selectable
			  if (pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly))
			  {
				  if (nearestGameObject.GetComponent<pb_Object>())
				  {
					  Exit(nearestGameObject);
				  }
			  }
			  else
			  {
				  Exit(nearestGameObject);
			  }

			  return null;
			}
		}
		// Clicked off gameObject.  Return selection to native functions.
		else
		{
			if(selectionMode == SelectMode.Vertex)
			{
				if(!VertexClickCheck(out pb))
				{
					Exit();
					return null;
				}
				else
				{
					UpdateSelection();
					SceneView.RepaintAll();
					return pb;
				}
			}
			else if(selectionMode == SelectMode.Edge)
			{
				if(!EdgeClickCheck(mousePosition, out pb))
				{
					Exit();
					return null;
				}
				else
				{
					UpdateSelection();
					SceneView.RepaintAll();
					return pb;
				}
			}
			else
			{
				Exit();
				return null;
			}
		}
		
		Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
		RaycastHit hit;

		// if( Physics.Raycast(ray.origin, ray.direction, out hit))
		object[] parameters = new object[] { ray, pb.msh, pb.transform.localToWorldMatrix, null };
		if(IntersectRayMesh == null) IntersectRayMesh = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
		object result = IntersectRayMesh.Invoke(this, parameters);	

		if ( (bool)result )
			hit = (RaycastHit)parameters[3];
		else
			return pb;

		pb_Object vpb;
		/**
		 *	If in vertex selection mode, check for a vertex click before checking for a face.
		 */
		if(selectionMode == SelectMode.Vertex && VertexClickCheck(out vpb))
		{
			pb = vpb;
		}
		// Then check if edge mode, and test for edgeVertices
		else
		if(selectionMode == SelectMode.Edge && EdgeClickCheck(mousePosition, out vpb))
		{
			pb = vpb;
		}
		// finally, fall back on face selection
		else
		{
			// Check for face hit
			Mesh m = pb.msh;

			int[] tri = new int[3] {
				m.triangles[hit.triangleIndex * 3 + 0], 
				m.triangles[hit.triangleIndex * 3 + 1], 
				m.triangles[hit.triangleIndex * 3 + 2] 
			};
			
			pb_Face selectedFace;
			if( pb.FaceWithTriangle(tri, out selectedFace) )
			{
				// Check to see if we've already selected this quad.  If so, remove it from selection cache.
				pbUndo.RecordObject(pb, "Change Face Selection");

				int indx = System.Array.IndexOf(pb.SelectedFaces, selectedFace);
				if( indx > -1 ) {
					pb.RemoveFromFaceSelectionAtIndex(indx);
				} else {
					pb.AddToFaceSelection(selectedFace);
				}
			}
		}

		Event.current.Use();

		UpdateSelection();
		SceneView.RepaintAll();

		return pb;
	}

	private bool VertexClickCheck(out pb_Object vpb)
	{
		if(!shiftKey && !ctrlKey) ClearFaceSelection();
		

		for(int i = 0; i < selectionLength; i++)
		{
			pb_Object pb = selection[i];
			if(!pb.isSelectable) continue;

			for(int n = 0; n < selected_uniqueIndices_all[i].Length; n++)
			{
				Vector3 v = selected_verticesInWorldSpace_all[i][selected_uniqueIndices_all[i][n]];

				if(mouseRect.Contains(HandleUtility.WorldToGUIPoint(v)))
				{
					// Check if index is already selected, and if not add it to the pot
					int indx = System.Array.IndexOf(pb.SelectedTriangles, selected_uniqueIndices_all[i][n]);

					// @all vertex handles
					if(!Selection.Contains(pb.gameObject))
						AddToSelection(pb.gameObject);

					// If we get a match, check to see if it exists in our selection array already, then add / remove
					if( indx > -1 )
						pb.SetSelectedTriangles(pb.SelectedTriangles.RemoveAt(indx));
					else
						pb.SetSelectedTriangles(pb.SelectedTriangles.Add(selected_uniqueIndices_all[i][n]));

					vpb = pb;
					return true;
				}
			}
		}

		vpb = null;
		return false;
	}

	private bool EdgeClickCheck(Vector3 hitPoint, out pb_Object pb)
	{
		if(nearestEdgeObjectIndex > -1)
		{
			pb = selection[nearestEdgeObjectIndex];

			if(!shiftKey && !ctrlKey) pb.ClearSelection();

			if(nearestEdgeIndex > -1)
			{
				int ind = pb.SelectedEdges.IndexOf(nearestEdge, pb.sharedIndices);
				// int ind = System.Array.IndexOf(pb.SelectedEdges, nearestEdge);
				
				if( ind > -1 )
					pb.SetSelectedEdges(pb.SelectedEdges.RemoveAt(ind));
				else
					pb.SetSelectedEdges(pb.SelectedEdges.Add(nearestEdge));
			}

			return true;
		}
		else
		{
			if(!shiftKey && !ctrlKey) ClearFaceSelection();
			pb = null;
			return false;
		}
	}

	private void DragCheck()
	{
		Camera cam = SceneView.lastActiveSceneView.camera;
		
		switch(selectionMode)
		{
			case SelectMode.Vertex:
			{
				if(!shiftKey && !ctrlKey) ClearFaceSelection();
				
				for(int i = 0; i < selectionLength; i++)
				{
					pb_Object pb = selection[i];
					if(!pb.isSelectable) continue;

					List<int> selectedTriangles = new List<int>(pb.SelectedTriangles);

					for(int n = 0; n < selected_uniqueIndices_all[i].Length; n++)
					{
						Vector3 v = selected_verticesInWorldSpace_all[i][selected_uniqueIndices_all[i][n]];

						if(selectionRect.Contains(HandleUtility.WorldToGUIPoint(v)))
						{
							// if point is behind the camera, ignore it.
							if(cam.WorldToScreenPoint(v).z < 0)
								continue;
							
							// Check if index is already selected, and if not add it to the pot
							int indx = selectedTriangles.IndexOf(selected_uniqueIndices_all[i][n]);

							if(indx > -1)
								selectedTriangles.RemoveAt(indx);//(selected_uniqueIndices_all[i][n]);
							else
								selectedTriangles.Add(selected_uniqueIndices_all[i][n]);

							// @all vertex handles
							// if(!Selection.Contains(pb.gameObject))
							// 	AddToSelection(pb.gameObject);
						}
					}
					
					pb.SetSelectedTriangles(selectedTriangles.ToArray());
				}

				if(!vertexSelectionMask)
					DragObjectCheck(true);

				UpdateSelection();
			}
			break;

			case SelectMode.Face:
			{
				if(!shiftKey && !ctrlKey) ClearFaceSelection();

				pb_Object[] pool = limitFaceDragCheckToSelection ? selection : (pb_Object[])FindObjectsOfType(typeof(pb_Object));
			
				List<pb_Face> selectedFaces;
				for(int i = 0; i < pool.Length; i++)
				{
					pb_Object pb = pool[i];
					selectedFaces = new List<pb_Face>(pb.SelectedFaces);

					if(!pb.isSelectable) continue;

					Vector3[] verticesInWorldSpace = limitFaceDragCheckToSelection ? selected_verticesInWorldSpace_all[i] : pb.VerticesInWorldSpace();
					bool addToSelection = false;
					for(int n = 0; n < pb.faces.Length; n++)
					{
						pb_Face face = pb.faces[n];
						// only check the first index per quad, and if it checks out, then check every other point
						if(selectionRect.Contains(HandleUtility.WorldToGUIPoint(verticesInWorldSpace[face.indices[0]])))
						{
							bool nope = false;
							for(int q = 0; q < pb.faces[n].distinctIndices.Length; q++)
							{
								if(!selectionRect.Contains(HandleUtility.WorldToGUIPoint(verticesInWorldSpace[face.distinctIndices[q]])))
								{
									nope = true;
									break;
								}
							}

							if(!nope)
							{
								int indx =  selectedFaces.IndexOf(face);
								
								if( indx > -1 ) {
									selectedFaces.RemoveAt(indx);
								} else {
									addToSelection = true;
									selectedFaces.Add(face);
								}
							}
						}
					}

					pb.SetSelectedFaces(selectedFaces.ToArray());
					if(addToSelection)
						AddToSelection(pb.gameObject);
				}

				DragObjectCheck(true);

				UpdateSelection();
			}
			break;

			case SelectMode.Edge:
			{
				#if PROFILE_TIMES
				profiler.LogStart("Drag Select Edges");
				#endif

				if(!shiftKey && !ctrlKey) ClearFaceSelection();

				for(int e = 0; e < selectionLength; e++)
				{
					pb_Object pb = selection[e];
					List<pb_Edge> inSelection = new List<pb_Edge>();

					for(int i = 0; i < selected_uniqueEdges_all[e].Length; i++)
					{
						pb_Edge edge = new pb_Edge(
							pb.sharedIndices[selected_uniqueEdges_all[e][i].x][0],
							pb.sharedIndices[selected_uniqueEdges_all[e][i].y][0]);

						if( selectionRect.Contains(HandleUtility.WorldToGUIPoint(selected_verticesInWorldSpace_all[e][edge.x])) &&
							selectionRect.Contains(HandleUtility.WorldToGUIPoint(selected_verticesInWorldSpace_all[e][edge.y])))
							inSelection.Add(edge);
					}

					List<pb_Edge> add = new List<pb_Edge>();
					List<int> remove = new List<int>();
					foreach(pb_Edge edge in inSelection.Distinct())
					{
						int ind = pb.SelectedEdges.IndexOf(edge, pb.sharedIndices);
						// int ind = System.Array.IndexOf(pb.SelectedEdges, edge);
						
						if(ind > -1)
							remove.Add(ind);
						else
							add.Add(edge);
					}

					List<pb_Edge> priorSelection = new List<pb_Edge>(pb.SelectedEdges.RemoveAt(remove.ToArray()));
					priorSelection.AddRange(add);
					pb.SetSelectedEdges(priorSelection.ToArray());
				}

				if(!vertexSelectionMask)
					DragObjectCheck(true);

				#if PROFILE_TIMES
				profiler.LogFinish("Drag Select Edges");
				#endif				
				UpdateSelection();
			}
			break;

		default:
			DragObjectCheck(false);
			break;
		}

		SceneView.RepaintAll();
	}

	// Emulates the usual Unity drag to select objects functionality
	private void DragObjectCheck(bool vertexMode)
	{
		// if we're in vertex selection mode, only add to selection if shift key is held, 
		// and don't clear the selection if shift isn't held.
		// if not, behave regularly (clear selection if shift isn't held)
		if(!vertexMode) {
			if(!shiftKey) ClearSelection();
		} else {
			if(!shiftKey && selectedVertexCount > 0) return;
		}

		// scan for new selected objects
		/// if mode based, don't allow selection of non-probuilder objects
		foreach(pb_Object g in HandleUtility.PickRectObjects(selectionRect).GetComponents<pb_Object>())
			if(!Selection.Contains(g.gameObject))
				AddToSelection(g.gameObject);
	}
#endregion

#region VERTEX TOOLS

	private bool snapToVertex = false;
	private Vector3 previousHandleScale = Vector3.one;
	private Vector3 currentHandleScale = Vector3.one;
	private Vector3[][] vertexOrigins;
	private Vector3[] vertexOffset;
	private Quaternion previousHandleRotation = Quaternion.identity;
	private Quaternion currentHandleRotation = Quaternion.identity;
	
	private Vector3 translateOrigin = Vector3.zero;
	private Vector3 rotateOrigin = Vector3.zero;
	private Vector3 scaleOrigin = Vector3.zero;

	private void VertexMoveTool()
	{
		newPosition = selected_handlePivotWorld;
		cachedPosition = newPosition;

		#if !UNITY_4_3
		Undo.ClearSnapshotTarget();
		#endif

		newPosition = Handles.PositionHandle(newPosition, handleRotation);

		bool previouslyMoving = movingVertices;
		if(newPosition != cachedPosition)
		{
			Vector3 diff = newPosition-cachedPosition;

			Vector3 mask = new Vector3(
				Mathf.Abs(diff.x) > .0001f ? 1f : 0f,
				Mathf.Abs(diff.y) > .0001f ? 1f : 0f,
				Mathf.Abs(diff.z) > .0001f ? 1f : 0f);

			Vector3 v;
			if(snapToVertex && FindNearestVertex(mousePosition, out v))
				diff = Vector3.Scale(v-cachedPosition, mask);

			movingVertices = true;
			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				#if !UNITY_4_3
				Undo.SetSnapshotTarget(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Move Vertices");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();
			}

			#if UNITY_4_3
				Undo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Move Vertices");
			#endif

			for(int i = 0; i < selectionLength; i++)
			{
				selection[i].TranslateVertices(selection[i].SelectedTriangles, diff);
				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
			}

			Internal_UpdateSelectionFast();
		}
	}

	private void VertexScaleTool()
	{
		newPosition = selected_handlePivotWorld;

		previousHandleScale = currentHandleScale;

		#if !UNITY_4_3
		Undo.ClearSnapshotTarget();
		#endif
	
		currentHandleScale = Handles.ScaleHandle(currentHandleScale, newPosition, handleRotation, HandleUtility.GetHandleSize(newPosition));

		bool previouslyMoving = movingVertices;
	
		if(previousHandleScale != currentHandleScale)
		{
			movingVertices = true;
			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				#if !UNITY_4_3
				Undo.SetSnapshotTarget(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Scale Vertices");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				// cache vertex positions for scaling later
				vertexOrigins = new Vector3[selection.Length][];
				vertexOffset = new Vector3[selection.Length];

				for(int i = 0; i < selection.Length; i++)
				{	
					// vertexOrigins[i] = new Vector3[selection[i].SelectedTriangles.Length];
					vertexOrigins[i] = selection[i].GetVertices(selection[i].SelectedTriangles);
					vertexOffset[i] = pb_Math.Average(vertexOrigins[i]);
				}
			}

			Vector3 ver;	// resulting vertex from modification
			Vector3 over;	// vertex point to modify. different for world, local, and plane
			
			#if UNITY_4_3
				Undo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Scale Vertices");
			#endif

			for(int i = 0; i < selection.Length; i++)
			{
				for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
				{
					switch(handleAlignment)
					{
						case HandleAlignment.Plane:

							if(vertexOrigins[i].Length < 3)
								goto case HandleAlignment.Local;

							Quaternion localRot = Quaternion.identity;

							// get the plane rotation in local space
							Vector3 nrm = pb_Math.Normal(vertexOrigins[i]);
							localRot = Quaternion.LookRotation(nrm, Vector3.up);	
							// move center of vertices to 0,0,0 and set rotation as close to identity as possible
							
							over = Quaternion.Inverse(localRot) * (vertexOrigins[i][n] - vertexOffset[i]);

							// apply scale
							ver = Vector3.Scale(over, currentHandleScale);
							// re-apply original rotation
							if(vertexOrigins[i].Length > 2)
								ver = localRot * ver;
							// re-apply world position offset
							ver += vertexOffset[i];
							// set the vertex in local space
							selection[i].SetSharedVertexPosition(selection[i].SelectedTriangles[n], ver);
							
							break;

						case HandleAlignment.World:
						case HandleAlignment.Local:
							// move vertex to relative origin from center of selection
							over = vertexOrigins[i][n] - vertexOffset[i];
							// apply scale
							ver = Vector3.Scale(over, currentHandleScale);
							// move vertex back to locally offset position
							ver += vertexOffset[i];
							// set vertex in local space on pb-Object
							selection[i].SetSharedVertexPosition(selection[i].SelectedTriangles[n], ver);
							break;
					}
				}
			
				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
			}

			Internal_UpdateSelectionFast();
		}
	}

	private void VertexRotateTool()
	{
		newPosition = selected_handlePivotWorld;

		previousHandleRotation = currentHandleRotation;

		#if !UNITY_4_3
		Undo.ClearSnapshotTarget();
		#endif

		currentHandleRotation = Handles.RotationHandle(currentHandleRotation, newPosition);

		bool previouslyMoving = movingVertices;

		if(currentHandleRotation != previousHandleRotation)
		{
			movingVertices = true;
			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;
				
				#if !UNITY_4_3
				Undo.SetSnapshotTarget(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Rotate Vertices");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				// cache vertex positions for scaling later
				vertexOrigins = new Vector3[selection.Length][];
				vertexOffset = new Vector3[selection.Length];

				for(int i = 0; i < selection.Length; i++)
				{					
					vertexOrigins[i] = selection[i].GetVertices(selection[i].SelectedTriangles).ToArray();
					vertexOffset[i] = pb_Math.Average(vertexOrigins[i]);
				}
			}
			
			#if UNITY_4_3
				Undo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Scale Vertices");
			#endif

			Vector3 ver;	// resulting vertex from modification
			Vector3 over;	// vertex point to modify. different for world, local, and plane
			for(int i = 0; i < selection.Length; i++)
			{
				Vector3 nrm = (vertexOrigins[i].Length > 2) ? pb_Math.Normal(vertexOrigins[i]) : Vector3.zero;
				
				Quaternion lr = selection[i].transform.localRotation;
				Quaternion pr = nrm == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(nrm, Vector3.up);
	
				Quaternion ro;

				for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
				{
					switch(handleAlignment)
					{
						case HandleAlignment.Plane:
							
							if(vertexOrigins[i].Length < 3)
								goto case HandleAlignment.Local;

							over = vertexOrigins[i][n] - vertexOffset[i];

							over = Quaternion.Inverse( pr ) * over;

							ver = (Quaternion.Inverse(lr)*currentHandleRotation) * over;

							ver += vertexOffset[i];

							selection[i].SetSharedVertexPosition(selection[i].SelectedTriangles[n], ver);

							break;

						case HandleAlignment.World:	// ahh fuck it
						case HandleAlignment.Local:
							// move vertex to relative origin from center of selection
							over = vertexOrigins[i][n] - vertexOffset[i];

							// apply scale
							ro = Quaternion.Inverse(lr) * currentHandleRotation;
							ver = ro * over;
							// move vertex back to locally offset position
							ver += vertexOffset[i];
							// set vertex in local space on pb-Object
							selection[i].SetSharedVertexPosition(selection[i].SelectedTriangles[n], ver);
							break;
					}

				}

				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
			}

			// don't modify the handle rotation because otherwise rotating with plane coordinates
			// updates the handle rotation with every change, making moving things a changing target
			Quaternion rotateToolHandleRotation = currentHandleRotation;
			
			Internal_UpdateSelectionFast();
			
			currentHandleRotation = rotateToolHandleRotation;
		}
	}

	private void ShiftExtrude()
	{
		pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Shift+Extrude Faces");

		int ef = 0;
		foreach(pb_Object pb in selection)
		{
			int len = selectionMode == SelectMode.Face ? pb.SelectedFaces.Length : pb.SelectedEdges.Length;
			if(len < 1) 
				continue;

			ef += len;

			switch(selectionMode)
			{
				case SelectMode.Vertex:
					goto case SelectMode.Face;

				case SelectMode.Face:
					pb.Extrude(pb.SelectedFaces, 0f);
					pb.SetSelectedFaces(pb.SelectedFaces);
					break;

				case SelectMode.Edge:
					if(pb.SelectedFaces.Length > 0)
						goto case SelectMode.Face;

					pb_Edge[] newEdges = pb.Extrude(pb.SelectedEdges, 0f, pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeExtrusionOnly));

					if(newEdges != null)
						pb.SetSelectedEdges(newEdges);
					else
						ef -= len;
					break;
			}
		}

		if(ef > 0)
		{
			pb_Editor_Utility.ShowNotification("Extrude");
			UpdateSelection();
		}
	}

	private bool FindNearestVertex(Vector2 mousePosition, out Vector3 vertex)
	{
		
		List<Transform> t = new List<Transform>((Transform[])pbUtil.GetComponents<Transform>(HandleUtility.PickRectObjects(new Rect(0,0,Screen.width,Screen.height))));
		
		GameObject nearest = HandleUtility.PickGameObject(mousePosition, false);
		if(nearest != null)
			t.Add(nearest.transform);

		object[] parameters = new object[] { (Vector2)mousePosition, t.ToArray(), null };
		if(findNearestVertex == null) findNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
		object result = findNearestVertex.Invoke(this, parameters);	
		vertex = (bool)result ? (Vector3)parameters[2] : Vector3.zero;
		return (bool)result;
	}

	#if !PROTOTYPE
	Vector3 texHandleCenter = Vector3.zero;
	Vector3 texHandleCenter_world = Vector3.zero;
	Vector3 texPos = Vector3.zero, t_texPos = Vector3.zero;
	bool movingPictures;
	public void TextureMoveTool()
	{
		t_texPos = texPos;
 		
	 	Matrix4x4 pm = Handles.matrix;

		Handles.matrix = selectedFaceMatrix;

		texPos = Handles.PositionHandle(texPos, Quaternion.identity);
		
		Handles.matrix = pm;

		bool previouslyMoving_tex = movingPictures;
		if(texPos != t_texPos)
		{
			movingPictures = true;
			if(previouslyMoving_tex == false)
			{
				OnBeginTextureModification();

				#if !UNITY_4_3
				Undo.SetSnapshotTarget(selection as Object[], "Translate UVs");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif
			}

			Vector3 delta = t_texPos-texPos;

			#if UNITY_4_3
			Undo.RecordObjects(selection, "Move Texture UVs");
			#endif
			pb_Texture_Editor.TranslateOffset( delta, selection );
		}	
	}

	Quaternion texRotation = new Quaternion(0f, 0f, 0f, 1f);
	Quaternion t_texRotation;
	public void TextureRotateTool()
	{
	 	Matrix4x4 pm = Handles.matrix;

		float handleSize = HandleUtility.GetHandleSize(texHandleCenter_world);

		Handles.matrix = selectedFaceMatrix;

		t_texRotation = texRotation;

		// static Quaternion Disc(Quaternion rotation, Vector3 position, Vector3 axis, float size, bool cutoffPlane, float snap);
		Color c = Handles.color;
		Handles.color = Color.blue;
		texRotation = Handles.Disc(texRotation, Vector3.zero, Vector3.forward, handleSize, false, 0f);
		Handles.color = c;
		
		Handles.matrix = pm;

		bool previouslyMoving_tex = movingPictures;
		if(t_texRotation != texRotation)
		{
			movingPictures = true;
			if(previouslyMoving_tex == false)
			{
				
				OnBeginTextureModification();

				#if !UNITY_4_3
				Undo.SetSnapshotTarget(selection as Object[], "Translate UVs");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif
			}

			float delta = 0f;
			delta = t_texRotation.eulerAngles.z - texRotation.eulerAngles.z;

			#if UNITY_4_3
			Undo.RecordObjects(selection, "Rotate Texture UVs");
			#endif
			pb_Texture_Editor.TranslateRotation(delta, selection);
		}
	}

	Vector3 texScale = Vector3.one;
	Vector3 t_texScale = Vector3.one;
	float scaleModifier = .5f;
	public void TextureScaleTool()
	{
	 	Matrix4x4 pm = Handles.matrix;

		float handleSize = HandleUtility.GetHandleSize(texHandleCenter_world);
		// if(handleSize < .3f) handleSize = .3f;

		Handles.matrix = selectedFaceMatrix;

		t_texScale = texScale;
		// why this is the only handle that requires GetHandleSize() is beyond me...
		texScale = Handles.ScaleHandle(texScale, Vector3.zero, Quaternion.identity, handleSize);
		
		Handles.matrix = pm;
		/////////

		bool previouslyMoving_tex = movingPictures;
		if(t_texScale != texScale)
		{
			movingPictures = true;
			if(previouslyMoving_tex == false) {
				
				OnBeginTextureModification();
 
				#if !UNITY_4_3
				Undo.SetSnapshotTarget(selection as Object[], "Scale UVs");
				Undo.CreateSnapshot();
				Undo.RegisterSnapshot();
				#endif
			}

			Vector2 delta = new Vector2( 
					-(t_texScale.x - texScale.x), 
					-(t_texScale.y - texScale.y));

			#if UNITY_4_3
			Undo.RecordObjects(selection, "Scale Texture UVs");
			#endif
			pb_Texture_Editor.TranslateScale(delta* scaleModifier, selection);
		}
	}
	#endif
#endregion

#region HANDLE DRAWING

	public void DrawHandles ()
	{
		Handles.lighting = false;

		switch(selectionMode)
		{

			case SelectMode.Face:
				pb_Editor_Graphics.DrawSelectionMesh();
				break;

			case SelectMode.Vertex:
			{		
				if(selection.Length > 0)
				{
					pb_Editor_Graphics.DrawVertexHandles(selectionLength, selected_uniqueIndices_all, selected_verticesInWorldSpace_all, defaultVertexColor);
					pb_Editor_Graphics.DrawVertexHandles(selectionLength, selected_uniqueIndices_sel, selected_verticesInWorldSpace_all, selectedVertexColor);
				}
				// 	pb_Editor_Graphics.DrawSelectionMesh();
			}
			break;
			
			case SelectMode.Edge:

				#if PROFILE_TIMES
				profiler.LogStart("Draw Edges");
				#endif
				Handles.color = Color.blue;

				for(int i = 0; i < selected_uniqueEdges_all.Length; i++)
				{
					pb_Object pb = selection[i];
					for(int e = 0; e < selected_uniqueEdges_all[i].Length; e++)
					{
						pb_Edge edge = selected_uniqueEdges_all[i][e];
						
						// UndoRedoPerformed isn't called fast enough
						if(edge.x >= pb.sharedIndices.Length || edge.y >= pb.sharedIndices.Length)
							break;

						Handles.DrawLine(selected_verticesInWorldSpace_all[i][pb.sharedIndices[edge.x][0]], selected_verticesInWorldSpace_all[i][pb.sharedIndices[edge.y][0]]);
					}
				}
				Handles.color = Color.green;

				for(int i = 0; i < selectionLength; i++)
				{
					for(int j = 0; j < selection[i].SelectedEdges.Length; j++)
					{
						pb_Object pb = selection[i];
						Vector3[] v = selected_verticesInWorldSpace_all[i];
						
						// TODO - figure out how to run UpdateSelection prior to an Undo event.
						// Currently UndoRedoPerformed is called after the action has taken place.
						if( v.Length < pb.SelectedEdges[j].x || v.Length < pb.SelectedEdges[j].y)
							continue;
	
						Handles.DrawLine(v[pb.SelectedEdges[j].x], v[pb.SelectedEdges[j].y]);
					}
				}

				if(nearestEdgeObjectIndex > -1 && nearestEdgeIndex > -1)
				{
					Handles.color = Color.red;
					Handles.DrawLine(
						selected_verticesInWorldSpace_all[nearestEdgeObjectIndex][nearestEdge.x],
						selected_verticesInWorldSpace_all[nearestEdgeObjectIndex][nearestEdge.y]);
				}
				Handles.color = Color.white;
				
				#if PROFILE_TIMES
				profiler.LogFinish("Draw Edges");
				#endif
				break;
		}

		Handles.lighting = true;
	}

	Color handleBgColor;
	public void DrawHandleGUI()
	{
		Handles.BeginGUI();

		handleBgColor = GUI.backgroundColor;

		#if SVN_EXISTS
		// SVN
		GUI.Label(new Rect(4, 4, 200, 40), "r" + revisionNo);
		#endif

		if(movingVertices)
		{
			GUI.backgroundColor = VertexTranslationInfoBackgroundColor;
			// Handles.Label(newPosition,
			// 	"Translate: " + (newPosition-translateOrigin).ToString() + 
			// 	"\nRotate: " + (currentHandleRotation.eulerAngles-rotateOrigin).ToString() +
			// 	"\nScale: " + (currentHandleScale-scaleOrigin).ToString()
			// 	, VertexTranslationInfoStyle);
			GUI.Label(new Rect(Screen.width-200, Screen.height-120, 162, 48), 
				"Translate: " + (newPosition-translateOrigin).ToString() + 
				"\nRotate: " + (currentHandleRotation.eulerAngles-rotateOrigin).ToString() +
				"\nScale: " + (currentHandleScale-scaleOrigin).ToString()
				, VertexTranslationInfoStyle
				);
		}

		#if DEBUG
		int startY = 0;
		GUI.Label(new Rect(18, startY += 20, 200, 40), "Faces: " + faceCount);
		GUI.Label(new Rect(18, startY += 20, 200, 40), "Vertices: " + vertexCount);
		GUI.Label(new Rect(18, startY += 20, 200, 40), "Triangles: " + triangleCount);
		startY += 20;
		GUI.Label(new Rect(18, startY += 20, 200, 40), "Selected Faces: " + selectedFaceCount);
		GUI.Label(new Rect(18, startY += 20, 200, 40), "Selected Vertices: " + selectedVertexCount);
		#endif

		#if !PROTOTYPE
			GUI.BeginGroup(new Rect(Screen.width/2f-EDITLEVEL_TOOLBAR_WIDTH/2f, EDITLEVEL_TOOLBAR_START_Y, EDITLEVEL_TOOLBAR_WIDTH, 48));
				GUILayout.BeginHorizontal();
					GUI.backgroundColor = (editLevel == EditLevel.Top) ? SCENE_TOOLBAR_ACTIVE_COLOR : Color.white;
					if(GUILayout.Button("Top", 
						EditorStyles.miniButtonLeft, 
						GUILayout.MaxWidth(EDITLEVEL_TOOLBAR_WIDTH/3-2)))
					{
						pb_Editor_Utility.ShowNotification("Top Level Editing");
						SetEditLevel(EditLevel.Top);
					}
					
					GUI.backgroundColor = (editLevel == EditLevel.Geometry) ? SCENE_TOOLBAR_ACTIVE_COLOR : Color.white;
					if(GUILayout.Button("Geometry", 
						EditorStyles.miniButtonMid, 
						GUILayout.MaxWidth(EDITLEVEL_TOOLBAR_WIDTH/3-2)))
					{
						pb_Editor_Utility.ShowNotification("Geometry Editing");
						SetEditLevel(EditLevel.Geometry);
					}
					
					GUI.backgroundColor = (editLevel == EditLevel.Texture) ? SCENE_TOOLBAR_ACTIVE_COLOR : Color.white;
					if(GUILayout.Button("Texture", 
						EditorStyles.miniButtonRight, 
						GUILayout.MaxWidth(EDITLEVEL_TOOLBAR_WIDTH/3-2)))
					{
						pb_Editor_Utility.ShowNotification("Texture Editing");
						SetEditLevel(EditLevel.Texture);
					}

				GUILayout.EndHorizontal();
			GUI.EndGroup();
		#else
			GUI.BeginGroup(new Rect(Screen.width/2f-EDITLEVEL_TOOLBAR_WIDTH/2f, EDITLEVEL_TOOLBAR_START_Y, EDITLEVEL_TOOLBAR_WIDTH, 48));
				GUILayout.BeginHorizontal();
					GUI.backgroundColor = (editLevel == EditLevel.Top) ? SCENE_TOOLBAR_ACTIVE_COLOR : Color.white;
					if(GUILayout.Button("Top", 
						EditorStyles.miniButtonLeft, 
						GUILayout.MaxWidth(EDITLEVEL_TOOLBAR_WIDTH/2-2)))
					{
						pb_Editor_Utility.ShowNotification("Top Level Editing");
						SetEditLevel(EditLevel.Top);
					}
					
					GUI.backgroundColor = (editLevel == EditLevel.Geometry) ? SCENE_TOOLBAR_ACTIVE_COLOR : Color.white;
					if(GUILayout.Button("Geometry", 
						EditorStyles.miniButtonRight, 
						GUILayout.MaxWidth(EDITLEVEL_TOOLBAR_WIDTH/2-2)))
					{
						pb_Editor_Utility.ShowNotification("Geometry Editing");
						SetEditLevel(EditLevel.Geometry);
					}

				GUILayout.EndHorizontal();
			GUI.EndGroup();
		#endif

		// Enables vertex selection with a mouse click
		if(editLevel == EditLevel.Geometry && !dragging && selectionMode == SelectMode.Vertex)
			mouseRect = new Rect(Event.current.mousePosition.x-10, Event.current.mousePosition.y-10, 20, 20);
		else
			mouseRect = pb_Constant.RectZero;

		// Draw selection rect if dragging

		if(dragging)
		{
			GUI.backgroundColor = dragRectColor;
			// Always draw from lowest to largest values
			Vector2 start = Vector2.Min(mousePosition_initial, mousePosition);
			Vector2 end = Vector2.Max(mousePosition_initial, mousePosition);

			selectionRect = new Rect(start.x, start.y, 
				end.x - start.x, end.y - start.y);

			GUI.Box(selectionRect, "");			

			HandleUtility.Repaint();
		}

		GUI.backgroundColor = handleBgColor;

		Handles.EndGUI();
	}
#endregion

#region SHORTCUT

	private bool ShortcutCheck()
	{
		int shortcut = pb_Shortcut.IndexOf(shortcuts, Event.current.keyCode, Event.current.modifiers);

		if( shortcut < 0 )
			return false;

		bool used = true;

		#if PROTOTYPE
		if(shortcuts[shortcut].action == "Texture Mode")
			return false;
		#endif

		used = AllLevelShortcuts(shortcuts[shortcut]);		

		if(!used)
		switch(editLevel)
		{
			case EditLevel.Top:
				used = TopLevelShortcuts(shortcuts[shortcut]);
				break;

			case EditLevel.Texture:
				goto case EditLevel.Geometry;

			case EditLevel.Geometry:
				used = GeoLevelShortcuts(shortcuts[shortcut]);
				break;

			default:
				used = false;
				break;
		}

		if(used)
		{
			if(	shortcuts[shortcut].action != "Delete Face" &&
				shortcuts[shortcut].action != "Quick Apply Nodraw" &&
				shortcuts[shortcut].action != "Toggle Geometry Mode" &&
				shortcuts[shortcut].action != "Toggle Handle Pivot" &&
				shortcuts[shortcut].action != "Toggle Selection Mode" )
				pb_Editor_Utility.ShowNotification(shortcuts[shortcut].action, shortcuts[shortcut].description);
	
			Event.current.Use();
		}

		shortcut = -1;

		return used;
	}

	private bool AllLevelShortcuts(pb_Shortcut shortcut)
	{
		bool used = true;
		switch(shortcut.action)
		{
			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Geometry Mode":

				if(editLevel == EditLevel.Geometry)
				{
					pb_Editor_Utility.ShowNotification("Top Level Editing");
					SetEditLevel(EditLevel.Top);
				}
				else
				{
					pb_Editor_Utility.ShowNotification("Geometry Editing");
					SetEditLevel(EditLevel.Geometry);
				}
				break;

				#if !PROTOTYPE
				case "Texture Mode":
					if(editLevel != EditLevel.Texture)
					{
						pb_Editor_Utility.ShowNotification("Texture Editing");
						SetEditLevel(EditLevel.Texture);
					}
					else
					{
						pb_Editor_Utility.ShowNotification("Top Level Editing");
						SetEditLevel(EditLevel.Top);						
					}

					break;
				#endif

			default:
				used = false;
				break;
		}

		return used;
	}

	private bool TopLevelShortcuts(pb_Shortcut shortcut)
	{
		if(selection == null || selection.Length < 1)
			return false;

		bool used = true;
		switch(shortcut.action)
		{
			/* ENTITY TYPES */
			case "Set Trigger":
				if(editLevel == EditLevel.Top)
				foreach(pb_Object pb in selection)
					pb_Editor_Utility.SetEntityType(EntityType.Trigger, pb.gameObject);
				break;

			#if !PROTOTYPE
			case "Set Occluder":
				if(editLevel == EditLevel.Top)
				foreach(pb_Object pb in selection)
					pb_Editor_Utility.SetEntityType(EntityType.Occluder, pb.gameObject);
				break;
			#endif

			case "Set Collider":
				if(editLevel == EditLevel.Top)
				foreach(pb_Object pb in selection)
					pb_Editor_Utility.SetEntityType(EntityType.Collider, pb.gameObject);
				break;

			case "Set Mover":
				if(editLevel == EditLevel.Top)
				foreach(pb_Object pb in selection)
					pb_Editor_Utility.SetEntityType(EntityType.Mover, pb.gameObject);
				break;
				
			case "Set Detail":
				if(editLevel == EditLevel.Top)
				foreach(pb_Object pb in selection)
					pb_Editor_Utility.SetEntityType(EntityType.Detail, pb.gameObject);
				break;

			default:	
				used = false;
				break;
		}

		return used;
	}

	private bool GeoLevelShortcuts(pb_Shortcut shortcut)
	{
		bool used = true;
		switch(shortcut.action)
		{
			case "Escape":
				ClearFaceSelection();
				UpdateSelection();
				SetEditLevel(EditLevel.Top);
			break;
		
			// TODO Remove once a workaround for non-upper-case shortcut chars is found			
			case "Toggle Selection Mode":
				ToggleSelectionMode();
				switch(selectionMode)
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
				break;

			#if !PROTOTYPE
			case "Quick Apply Nodraw":
		
				if(editLevel != EditLevel.Top)
					pb_Editor_Utility.ShowNotification(shortcut.action, shortcut.description);

				pb_Texture_Editor.ApplyNoDraw(selection, show_NoDraw);
				ClearFaceSelection();
				break;
			#endif

			#if !PROTOTYPE
			case "Delete Face":
				pbUndo.RecordObjects(selection, "Delete selected faces.");

				int sel_faces = 0;
				foreach(pb_Object pb in selection)
				{
					sel_faces += pb.SelectedFaces.Length;

					pb.DeleteFaces(pb.SelectedFaces);

					if(pb.faces.Length < 1)
					{
						pbUndo.DestroyImmediate(pb, "Delete Object");
					}
					else
					{
						pb.GenerateUV2(show_NoDraw);
						pb.Refresh();
					}
				}

				if(sel_faces > 0)
					pb_Editor_Utility.ShowNotification(shortcut.action, shortcut.description);

				ClearFaceSelection();

				UpdateSelection();

				OnGeometryChanged(selection);

				break;
			#endif

			/* handle alignment */
			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Handle Pivot":
				if(selectedVertexCount < 1)
					break;
				
				ToggleHandleAlignment();

				pb_Editor_Utility.ShowNotification("Handle Alignment: " + ((HandleAlignment)handleAlignment).ToString());
				break;

			case "Set Pivot":

		        if (selection.Length > 0)
		        {
					foreach (pb_Object pbo in selection)
					{
						pbUndo.RecordObjects(new Object[2] {pbo, pbo.transform}, "Set Pivot");

						if (pbo.SelectedTriangles.Length > 0)
						{
							pbo.CenterPivot(pbo.SelectedTriangles);
						}
						else
						{
							pbo.CenterPivot(null);
						}
					}
				}
				break;

			default:
				used = false;
				break;
		}
		return used;
	}
#endregion

#region VIS GROUPS

	public bool show_Detail;
	public bool show_Occluder;
	public bool show_Mover;
	public static bool show_NoDraw;
	public bool show_Collider;
	public bool show_Trigger;

	public void ToggleEntityVisibility(EntityType entityType, bool isVisible)
	{
		foreach(pb_Entity sel in Object.FindObjectsOfType(typeof(pb_Entity)))
		{
			if(sel.entityType == entityType) {
				sel.GetComponent<MeshRenderer>().enabled = isVisible;
				if(sel.GetComponent<MeshCollider>())
					sel.GetComponent<MeshCollider>().enabled = isVisible;
			}
		}		
	}
	
	public void ToggleNoDrawVisibility(bool show)
	{
		#if !PROTOTYPE
		if(show_NoDraw)
			ndgraphic = nodraw_OnGraphic;
		else
			ndgraphic = nodraw_OffGraphic;

		foreach(pb_Object pb in GameObject.FindObjectsOfType(typeof(pb_Object)))
			pb.ToMesh(!show_NoDraw);// param is hideNoDraw
		#endif
	}
#endregion

#region TOOL SETTINGS

	public void SetTool(Tool newTool)
	{
		// world rotation doesn't really work well, nor is it particularly useful
		if(handleAlignment == HandleAlignment.World) handleAlignment = HandleAlignment.Local;

		currentHandle = newTool;
	}

	public void SetHandleAlignment(HandleAlignment ha)
	{
		if(editLevel == EditLevel.Texture)
			ha = HandleAlignment.Plane;

		#if FREE || TORNADO_TWINS
			handleAlignment = HandleAlignment.World;
		#endif
		
		if(currentHandle == Tool.Rotate && ha == HandleAlignment.World)
			ha = HandleAlignment.Plane;

		handleAlignment = ha;
		EditorPrefs.SetInt(pb_Constant.pbHandleAlignment, (int)handleAlignment);

		UpdateHandleRotation();

		currentHandleRotation = handleRotation;

		SceneView.RepaintAll();
	}

	public void ToggleHandleAlignment()
	{
		int newHa = (int)handleAlignment+1;
		if( newHa >= System.Enum.GetValues(typeof(HandleAlignment)).Length)
			newHa = 0;
		SetHandleAlignment((HandleAlignment)newHa);
	}

	public void ToggleEditLevel()
	{
		if(editLevel == EditLevel.Geometry)
			SetEditLevel(EditLevel.Top);
		else
			SetEditLevel(EditLevel.Geometry);
	}

	/**
	 * Toggles between the SelectMode values and updates the graphic handles
	 * as necessary.
	 */
	public void ToggleSelectionMode()
	{
		int smode = (int)selectionMode;
		smode++;
		if(smode >= SELECT_MODE_LENGTH)
			smode = 0;
		SetSelectionMode( (SelectMode)smode );
	}

	/**
	 * \brief Sets the current selection mode @SelectMode to the mode value.
	 */
	public void SetSelectionMode(SelectMode mode)
	{
		// If texture window is open, force the mode to Face
		if(editLevel == EditLevel.Texture && mode != SelectMode.Face)
			selectionMode = SelectMode.Face;
		else
			selectionMode = mode;

		#if UNITY_4
			pb_Editor_Graphics.UpdateSelectionMesh(selection, selectionMode);
		#else
		if(selectionMode != SelectMode.Edge)
			pb_Editor_Graphics.UpdateSelectionMesh(selection, selectionMode);
		else
		{
			pb_Editor_Graphics.ClearSelectionMesh();
			UpdateSelection();
		}
		#endif

		UpdateModeGraphic();

		SceneView.RepaintAll();

		EditorPrefs.SetInt(pb_Constant.pbDefaultSelectionMode, (int)selectionMode);
	}

	public SelectMode GetSelectionMode()
	{
		return selectionMode;
	}

	public void SetEditLevel(EditLevel el)
	{
		SetEditLevel(el, true);
	}

	public void SetEditLevel(EditLevel el, bool attemptCloseTextureWindow)
	{	
		#if !PROTOTYPE
		if(attemptCloseTextureWindow && el != EditLevel.Texture && pb_Texture_Editor.instanceIfExists != null)
			pb_Texture_Editor.instanceIfExists.Close();
		#endif

		switch(el)
		{
			case EditLevel.Top:				
				modeGraphic = select_Graphic;

				Exit(Selection.gameObjects);
				break;

			case EditLevel.Geometry:
				
				Tools.current = Tool.None;

				switch(selectionMode)
				{
					case SelectMode.Vertex:
						modeGraphic = vertex_Graphic;
						break;
					
					case SelectMode.Edge:	
						modeGraphic = edge_Graphic;
						break;

					default:
						modeGraphic = face_Graphic;
						break;
				}
				
				UpdateSelection();
				SceneView.RepaintAll();
				break;

			#if !PROTOTYPE
			case EditLevel.Texture:
				
				SetHandleAlignment(HandleAlignment.Plane);

				OpenTextureWindow();
				modeGraphic = texture_Graphic;
				break;
			#endif
		}

		editLevel = el;

		EditorPrefs.SetInt(pb_Constant.pbDefaultEditLevel, (int)editLevel);
	}
#endregion

#region SELECTION CACHING AND MANAGING
	
	/** 
	 *	\brief Updates the arrays used to draw GUI elements (both Window and Scene).
	 *	@selection_vertex should already be populated at this point.  UpdateSelection 
	 *	just removes duplicate indices, and populates the gui arrays for displaying
	 *	 things like quad faces and vertex billboards.
	 */

	int 				selectionLength = 0;
	int[][] 			selected_uniqueIndices_all = new int[0][];
	int[][] 			selected_uniqueIndices_sel = new int[0][];
	Vector3[][] 		selected_verticesInWorldSpace_all = new Vector3[0][];
	Vector3[][] 		selected_verticesLocal_sel = new Vector3[0][];
	pb_Edge[][] 		selected_uniqueEdges_all = new pb_Edge[0][];
	
	public pb_Face[][] 	SelectedFacesInEditZone { get; private set; }//new pb_Face[0][];		// faces that need to be refreshed when moving or modifying the actual selection

	Vector3				selected_handlePivotWorld = Vector3.zero;
	Vector3[]			selected_handlePivot = new Vector3[0];

	#if DEBUG
	int faceCount = 0;
	int vertexCount = 0;
	int selectedFaceCount = 0;
	int triangleCount = 0;
	#endif

	#if PROFILE_TIMES
	pb_Profiler updateSelectionProfiler = new pb_Profiler();
	#endif
	public void UpdateSelection()
	{
		#if PROFILE_TIMES
		profiler.LogStart("UpdateSelection");
		#endif

		selectedVertexCount = 0;
		#if DEBUG
		selectedFaceCount = 0;
		faceCount = 0;
		vertexCount = 0;
		triangleCount = 0;
		#endif

		selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);
		selectionLength = selection.Length;
		transformCache = new Vector3[selection.Length][];

		selected_uniqueIndices_all			= new int[selectionLength][];
		selected_uniqueIndices_sel			= new int[selectionLength][];
		selected_verticesInWorldSpace_all 	= new Vector3[selectionLength][];
		selected_uniqueEdges_all			= new pb_Edge[selectionLength][];
		selected_verticesLocal_sel			= new Vector3[selectionLength][];
		SelectedFacesInEditZone 			= new pb_Face[selectionLength][];
		selected_handlePivot 				= new Vector3[selectionLength];
		
		selected_handlePivotWorld			= Vector3.zero;
		
		for(int i = 0; i < selectionLength; i++)
		{			
			pb_Object pb = selection[i];

			transformCache[i] = new Vector3[3]
			{
				pb.transform.position,
				pb.transform.localRotation.eulerAngles,
				pb.transform.localScale
			};

			// things necessary to call every frame
			selected_uniqueIndices_all[i] = pb.uniqueIndices;
			selected_uniqueIndices_sel[i] = pb.SelectedTriangles;
			
			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("selected_verticesInWorldSpace_all");
			#endif
			selected_verticesInWorldSpace_all[i] = pb.VerticesInWorldSpace();	// to speed this up, could just get uniqueIndices vertiecs
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("selected_verticesInWorldSpace_all");			
			#endif

			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("selected_verticesLocal_sel");
			#endif
			selected_verticesLocal_sel[i] = pb.GetVertices(pb.SelectedTriangles);
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("selected_verticesLocal_sel");
			#endif


			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("selected_handlePivot");
			#endif
			selected_handlePivot[i] = pb_Math.Average(selected_verticesLocal_sel[i]);
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("selected_handlePivot");
			#endif

			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("selected_handlePivotWorld");
			#endif
			selected_handlePivotWorld += pb_Math.Average(pbUtil.ValuesWithIndices(selected_verticesInWorldSpace_all[i], pb.SelectedTriangles));
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("selected_handlePivotWorld");
			#endif

			// necessary only once on selection modification
			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("selected_uniqueEdges_all");
			#endif
			selected_uniqueEdges_all[i] = pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), pb.sharedIndices).ToArray();
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("selected_uniqueEdges_all");
			#endif
			
			#if PROFILE_TIMES
			updateSelectionProfiler.LogStart("SelectedFacesInEditZone");
			#endif
			SelectedFacesInEditZone[i] = pbMeshUtils.GetConnectedFaces(pb, pb.SelectedTriangles);
			#if PROFILE_TIMES
			updateSelectionProfiler.LogFinish("SelectedFacesInEditZone");
			#endif

			selectedVertexCount += selection[i].SelectedTriangles.Length;

			#if DEBUG
			selectedFaceCount += selection[i].SelectedFaces.Length;
			faceCount += selection[i].faces.Length;
			vertexCount += selection[i].vertexCount;
			triangleCount += selection[i].msh.triangles.Length;
			#endif
		}

		selected_handlePivotWorld /= (float)selectionLength;

		#if !PROTOTYPE
		UpdateTextureHandles();
		#endif
		
		UpdateGraphics();
		UpdateModeGraphic();

		UpdateHandleRotation();
		currentHandleRotation = handleRotation;

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);

		#if PROFILE_TIMES
		profiler.LogFinish("UpdateSelection");
		#endif
	}

	// Only updates things that absolutely need to be refreshed, and assumes that no selection changes have occured
	private void Internal_UpdateSelectionFast()
	{
		selected_handlePivotWorld = Vector3.zero;
		for(int i = 0; i < selectionLength; i++)
		{
			pb_Object pb = selection[i];
			
			transformCache[i] = new Vector3[3]
			{
				pb.transform.position,
				pb.transform.localRotation.eulerAngles,
				pb.transform.localScale
			};
			
			selected_verticesInWorldSpace_all[i] = pb.VerticesInWorldSpace();	// to speed this up, could just get uniqueIndices vertiecs
			selected_verticesLocal_sel[i] = pb.GetVertices(pb.SelectedTriangles);
			selected_handlePivot[i] = pb_Math.Average(selected_verticesLocal_sel[i]);
			selected_handlePivotWorld += pb_Math.Average(pbUtil.ValuesWithIndices(selected_verticesInWorldSpace_all[i], pb.SelectedTriangles));
		}
		selected_handlePivotWorld /= (float)selectionLength;
		
		UpdateGraphics();
		UpdateHandleRotation();
		currentHandleRotation = handleRotation;

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);
	}

	private void UpdateGraphics()
	{
		#if UNITY_4
			pb_Editor_Graphics.UpdateSelectionMesh(selection, selectionMode);
		#else
		if(selectionMode != SelectMode.Edge)
			pb_Editor_Graphics.UpdateSelectionMesh(selection, selectionMode);
		else
			pb_Editor_Graphics.ClearSelectionMesh();
		#endif
	}

	public void AddToSelection(GameObject t)
	{
		Object[] temp = new Object[Selection.objects.Length + 1];
		temp[0] = t;
		for(int i = 1; i < temp.Length; i++)
			temp[i] = Selection.objects[i-1];
		Selection.objects = temp;
	}

	public void RemoveFromSelection(GameObject t)
	{
		int ind = System.Array.IndexOf(Selection.objects, t);
		if(ind < 0)
			return;

		Object[] temp = new Object[Selection.objects.Length - 1];

		for(int i = 1; i < temp.Length; i++) {
			if(i != ind)
				temp[i] = Selection.objects[i];
		}

		Selection.objects = temp;
	}

	public void Exit()
	{
		pbUndo.RecordObjects(selection, "Change Selection");

		ClearSelection();

		// if the previous tool was set to none, use Tool.Move
		if(Tools.current == Tool.None)
			Tools.current = Tool.Move;

		Selection.activeTransform = null;

	}

	public void Exit(GameObject newSelection)
	{
		pbUndo.RecordObjects(selection, "Change Selection");

		ClearSelection();

		// if the previous tool was set to none, use Tool.Move
		if(Tools.current == Tool.None)
			Tools.current = Tool.Move;

		if(newSelection)
			Selection.activeTransform = newSelection.transform;
		else
			Selection.activeTransform = null;
	}

	public void Exit(GameObject[] newSelection)
	{
		pbUndo.RecordObjects(selection, "Change Selection");
		
		ClearSelection();

		// if the previous tool was set to none, use Tool.Move
		if(Tools.current == Tool.None)
			Tools.current = Tool.Move;

		if(newSelection != null && newSelection.Length > 0) {
			Selection.activeTransform = newSelection[0].transform;
			Selection.objects = newSelection;
		}
		else
			Selection.activeTransform = null;
	}

	public void SetSelection(GameObject go)
	{
		pbUndo.RecordObjects(selection, "Change Selection");
		
		ClearSelection();
		AddToSelection(go);
	}

	/**
	 *	Clears all `selected` caches associated with each pb_Object in the current selection.  The means triangles, faces, and edges.
	 */
	public void ClearFaceSelection()
	{
		foreach(pb_Object pb in selection) {
			pb.ClearSelection();
		}

		nearestEdge = null;
		nearestEdgeObjectIndex = -1;
		nearestEdgeIndex = -1;
		
		pb_Editor_Graphics.ClearSelectionMesh();
	}

	public void ClearSelection()
	{
		foreach(pb_Object pb in selection) {
			pb.ClearSelection();
		}

		Selection.objects = new Object[0];

		pb_Editor_Graphics.ClearSelectionMesh();
	}
#endregion

#region HANDLE AND GUI CALCULTATIONS

	#if !PROTOTYPE
	Matrix4x4 selectedFaceMatrix = Matrix4x4.identity;
	private void UpdateTextureHandles()
	{
		texHandleCenter = selectionLength < 1 ? Vector3.up : selected_handlePivot[0];
		texHandleCenter_world = selected_handlePivotWorld;
		
		// Move Handle + General Info Seeking
		movingPictures = false;

		texPos = Vector3.zero;

		// Rotation Handle
		texRotation = Quaternion.identity;

		// Scale Handle
		texScale = Vector3.one;

		// Cache the local matrix trasnforms required to make the current face in local spcae...
		/// or something like that.  Is used to operate Texture Tools in local space.
		if(selection.Length < 1)
			return;

		Vector3 nrm = selected_verticesLocal_sel[0].Length > 2 ? pb_Math.Normal(selected_verticesLocal_sel[0]) : Vector3.forward;
 		Vector3 t = selection[0].transform.position;
 		Quaternion r = selection[0].transform.localRotation;
 		selectedFaceMatrix = Matrix4x4.TRS(t, r, Vector3.one);

		Quaternion faceRotation = Quaternion.LookRotation(nrm, Vector3.up );
        selectedFaceMatrix *= Matrix4x4.TRS(texHandleCenter, faceRotation, Vector3.one);
	}
	#endif


	Vector3 selectedNormal_local = Vector3.up;
	Quaternion handleRotation = new Quaternion(0f, 0f, 0f, 1f);
	public void UpdateHandleRotation()
	{
		selectedNormal_local = selectionLength > 0 ? selection[0].transform.forward : Vector3.up;
		foreach(Vector3[] v in selected_verticesLocal_sel)
			if(v.Length > 2)
			{
				selectedNormal_local = pb_Math.Normal(v);
				break;			
			}

		// Unity freaks out if SetLookRotation() is Vector3.zero, throwing Debug Logs like crazy - 
		// which in turn slows the editor to a crawl.  This catches that and prevents a zero'd 
		// Vector3 from sneaking through.
		if(selectedNormal_local == Vector3.zero) {
			selectedNormal_local = Vector3.up;
		}

		Quaternion localRot = Selection.activeTransform == null ? Quaternion.identity : Selection.activeTransform.localRotation;

		switch(handleAlignment)
		{
			case HandleAlignment.Plane:
				// apply local rotation, then apply rotation derived from normal unit vector
				if(selectedVertexCount < 3) goto case HandleAlignment.Local;
				handleRotation = localRot * Quaternion.LookRotation( selectedNormal_local, Vector3.up );
				hgraphic = plane_Graphic;
				break;
			case HandleAlignment.Local:
				handleRotation = HANDLE_ROTATION * localRot;
				hgraphic = local_Graphic;
				break;
			case HandleAlignment.World:
				handleRotation = HANDLE_ROTATION;
				hgraphic = global_Graphic;
				break;
		}
	}

	private void UpdateModeGraphic()
	{
		// mode based
		if(editLevel == EditLevel.Geometry)
		{
			switch(selectionMode)
			{
				case SelectMode.Vertex:
					modeGraphic = vertex_Graphic;
					break;
				
				case SelectMode.Edge:	
					modeGraphic = edge_Graphic;
					break;

				default:
					modeGraphic = face_Graphic;
					break;
			}
		}
	}
#endregion

#region Selection Management and checks

	private void VerifyTextureGroupSelection()
	{
		foreach(pb_Object pb in selection)
		{
			List<int> alreadyChecked = new List<int>();

			foreach(pb_Face f in pb.SelectedFaces)	
			{
				int tg = f.textureGroup;
				if(tg > 0 && !alreadyChecked.Contains(f.textureGroup))
				{
					foreach(pb_Face j in pb.faces)
						if(j != f && j.textureGroup == tg && !pb.SelectedFaces.Contains(j))
						{
							// int i = EditorUtility.DisplayDialogComplex("Mixed Texture Group Selection", "One or more of the faces selected belong to a Texture Group that does not have all it's member faces selected.  To modify, please either add the remaining group faces to the selection, or remove the current face from this smoothing group.", "Add Group to Selection", "Cancel", "Remove From Group");
							int i = 0;
							switch(i)
							{
								case 0:
									List<pb_Face> newFaceSection = new List<pb_Face>();
									foreach(pb_Face jf in pb.faces)
										if(jf.textureGroup == tg)
											newFaceSection.Add(jf);
									pb.SetSelectedFaces(newFaceSection.ToArray());
									UpdateSelection();
									break;

								case 1:
									break;

								case 2:
									f.textureGroup = 0;
									break;
							}
							break;
						}
				}
				alreadyChecked.Add(f.textureGroup);
			}
		}
	}
#endregion

#region EVENTS AND LISTENERS

	/// @todo -- use hasChanged flag (someday)
	Vector3[][] transformCache = new Vector3[0][];

	private void ListenForTopLevelMovement()
	{
		if(selectedVertexCount > 1 || GUIUtility.hotControl < 1)
			return;

		bool movementDetected = false;
		for(int i = 0; i < selection.Length; i++)
		{
			if(selection[i] == null)
				continue;
			if(	selection[i].transform.position != transformCache[i][0] ||
				selection[i].transform.localRotation.eulerAngles != transformCache[i][1] ||
				selection[i].transform.localScale != transformCache[i][2])
			{
				movementDetected = true;
				break;
			}
		}

		if(!movementDetected)
			return;

		UpdateSelection();
	}

	public void OnHierarchyChange()
	{
		// selection = new pb_Object[0];
		if(!EditorApplication.isPlaying && !movingVertices)
		{
			// don't delete, dummy!
			foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))//pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				/**
				 * If it's a prefab instance, reconstruct.
				 */
				if(PrefabUtility.GetPrefabType(pb.gameObject) == PrefabType.PrefabInstance)
				{
					PropertyModification[] mods = PrefabUtility.GetPropertyModifications(pb.gameObject);

					if(System.Array.Exists(mods, x => x.target is MeshFilter))
						pb.ReconstructMesh();
					else
						pb.Verify();
				}
				// else
				// {
				// }
			}
		}

		UpdateSelection();
		SceneView.RepaintAll();
	}

	public void OnSelectionChange()
	{
		nearestEdge = null;
		nearestEdgeIndex = -1;
		nearestEdgeObjectIndex = -1;
		
		if(Selection.objects.Contains(pb_Editor_Graphics.selectionGameObject)) {
			// Debug.LogWarning("TELL KARL THIS WARNING WAS THROWN");
			RemoveFromSelection(pb_Editor_Graphics.selectionGameObject);
		}

		// InitSelectionGameObject();

		UpdateSelection();
	}

	public void OnPlayModeStateChanged()
	{		
		if(EditorApplication.isPlaying)
			OnEnterPlaymode();
		else
			OnExitPlaymode();
	}

	#if !PROTOTYPE
	bool toggleNoDrawOnExit = false; 	 
	#endif
	bool toggleColliderOnExit = false; 
	bool toggleTriggerOnExit = false;  
	public void OnEnterPlaymode()
	{
		// Set all visgroup graphics to off, but don't fuck with the meshes.

		cgraphic 	= collision_OffGraphic;
		tgraphic 	= trigger_OffGraphic;

		#if !PROTOTYPE
		ndgraphic 	= nodraw_OffGraphic;
		if(show_NoDraw)		toggleNoDrawOnExit 	 = true;
		#endif

		if(show_Collider)	toggleColliderOnExit = true;
		if(show_Trigger)	toggleTriggerOnExit  = true;
		
		show_NoDraw 	= false;
		show_Collider 	= false;
		show_Trigger 	= false;
	}

	public void OnExitPlaymode()
	{
		#if !PROTOTYPE
		if(toggleNoDrawOnExit) 		
		{
			ndgraphic = nodraw_OnGraphic;
			show_NoDraw	= true;
		}
		#endif

		if(toggleColliderOnExit)
		{
			cgraphic = collision_OnGraphic; 	
			show_Collider = true;
		}

		if(toggleTriggerOnExit)
		{
			tgraphic = trigger_OnGraphic;
			show_Trigger = true;
		}
	}

	public void SceneWideDuplicateCheck()
	{
		pb_Object[] allPBObjects = FindObjectsOfType(typeof(pb_Object)) as pb_Object[];
		foreach(pb_Object pb in allPBObjects)
			pb.Verify();
	}

	public void OnValidateCommand(string command)
	{
		switch(command)
		{
			case "UndoRedoPerformed":

				pb_Object[] pbos = pbUtil.GetComponents<pb_Object>(Selection.transforms);
		
				foreach(pb_Object pb in pbos)
				{
					pb.Verify();

					// for whatever reason, the cached properties of faces in this array
					// don't get undone when undo-ing.  the pb.faces array is okay though ?
					foreach(pb_Face f in pb.SelectedFaces)
						f.RebuildCaches();

					pb.GenerateUV2(show_NoDraw);
					pb.Refresh();
				}

				UpdateSelection();
				SceneView.RepaintAll();

				break;
		}
	}
	
	private void UndoRedoPerformed()
	{
		pb_Object[] pbos = pbUtil.GetComponents<pb_Object>(Selection.transforms);
		
		foreach(pb_Object pb in pbos)
		{
			pb.Verify();

			// for whatever reason, the cached properties of faces in this array
			// don't get undone when undo-ing.  the pb.faces array is okay though ?
			foreach(pb_Face f in pb.SelectedFaces)
				f.RebuildCaches();

			pb.Refresh();
			pb.GenerateUV2(show_NoDraw);
		}

		UpdateSelection();
		SceneView.RepaintAll();
	}

	/**
	 *	\brief Used to check whether object is nodraw free or not - matching call is in pb_Texture_Editor.SetNoDraw
	 */
	private void OnGeometryChanged( pb_Object[] pbs )
	{
		// check the object flags
		foreach(pb_Object pb in pbs)
		{
			StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags( pb.gameObject );
			
			// if nodraw found
			if(pb.containsNodraw)
			{
				if( (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic )
				{
					flags ^= StaticEditorFlags.BatchingStatic;
					GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
				}
			}
			else
			{
				// if nodraw not found, and entity type should be batching static
				if(pb.GetComponent<pb_Entity>().entityType != EntityType.Mover)
				{
					flags = flags | StaticEditorFlags.BatchingStatic;
					GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
				}
			}
		}
	}

	private void PushToGrid(float snapVal)
	{
		for(int i = 0; i  < selectionLength; i++)
		{
			pb_Object pb = selection[i];

			int[] indices = pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles);

			Vector3[] verts = pb.vertices;
			
			for(int n = 0; n < indices.Length; n++)
				verts[indices[n]] = pb.transform.InverseTransformPoint(pbUtil.SnapValue(pb.transform.TransformPoint(verts[indices[n]]), Vector3.one, snapVal));
				
			// don't bother calling a full ToMesh() here because we know for certain that the _vertices and msh.vertices arrays are equal in length
			pb.SetVertices(verts);
			pb.msh.vertices = verts;

			pb.RefreshUV( SelectedFacesInEditZone[i] );
			pb.RefreshNormals();
		}

		Internal_UpdateSelectionFast();
	}

	/**
	 *	A tool, any tool, has just been engaged
	 */
	public void OnBeginTextureModification()
	{
		VerifyTextureGroupSelection();
	}

	public void OnFinishedVertexModification()
	{
		if(OnVertexMovementFinished != null)
			OnVertexMovementFinished(selection);

		currentHandleScale = Vector3.one;
		currentHandleRotation = handleRotation;

		foreach(pb_Object sel in selection)
		{
			sel.GenerateUV2(show_NoDraw);
			sel.Refresh();
		}
		
		// if(scaling)
		// {
		// 	#if UNITY_4_3
		// 		Undo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Freeze Scale Transforms");
		// 	#else
		// 		Undo.RegisterUndo(pbUtil.GetComponents<pb_Object>(Selection.transforms) as Object[], "Freeze Scale Transforms");
		// 	#endif
		// 	foreach(pb_Object sel in selection)
		// 	{
		// 		if(sel.isSelectable)
		// 			sel.FreezeScaleTransform();
		// 	}
		// }

		movingVertices = false;
		scaling = false;
	}
#endregion

#region WINDOW MANAGEMENT

	public void OpenLightmappingInterface()
	{
		pb_Lightmap_Editor.Init(this);
	}
	
	#if !PROTOTYPE
	void OpenTextureWindow()
	{
		// EditorWindow.GetWindow<pb_UV_Editor>().Show();
		EditorWindow.GetWindow<pb_Texture_Editor>(true, "Texture Window", true).Show();
	}
	#endif

	public void OpenGeometryInterface()
	{
		EditorWindow.GetWindow(typeof(pb_Geometry_Interface), true, "Shape Tool", true);
	}
#endregion

#region DEBUG

	public void DrawVertexNormals()
	{
		foreach(pb_Object pb in selection)
		{
			// selection doesn't update fast enough, so this null check needs to exist
			if(pb == null)
				continue;

			Vector3[] verts = pb.VerticesInWorldSpace();
			for(int i = 0; i < verts.Length; i++)
			{
				float angle = 0f;
				Vector3 vup = verts[i];
				pb.transform.localRotation.ToAngleAxis(out angle, out vup);

				Vector3 v = verts[i];
				Handles.color = Color.green;
					Handles.DrawLine(v, v + Quaternion.AngleAxis(angle, vup) * pb.msh.normals[i]);
				Handles.color = Color.white;
			}
		}
	}

	const float NRML_LENGTH = .3f;

	public void DrawFaceNormals()
	{
		foreach(pb_Object pb in selection)
		{
			// selection doesn't update fast enough, so this null check needs to exist
			if(pb == null)
				continue;

			pb_Face[] faces = pb.SelectedFaces;

			for(int i = 0; i < faces.Length; i++)
			{
				Vector3[] fv = pb.GetVertices(faces[i]);
				Vector3 v = pb.transform.TransformPoint(pb_Math.Average(fv));
				Vector3 nrml = pb.transform.TransformDirection(pb_Math.Normal(fv));

				Handles.color = Color.green;
					Handles.DrawLine(v, v + nrml);
				Handles.color = Color.white;
			}
		}
	}
#endregion

#region CONVENIENCE CALLS
	// Handy calls
	public bool altClick { get { return (Event.current.alt); } }
	public bool leftClick { get { return (Event.current.type == EventType.MouseDown); } }
	public bool leftClickUp { get { return (Event.current.type == EventType.MouseUp); } }
	public bool middleClick { get { return Event.current.isMouse ? Event.current.button > 1 : false; } }
	public bool contextClick { get { return (Event.current.type == EventType.ContextClick); } }
	public bool mouseDrag { get { return (Event.current.type == EventType.MouseDrag); } }
	public bool ignore { get { return Event.current.type == EventType.Ignore; } }
	public Vector2 mousePosition { get { return Event.current.mousePosition; } }
	public Vector2 eventDelta { get { return Event.current.delta; } }
	public bool rightClick { get { return (Event.current.type == EventType.ContextClick); } }
	public bool shiftKey { get { return Event.current.shift; } }
	public bool ctrlKey { get { return Event.current.command || Event.current.control; } }
	public bool earlyOut { get { return (
		altClick || 
		Tools.current == Tool.View || 
		GUIUtility.hotControl > 0 || 
		middleClick ||
		Tools.viewTool == ViewTool.FPS ||
		Tools.viewTool == ViewTool.Orbit); } }
	public KeyCode getKeyUp { get { return Event.current.type == EventType.KeyUp ? Event.current.keyCode : KeyCode.None; } }
#endregion

}