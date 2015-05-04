#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProCore.Common;

#if BUGGER
using Parabox.Bugger;
#endif

public static class pb_Editor_Utility
{
#region NOTIFICATION MANAGER

	const float TIMER_DISPLAY_TIME = 1f;
	private static float notifTimer = 0f;
	private static bool notifDisplayed = false;

	public static void ShowNotification(string notif)
	{
		ShowNotification(notif, "");
	}

	public static void ShowNotification(string notif, string help)
	{
		if(EditorPrefs.HasKey(pb_Constant.pbShowEditorNotifications) && !EditorPrefs.GetBool(pb_Constant.pbShowEditorNotifications))
			return;
			
		SceneView scnview = SceneView.lastActiveSceneView;
		if(scnview == null)
			scnview = EditorWindow.GetWindow<SceneView>();
		
		scnview.ShowNotification(new GUIContent(notif, help));
		scnview.Repaint();

		if(EditorApplication.update != NotifUpdate)
			EditorApplication.update += NotifUpdate;

		notifTimer = Time.realtimeSinceStartup + TIMER_DISPLAY_TIME;
		notifDisplayed = true;
	}
	
	public static void RemoveNotification()
	{
		SceneView scnview = GetSceneView();
		
		scnview.RemoveNotification();
		scnview.Repaint();
	}

	private static void NotifUpdate()
	{
		if(notifDisplayed && Time.realtimeSinceStartup > notifTimer)
		{
			notifDisplayed = false;
			RemoveNotification();
		}
	}
#endregion

#region OBJECT

	/**
	 *	\brief Force refreshes all meshes in scene.
	 */
	public static void ForceRefresh(bool interactive)
	{
		pb_Object[] all = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
		for(int i = 0; i < all.Length; i++)
		{
			if(interactive)
			EditorUtility.DisplayProgressBar(
				"Refreshing ProBuilder Objects",
				"Reshaping pb_Object " + all[i].id + ".",
				((float)i / all.Length));

			all[i].Refresh();
		}
		if(interactive)
		{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Refresh ProBuilder Objects", "Successfully refreshed all ProBuilder objects in scene.", "Okay");
		}
	}
#endregion

#region GUI 
	
	public static Rect GUIRectWithObject(GameObject go)
	{
		Vector3 cen = go.GetComponent<Renderer>().bounds.center;
		Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
		Vector2[] extentPoints = new Vector2[8]
		{
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),

			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
		};

		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];

		foreach(Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}

		return new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
	}
#endregion

#region UV WRAPPING
#endregion

#region OBJ EXPORT

	public static string ExportOBJ(pb_Object[] pb)
	{
		if(pb.Length < 1) return "";

		pb_Object combined;
		if(pb.Length > 1)
			pbMeshOps.CombineObjects(pb, out combined);
		else
			combined = pb[0];

		// re-enable since CombineObjects sets all objs to disabled
		// foreach(pb_Object p in pb) {
		// 	// blech!  shield your eyes!
		// 	System.Type pbVB = Assembly.Load("Assembly-CSharp").GetTypes().First(t => t.Name == "pbVersionBridge");
		// 	MethodInfo setActive = pbVB.GetMethod("SetActive");
		// 	setActive.Invoke(null, new object[2]{p.gameObject, true});
		// 	// p.MakeUnique();
		// }

		string path = EditorUtility.SaveFilePanel("Save ProBuilder Object as Obj", "", "pb" + pb[0].id + ".obj", "");
		if(path == null || path == "")
		{
			if(pb.Length > 1) {
				GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
				GameObject.DestroyImmediate(combined.gameObject);
			}
			return "";
		}
		EditorObjExporter.MeshToFile(combined.GetComponent<MeshFilter>(), path);
		AssetDatabase.Refresh();

		if(pb.Length > 1) {
			GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
			GameObject.DestroyImmediate(combined.gameObject);
		}
		return path;
	}
#endregion

#region ENTITY

	/**
	 *	\brief Sets the EntityType for the passed gameObject. 
	 *	@param newEntityType The type to set.
	 *	@param target The gameObject to apply the EntityType to.  Must contains pb_Object and pb_Entity components.  Method does contain null checks.
	 */
	public static void SetEntityType(this pb_Entity pb, EntityType newEntityType)
	{
		SetEntityType(newEntityType, pb.gameObject);
	}

	public static void SetEntityType(EntityType newEntityType, GameObject target)
	{
		pb_Entity ent = target.GetComponent<pb_Entity>();
		
		if(ent == null)
			ent = target.AddComponent<pb_Entity>();

		pb_Object pb = target.GetComponent<pb_Object>();

		if(!ent || !pb)
			return;

		SetDefaultEditorFlags(target);

		switch(newEntityType)
		{
			case EntityType.Detail:
				SetBrush(target);
				break;

			case EntityType.Occluder:
				SetOccluder(target);
				break;

			case EntityType.Trigger:
				SetTrigger(target);
				break;

			case EntityType.Collider:
				SetCollider(target);
				break;

			case EntityType.Mover:
				SetDynamic(target);
				break;
		}

		ent.SetEntity(newEntityType);
	}

	private static void SetBrush(GameObject target)
	{
		EntityType et = target.GetComponent<pb_Entity>().entityType;

		if(	et == EntityType.Trigger || 
			et == EntityType.Collider )
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );
	}

	private static void SetDynamic(GameObject target)
	{
		EntityType et = target.GetComponent<pb_Entity>().entityType;

		SetEditorFlags((StaticEditorFlags)0, target);

		if(	et == EntityType.Trigger || 
			et == EntityType.Collider )
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );
	}

	private static void SetOccluder(GameObject target)
	{
		EntityType et = target.GetComponent<pb_Entity>().entityType;	
		
		if(	et == EntityType.Trigger || 
			et == EntityType.Collider )
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );

		StaticEditorFlags editorFlags;
		if( !target.GetComponent<pb_Object>().containsNodraw )
			editorFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;
		else
			editorFlags = StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;
		
		SetEditorFlags(editorFlags, target);
	}

	private static void SetTrigger(GameObject target)
	{
		target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, (Material)Resources.Load("Materials/Trigger", typeof(Material)) );
		SetIsTrigger(true, target);
		SetEditorFlags((StaticEditorFlags)0, target);
	}

	private static void SetCollider(GameObject target)
	{
		target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, (Material)Resources.Load("Materials/Collider", typeof(Material)) );
		SetEditorFlags( (StaticEditorFlags)(StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration), target);
	}

	// private static void SetEditorFlagsWithBounds(StaticEditorFlags editorFlags, GameObject target)
	// {
	// 	Bounds occluderBounds = target.GetComponent<MeshRenderer>().bounds;

	// 	foreach(pb_Object pb in GameObject.FindObjectsOfType(typeof(pb_Object)))
	// 	{
	// 		if(occluderBounds.Contains(pb.gameObject.transform.position))
	// 		{
	// 			GameObjectUtility.SetStaticEditorFlags(pb.gameObject, editorFlags);
	// 		}
	// 	}
	// }

	private static void SetEditorFlags(StaticEditorFlags editorFlags, GameObject target)
	{
		GameObjectUtility.SetStaticEditorFlags(target, editorFlags);
	}	

	private static void SetIsTrigger(bool val, GameObject target)
	{
		Collider[] colliders = pbUtil.GetComponents<Collider>(target);
		foreach(Collider col in colliders)
			col.isTrigger = val;
	}

	/**
	 * Use Default static flags - StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration
	 * If NoDraw is present, BatchingStatic will not be flagged.
	 */
	private static void SetDefaultEditorFlags(GameObject target)
	{
		SetIsTrigger(false, target);
		
		StaticEditorFlags editorFlags;
		if(!target.GetComponent<pb_Object>().containsNodraw)
			editorFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;
		else
			editorFlags = StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;

		// if(target.GetComponent<pb_Entity>().entityType == EntityType.Occluder)
		// 	SetEditorFlagsWithBounds(editorFlags, target);

		SetEditorFlags(editorFlags, target);
	}
#endregion

#region EDITOR

	/**
	 * \brief ProBuilder objects created in Editor need to be initialized with a number of additional Editor-only settings.
	 *	This method provides an easy method of doing so in a single call.  #InitObjectFlags will set the Entity Type, generate 
	 *	a UV2 channel, set the unwrapping parameters, and center the object in the screen. 
	 */
	public static void InitObjectFlags(pb_Object pb, ColliderType col, EntityType et)
	{
		switch(col)
		{
			case ColliderType.BoxCollider:
				pb.gameObject.AddComponent<BoxCollider>();
			break;

			case ColliderType.MeshCollider:
				pb.gameObject.AddComponent<MeshCollider>().convex = EditorPrefs.HasKey(pb_Constant.pbForceConvex) ? EditorPrefs.GetBool(pb_Constant.pbForceConvex) : false;
				break;
		}

		pb_Lightmap_Editor.SetObjectUnwrapParamsToDefault(pb);
		pb.GenerateUV2(true);
		pb_Editor_Utility.SetEntityType(et, pb.gameObject);
		pb_Editor_Utility.ScreenCenter( pb.gameObject );
	}

	public static void ScreenCenter(GameObject _gameObject)
	{
		if(_gameObject == null)
			return;
			
		// If in the unity editor, attempt to center the object the sceneview or main camera, in that order
		_gameObject.transform.position = SceneCameraPosition();

		Selection.activeObject = _gameObject;
	}

	public static Vector3 SceneCameraPosition()
	{
		return GetSceneView().pivot;
	}

	/**
	 * If EditorPrefs say to set pivot to corner and ProGrids or PB pref says snap to grid, do it.
	 * @param indicesToCenterPivot If any values are passed here, the pivot is set to an average of all vertices at indices.  If null, the first vertex is used as the pivot.
	 */
	public static void SetPivotAndSnapWithPref(pb_Object pb, int[] indicesToCenterPivot)
	{
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot))
			pb.CenterPivot( indicesToCenterPivot == null ? new int[1]{0} : indicesToCenterPivot );
		else
			pb.CenterPivot(indicesToCenterPivot == null ? pb.uniqueIndices : indicesToCenterPivot );

		if(pbUtil.SharedSnapEnabled)
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, pbUtil.SharedSnapValue);
		else
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);
	}

	public static string[] GetScenes()
	{
		string[] allFiles = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories);
		string[] allScenes = System.Array.FindAll(allFiles, name => name.EndsWith(".unity"));
		return allScenes;
	}

	public static SceneView GetSceneView()
	{
		return SceneView.lastActiveSceneView == null ? EditorWindow.GetWindow<SceneView>() : SceneView.lastActiveSceneView;
	}

	public static void FocusSceneView()
	{
		GetSceneView().Focus();
	}
#endregion
}
