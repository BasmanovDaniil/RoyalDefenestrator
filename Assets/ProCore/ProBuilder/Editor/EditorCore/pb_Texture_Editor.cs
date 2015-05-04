#pragma warning disable 0414

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

#if BUGGER
using Parabox.Bugger;
#endif

/**
 *	Todo: !-- Remove hardset ints that are everywhere and replace with consts
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.EditorEnum;

public class pb_Texture_Editor : EditorWindow {

#if !PROTOTYPE

#region MEMBERS

	public static pb_Texture_Editor instanceIfExists;

	pb_Smoothing_Editor smoothingEditor;
	public pb_UV uv_gui = new pb_UV();		// store GUI changes here, so we may selectively apply them later

	pb_Object[] selection = new pb_Object[0];	
	pb_Face[][] SelectedFacesInEditZone = new pb_Face[0][];

	public List<pb_UV> uv_selection = new List<pb_UV>();
	Dictionary<string, bool> uv_diff = new Dictionary<string, bool>() {
		{"projectionAxis", false},
		{"useWorldSpace", false},
		{"flipU", false},
		{"flipV", false},
		{"swapUV", false},
		{"fill", false},
		{"scalex", false},
		{"scaley", false},
		{"offsetx", false},
		{"offsety", false},
		{"rotation", false},
		{"justify", false}
	};
	Material currentMat;
	Material queuedMat;

	pb_Shortcut[] shortcuts;

	public enum XY {
		XY,
		X,
		Y
	}
#endregion

#region INITIALIZATION / EXIT HANDLING

	public void LoadPrefs()
	{
		shortcuts = pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts));
		
		if(pb_Editor.instanceIfExists)
		{
			pb_Editor.instanceIfExists.drawHandles = (EditorPrefs.HasKey(pb_Constant.pbDefaultHideFaceMask)) ? 
				!EditorPrefs.GetBool(pb_Constant.pbDefaultHideFaceMask) : true;
		}
	}

	public void OnEnable()
	{
		LoadPrefs();

		pb_Editor.OnSelectionUpdate += new pb_Editor.OnSelectionUpdateEventHandler(OnSelectionUpdate);
		
		this.minSize = new Vector2(245, 448);
		this.autoRepaintOnSceneChange = true;

		instanceIfExists = this;

		if(pb_Editor.instanceIfExists)
		{
			OnSelectionUpdate(pb_Editor.instanceIfExists.selection);
			pb_Editor.instanceIfExists.SetSelectionMode(SelectMode.Face);		
		}
	}

	public void OnFocus()
	{
		if(pb_Editor.instanceIfExists)
			pb_Editor.instanceIfExists.SetSelectionMode(SelectMode.Face);
	}

	public void OnDisable()
	{
		if(smoothingEditor)
			smoothingEditor.Close();
		
		if(pb_Editor.instanceIfExists)
			pb_Editor.instanceIfExists.SetEditLevel(EditLevel.Top, false);
	}
#endregion

#region ONGUI

	Rect currentMatRect = new Rect(0,0,0,0);
	Rect currentMatRect_inset = new Rect(0,0,0,0);
	
	Rect queuedMatRect = new Rect(0,0,0,0);
	Rect queuedMatRect_inset = new Rect(0,0,0,0);

	// Rect matFieldRect = new Rect(0,0,0,0);
	Rect applyButtonRect = new Rect(0,0,0,0);
	int previewMatSize = 94;
	int inset = 4;
	int pad = 8;

	int prevWidth, prevHeight;
	
	Vector2 scrollPos;

	int oneThird = 1;
	int oneFourth = 1;

	void OnGUI()
	{
		if(Screen.width != prevWidth || Screen.height != prevHeight && Event.current.type != EventType.Repaint)
			OnWindowResize();
		
		if(undoRedoPerformed) { RepaintSceneViews(); }

		// QUEUED MAT
		DrawMatPreview(queuedMat, queuedMatRect, queuedMatRect_inset, "Queued Material");

		if(GUI.Button(applyButtonRect, "Apply", EditorStyles.miniButton))
			ApplyMaterial(selection, queuedMat);
		
		GUILayout.BeginVertical();
		#if FREE
		GUI.enabled = false;
			EditorGUILayout.ObjectField(queuedMat, typeof(Material), false);
		GUI.enabled = true;
		#else
		queuedMat = (Material)EditorGUILayout.ObjectField(queuedMat, typeof(Material), false);
		#endif

		GUILayout.Space(Screen.width+2);
	
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		// Set initial changed to false (in case Apply was buttoned... is that the verb?)
		// Nope, pressed.
		GUI.changed = false;

		#if FREE || TORNADO_TWINS
		GUI.enabled = false;
		#endif

		GUILayout.BeginHorizontal();

			EditorGUI.showMixedValue = uv_diff["flipU"];
			uv_gui.flipU = GUILayout.Toggle(uv_gui.flipU, "Flip U", GUILayout.MaxWidth(oneThird), GUILayout.MinWidth(oneThird));
			if(GUI.changed) { SetFlipU(uv_gui.flipU, selection);  UpdateDiffDictionary();  GUI.changed = false; }

			EditorGUI.showMixedValue = uv_diff["flipV"];
			uv_gui.flipV = GUILayout.Toggle(uv_gui.flipV, "Flip V", GUILayout.MaxWidth(oneThird), GUILayout.MinWidth(oneThird));
			if(GUI.changed) { SetFlipV(uv_gui.flipV, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			
			EditorGUI.showMixedValue = uv_diff["swapUV"];
			uv_gui.swapUV = GUILayout.Toggle(uv_gui.swapUV, "Swap U/V", GUILayout.MaxWidth(oneThird), GUILayout.MinWidth(oneThird));
			if(GUI.changed) { SetSwapUV(uv_gui.swapUV, selection);  UpdateDiffDictionary();  GUI.changed = false; }

		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			
			#if FREE || TORNADO_TWINS
			GUI.enabled = true;
			#endif
			EditorGUI.showMixedValue = false;
			GUI.changed = false;
			if(pb_Editor.instanceIfExists)
				pb_Editor.instanceIfExists.drawHandles = GUILayout.Toggle(pb_Editor.instanceIfExists.drawHandles, "Highlight", GUILayout.MaxWidth(oneThird), GUILayout.MinWidth(oneThird));
			if(GUI.changed) { SceneView.RepaintAll(); GUI.changed = false; }

			#if FREE || TORNADO_TWINS
			GUI.enabled = false;
			#endif
			EditorGUI.showMixedValue = uv_diff["useWorldSpace"];
			uv_gui.useWorldSpace = GUILayout.Toggle(uv_gui.useWorldSpace, "World Space");
			if(GUI.changed) { SetUseWorldSpace(uv_gui.useWorldSpace, selection);  UpdateDiffDictionary();  GUI.changed = false; }

		GUILayout.EndHorizontal();
		
		GUILayout.Space(8);

		#if TORNADO_TWINS
		GUI.enabled = true;
		#endif	
		GUILayout.BeginVertical();
		{
			/* SCALE */
			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["scalex"];
				GUILayout.Label("X Scale", GUILayout.MaxWidth(oneFourth));
				uv_gui.scale.x = EditorGUILayout.FloatField(uv_gui.scale.x, GUILayout.MaxWidth(oneFourth));
				if(GUI.changed) { SetScale(uv_gui.scale, XY.X, selection);  UpdateDiffDictionary();  GUI.changed = false; }
					
				EditorGUI.showMixedValue = uv_diff["scaley"];
				GUILayout.Label("Y Scale", GUILayout.MaxWidth(oneFourth));
				uv_gui.scale.y = EditorGUILayout.FloatField(uv_gui.scale.y, GUILayout.MaxWidth(oneFourth));
				if(GUI.changed) { SetScale(uv_gui.scale, XY.Y, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			GUILayout.EndHorizontal();

		#if FREE
			GUI.enabled = true;
		#endif		
			/* OFFSET */
			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["offsetx"];
				GUILayout.Label("X Offset", GUILayout.MaxWidth(oneFourth));
				uv_gui.offset.x = EditorGUILayout.FloatField(uv_gui.offset.x, GUILayout.MaxWidth(oneFourth));
				if(GUI.changed) { SetOffset(uv_gui.offset, XY.X, selection);  UpdateDiffDictionary();  GUI.changed = false; }
				
				EditorGUI.showMixedValue = uv_diff["offsety"];
				GUILayout.Label("Y Offset", GUILayout.MaxWidth(oneFourth));
				uv_gui.offset.y = EditorGUILayout.FloatField(uv_gui.offset.y, GUILayout.MaxWidth(oneFourth));
				if(GUI.changed) { SetOffset(uv_gui.offset, XY.Y, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			GUILayout.EndHorizontal();

		#if FREE
			GUI.enabled = false;
		#endif
			/* ROTATION */
			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["rotation"];
				GUILayout.Label("Rotation", GUILayout.MaxWidth(oneFourth), GUILayout.MinWidth(oneFourth));
				uv_gui.rotation = EditorGUILayout.FloatField(uv_gui.rotation, GUILayout.MaxWidth(oneFourth), GUILayout.MinWidth(oneFourth));
				if(GUI.changed) { SetRotation(uv_gui.rotation, selection);  UpdateDiffDictionary();  GUI.changed = false; }
		
				if(GUILayout.Button("Smoothing", GUILayout.MaxHeight(18)))
					OpenSmoothingGroups();
			GUILayout.EndHorizontal();
		}

		// HAX
		pb_Object[] sel = selection;
		if(sel != null && sel.Length > 0 && sel[0].SelectedFaces.Length > 0)
		{
			int texGroup = sel[0].SelectedFaces[0].textureGroup;
			int t_group = texGroup;

			GUILayout.BeginHorizontal();
			t_group = EditorGUILayout.IntField("Texture Group", t_group);
			if(t_group > -1)
			{
				if(GUILayout.Button(new GUIContent("Select", "Selects all faces contained in this texture group."), GUILayout.MaxWidth(80)))
				{
					for(int i = 0; i < sel.Length; i++)
					{
						List<pb_Face> faces = new List<pb_Face>();
						foreach(pb_Face f in sel[i].faces)	
							if(f.textureGroup == t_group)
								faces.Add(f);
						sel[i].SetSelectedFaces(faces.ToArray());
						pb_Editor.instanceIfExists.UpdateSelection();
					}
				}
			}
			GUILayout.EndHorizontal();
			
			if(texGroup != t_group)
				for(int i = 0; i < sel.Length; i++)
					foreach(pb_Face face in sel[i].SelectedFaces)
						face.textureGroup = t_group;
		}

		if(GUILayout.Button("Group Selected Face UVs"))
			for(int i = 0; i < sel.Length; i++)
				TextureGroupSelectedFaces(sel[i]);

		GUILayout.EndVertical();
		
		GUILayout.Space(8);
		
		GUILayout.BeginVertical();
		#if TORNADO_TWINS
			GUI.enabled = true;
		#endif
			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["fill"];
				GUILayout.Label("Fill Mode", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
				uv_gui.fill = (pb_UV.Fill)EditorGUILayout.EnumPopup(uv_gui.fill);
				if(GUI.changed) { SetFill(uv_gui.fill, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			GUILayout.EndHorizontal();	
		#if TORNADO_TWINS
			GUI.enabled = false;
		#endif

			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["justify"];
				GUILayout.Label("Justify", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
				uv_gui.justify = (pb_UV.Justify)EditorGUILayout.EnumPopup(uv_gui.justify);
				if(GUI.changed) { SetJustify(uv_gui.justify, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				EditorGUI.showMixedValue = uv_diff["projectionAxis"];
				GUILayout.Label("Projection", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
				uv_gui.projectionAxis = (pb_UV.ProjectionAxis)EditorGUILayout.EnumPopup(uv_gui.projectionAxis);
				if(GUI.changed) { SetProjectionAxis(uv_gui.projectionAxis, selection);  UpdateDiffDictionary();  GUI.changed = false; }
			GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		// if(GUILayout.Button("Continnue"))
		// 	Continue();

		GUILayout.EndVertical();

		GUILayout.EndScrollView();

		#if FREE || TORNADO_TWINS
		GUI.enabled = true;
		#endif
		
		if(GUI.changed) {
			UpdateDiffDictionary();
			RepaintSceneViews();
		}
	}
#endregion

#region MODIFY SINGLE PROPERTIES

	/**
	 *	When modifying single properties, also set the textureGroup back to 0 since
	 *	generally users don't want their face UVs to snap back to form after applying
	 *	a change.
	 */
	public static void TranslateOffset(Vector2 delta, pb_Object[] sel)
	{
		delta.x = -delta.x;
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) 
			{
				q.uv.offset -= delta.DivideBy(q.uv.scale);
			}

			sel[i].RefreshUV(pb_Editor.instance.SelectedFacesInEditZone[i]);
		}
	}

	public static void TranslateRotation(float delta, pb_Object[] sel)
	{
		#if !FREE
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.rotation += delta;
			}
			sel[i].RefreshUV(pb_Editor.instance.SelectedFacesInEditZone[i]);
		}
		#endif
	}

	public static void TranslateScale(Vector2 delta, pb_Object[] sel)
	{
		#if !FREE
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.scale += delta;
			}
			sel[i].RefreshUV(pb_Editor.instance.SelectedFacesInEditZone[i]);
		}
		#endif
	}

	// These could be public if we added a selection param
	private void SetFlipU(bool flipU, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.flipU = flipU;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetFlipV(bool flipV, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++) {
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.flipV = flipV;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetSwapUV(bool swapUV, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++) {
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.swapUV = swapUV;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetUseWorldSpace(bool useWorldSpace, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++) {
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.useWorldSpace = useWorldSpace;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetFill(pb_UV.Fill fill, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.fill = fill;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetJustify(pb_UV.Justify justify, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.justify = justify;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetProjectionAxis(pb_UV.ProjectionAxis projectionAxis, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.projectionAxis = projectionAxis;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}
	}

	private void SetOffset(Vector2 offset, XY xy, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				switch(xy)
				{
					case XY.XY:
						q.uv.offset = offset;
						break;
					case XY.X:
						q.uv.offset.x = offset.x;
						break;
					case XY.Y:
						q.uv.offset.y = offset.y;
						break;
				}
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}		
	}

	private void SetRotation(float rot, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				q.uv.rotation = rot;
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}		
	}	

	private void SetScale(Vector2 scale, XY xy, pb_Object[] sel)
	{
		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face q in sel[i].SelectedFaces) {
				switch(xy)
				{
					case XY.XY:
						q.uv.scale = scale;
						break;
					case XY.X:
						q.uv.scale.x = scale.x;
						break;
					case XY.Y:
						q.uv.scale.y = scale.y;
						break;
				}
			}
			sel[i].RefreshUV( SelectedFacesInEditZone[i] );
		}		
	}
#endregion

#region MODIFY PER-UV SETTINGS

	public void ApplyMaterial(pb_Object[] selected, Material mat)
	{
		if(mat==null)
			return;

		pbUndo.RecordObjects(selected, "Apply Material + " + mat.name);

		int selectedFaces = 0;
		foreach(pb_Object pb in selected)
		{
			EntityType ent = pb.GetComponent<pb_Entity>().entityType;
			if(ent == EntityType.Trigger || ent  == EntityType.Collider)
				continue;

			selectedFaces += pb.SelectedFaces.Length;
			pb.SetFaceMaterial(pb.SelectedFaces, mat);
		}

		// If no faces are selected, apply it to ALL the faces!
		if(selectedFaces < 1)
		foreach(pb_Object pb in selected)
		{
			if(pb.GetComponent<pb_Entity>().entityType == EntityType.Trigger || 
				pb.GetComponent<pb_Entity>().entityType == EntityType.Collider)
				continue;
				
			pb.SetFaceMaterial(pb.faces, mat);
		}

		foreach(pb_Object pb in selected)
		{	
			pb.GenerateUV2(pb_Editor.show_NoDraw);
		}


		OnFaceChanged( selected );
		RepaintSceneViews();
	}

	public void ApplyMaterial(pb_Object pb, pb_Face quad, Material mat)
	{
		if(mat == null)
			return;
			
		pbUndo.RecordObject(pb, "Apply Material + " + mat.name);

		if(mat)
			pb.SetFaceMaterial(quad, mat);

		pb.GenerateUV2(pb_Editor.show_NoDraw);
		OnSelectionUpdate(selection);		

		OnFaceChanged( pb );
	}

	public static void ApplyNoDraw(pb_Object[] sel, bool showNoDraw)
	{
		Material nodrawMat = (Material)Resources.Load("Materials/NoDraw");
		if(nodrawMat == null)
			Debug.LogError("NoDraw material not found. Please create a Material named NoDraw in the Materials folder");

		pbUndo.RecordObjects(sel, "Apply NoDraw to " + sel.Length + " object(s).");

		for(int i = 0; i < sel.Length; i++)
		{
			foreach(pb_Face face in sel[i].SelectedFaces)
				face.SetMaterial(nodrawMat);
			sel[i].ToMesh(!showNoDraw);
			sel[i].GenerateUV2(showNoDraw);
	
			if(pb_Editor.instanceIfExists != null)
				sel[i].RefreshUV(pb_Editor.instanceIfExists.SelectedFacesInEditZone[i]);
		}

		OnFaceChanged( sel );
	}
#endregion

#region MODIFY MULTIPLE FACES
	
	private void TextureGroupSelectedFaces(pb_Object pb)//, pb_Face face)
	{
		if(pb.SelectedFaces.Length < 1) return;

		pb_UV cont_uv = pb.SelectedFaces[0].uv;

		int texGroup = pb.UnusedTextureGroup();

		foreach(pb_Face f in pb.SelectedFaces)
		{
			f.SetUV( new pb_UV(cont_uv) );
			f.textureGroup = texGroup;
		}

		pb.RefreshUV(pb.SelectedFaces);
		SceneView.RepaintAll();
	}
#endregion

#region SHORTCUTS

	public bool ClickShortcutCheck(EventModifiers em, pb_Object pb, pb_Face quad)
	{
		// if(em == (EventModifiers.Control | EventModifiers.Shift | EventModifiers.Alt))
		// {
		// 	Continue(pb, quad);
		// 	pb_Editor_Utility.ShowNotification("Continue UV");
		// 	return true;
		// }

		if(em == (EventModifiers.Control | EventModifiers.Shift)) {
			ApplyMaterial(pb, quad, queuedMat);
			RepaintSceneViews();
			pb_Editor_Utility.ShowNotification("Quick Apply Material");
			return true;
		}

		if(em == (EventModifiers.Control))
		{
			pb.SetFaceUV(quad, new pb_UV(uv_gui));
			RepaintSceneViews();
			pb_Editor_Utility.ShowNotification("Copy UV Settings");
			return true;
		}

		return false;
	}
#endregion

#region EVENT


	/**
	 *	\brief Used to check whether object is nodraw free or not - matching call is in pb_Texture_Editor.SetNoDraw
	 */
	private static void OnFaceChanged( pb_Object[] pbs )
	{
		// check the object flags
		foreach(pb_Object pb in pbs)
		{
			OnFaceChanged( pb );
		}
	}

	private static void OnFaceChanged( pb_Object pb )
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
#endregion

#region EDITOR CACHES

	public void OnSelectionUpdate(pb_Object[] sel)
	{
	
		uv_selection.Clear();
		
		// Cache all selected UV settings
		for(int i = 0; i < sel.Length; i++) {
			foreach(pb_Face q in sel[i].SelectedFaces) {
				uv_selection.Add(q.uv);
			}
		}

		UpdateDiffDictionary();

		if(selection != null && selection.Length > 0) 
		{
			if(selection[0].SelectedFaces.Length > 0)
			{
				uv_gui = selection[0].SelectedFaces[0].uv;
				
				currentMat = selection[0].SelectedFaces[0].material;

				if(queuedMat == null)
					queuedMat = currentMat;
			}
		}
		else
		{
			currentMat = null;
		}

		selection = sel;
		if(pb_Editor.instanceIfExists)
			SelectedFacesInEditZone = pb_Editor.instanceIfExists.SelectedFacesInEditZone;

		if(smoothingEditor) smoothingEditor.UpdateSelection(selection);
	}

	public void UpdateDiffDictionary()
	{
		// Clear values for each iteration
		foreach(string key in uv_diff.Keys.ToList())
			uv_diff[key] = false;

		if(uv_selection.Count < 1)
			return;

		pb_UV sample = uv_selection[0];
		foreach(pb_UV u in uv_selection) {
			if(u.projectionAxis != sample.projectionAxis)
				uv_diff["projectionAxis"] = true;
			if(u.useWorldSpace != sample.useWorldSpace)
				uv_diff["useWorldSpace"] = true;
			if(u.flipU != sample.flipU)
				uv_diff["flipU"] = true;
			if(u.flipV != sample.flipV)
				uv_diff["flipV"] = true;
			if(u.swapUV != sample.swapUV)
				uv_diff["swapUV"] = true;
			if(u.fill != sample.fill)
				uv_diff["fill"] = true;
			if(u.scale.x != sample.scale.x)
				uv_diff["scalex"] = true;
			if(u.scale.y != sample.scale.y)
				uv_diff["scaley"] = true;
			if(u.offset.x != sample.offset.x)
				uv_diff["offsetx"] = true;
			if(u.offset.y != sample.offset.y)
				uv_diff["offsety"] = true;
			if(u.rotation != sample.rotation)
				uv_diff["rotation"] = true;
			if(u.justify != sample.justify)
				uv_diff["justify"] = true;
		}
		// Debug.Log(pbUtil.StringWithDictionary<string, bool>(uv_diff));
	}
#endregion

#region EDITOR / SCENE UTILS

	public void RepaintSceneViews()
	{
		if(SceneView.sceneViews.Count > 0)
			foreach(SceneView sv in SceneView.sceneViews)
				sv.Repaint();
	}

	public void OnInspectorUpdate()
	{
		if(EditorWindow.focusedWindow != this)
			Repaint();
	}
#endregion

#region WINDOW MANAGEMENT
	
	public void OpenSmoothingGroups()
	{
		smoothingEditor = pb_Smoothing_Editor.Init(this, selection);
	}
#endregion

#region SNAP NUDGE

	// public void SetNudgeValue(float nudge)
	// {
	// 	EditorPrefs.SetFloat("pbNudgeValue", nudge);

	// 	NudgeValueX = new Vector2(nudge, 0f);	
	// 	NudgeValueY = new Vector2(0f, nudge);	
	// }

	// public float GetNudgeValue()
	// {
	// 	if(EditorPrefs.HasKey("pbNudgeValue"))
	// 		return EditorPrefs.GetFloat("pbNudgeValue");
	// 	else
	// 		return .25f;
	// }

	public void SnapUVSettings(ref pb_UV uvs, float snapValue)
	{
		uvs.scale = new Vector2(
			snapValue * Mathf.Round(uvs.scale.x / snapValue), 
			snapValue * Mathf.Round(uvs.scale.y / snapValue));
		uvs.offset = new Vector2(
			snapValue * Mathf.Round(uvs.offset.x / snapValue), 
			snapValue * Mathf.Round(uvs.offset.y / snapValue));
		uvs.rotation = snapValue * Mathf.Round(uvs.rotation / snapValue);
	}
#endregion

#region GUI UTILS

	public void OnWindowResize()
	{	
		prevWidth = Screen.width;
		prevHeight = Screen.height;

		oneThird = (Screen.width/3)-8;
		oneFourth = (Screen.width/4)-5;

		//currentMatRect = new Rect(Screen.width-previewMatSize-pad, pad, previewMatSize, previewMatSize);
		currentMatRect = new Rect(1,28,Screen.width-2,Screen.width-2);
		currentMatRect_inset = new Rect(currentMatRect.x+inset, currentMatRect.y+inset, currentMatRect.width-(inset*2), currentMatRect.height-(inset*2));
		
		// I dunno where this should go.  For now, just steal the current mat's rect and don't render the current material.
		queuedMatRect = currentMatRect;
		queuedMatRect_inset = currentMatRect_inset;

		//applyButtonRect = new Rect(queuedMatRect.x + (queuedMatRect.width-47), queuedMatRect.y + (queuedMatRect.height-15), 46, 14);
		applyButtonRect = new Rect(queuedMatRect.x + (queuedMatRect.width-47), queuedMatRect.y + (queuedMatRect.height-15), 46, 14);
	}

	public void DrawMatPreview(Material mat, Rect matRect, Rect matRect_inset, string alt)
	{
		if(mat != null)	
		{
			GUI.Label(matRect, "", EditorStyles.objectFieldThumb);

			if(mat.mainTexture != null)
				GUI.Label(matRect_inset, mat.mainTexture);
				// EditorGUI.DrawPreviewTexture(matRect_inset, mat.mainTexture, mat);			
			// else
			// 	EditorGUI.DrawPreviewTexture(matRect_inset, EditorGUIUtility.whiteTexture, mat);

			if(	Event.current.type == EventType.MouseUp && 
				Event.current.button == 0 && matRect.Contains(Event.current.mousePosition) && 
				!applyButtonRect.Contains(Event.current.mousePosition))
				EditorGUIUtility.PingObject(mat);
		}
		else
			GUI.Label(matRect, alt, EditorStyles.objectFieldThumb);		
	}
#endregion

	public bool undoRedoPerformed { get { return Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed"; } }

#endif
}
