#pragma warning disable 0162 // TODO - FIX
#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#endif

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4
#endif

#undef UNITY_4

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorEnum;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using System.Collections.Generic;

#if BUGGER
using Parabox.Bugger;
#endif

[CustomEditor(typeof(pb_Object))]
[CanEditMultipleObjects]
public class pb_Object_Editor : Editor
{
	public delegate void OnGetFrameBoundsDelegate ();
	public static event OnGetFrameBoundsDelegate OnGetFrameBoundsEvent;


	pb_Object pb;
	
	// RectOffset buttonPadding = new RectOffset(2, 2, 2, 2);
	bool info = false;
	Renderer ren;
	Vector3 offset = Vector3.zero;

	public void OnEnable()
	{	
		if(EditorApplication.isPlayingOrWillChangePlaymode)
			return;
		
		if(target is pb_Object)
			pb = (pb_Object)target;
		else
			return;

		ren = pb.gameObject.GetComponent<Renderer>();

		// get all materials in use (as far as pb_Object knows)

		// if(Selection.activeTransform != pb.transform) //System.Array.IndexOf(Selection.transforms, pb.transform) < 0 )
		// Unity drag and drop material always only sets the first sub-object material, so check that it's the same
		// if(ren.sharedMaterials.Length > 0)
		// {
		// 	Bugger.Log("OnEnable set face material");

		// 	HashSet<Material> mats = new HashSet<Material>();
		// 	foreach(pb_Face f in pb.faces)
		// 		mats.Add(f.material);

		// 	HashSet<Material> renMats = new HashSet<Material>(ren.sharedMaterials);

		// 	if(!renMats.SetEquals(mats))
		// 	{

		// 		pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Set Face Materials");
				
		// 		foreach(pb_Object pbs in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		// 			pbs.SetFaceMaterial(pbs.faces, ren.sharedMaterials[0]);
		// 	}
		// }

		#if UNITY_4
		EditorUtility.SetSelectedWireframeHidden(ren, true);
		#else
		EditorUtility.SetSelectedWireframeHidden(ren, false);
		#endif

		pb.Verify();
		pb.GenerateUV2(true);
	}

	// bool pbInspectorFoldout = false;
	public override void OnInspectorGUI()
	{
		GUI.backgroundColor = Color.green;

		if(GUILayout.Button("Open " + pb_Constant.PRODUCT_NAME))
			if (EditorPrefs.HasKey(pb_Constant.pbDefaultOpenInDockableWindow) && 
				!EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow))
				EditorWindow.GetWindow(typeof(pb_Editor), true, pb_Constant.PRODUCT_NAME, true);			// open as floating window
			else
				EditorWindow.GetWindow(typeof(pb_Editor), false, pb_Constant.PRODUCT_NAME, true);			// open as dockable window

		GUI.backgroundColor = Color.white;

		info = EditorGUILayout.Foldout(info, "Info");

		if(info)
		{
			Vector3 sz = ren.bounds.size;
			EditorGUILayout.Vector3Field("Object Size (read only)", sz);
		}

		if(pb == null) return;
		
		if(pb.SelectedTriangles.Length > 0)
		{
			offset = EditorGUILayout.Vector3Field("Quick Offset", offset);
			if(GUILayout.Button("Apply Offset"))
			{
				pbUndo.RecordObject(pb, "Offset Vertices");
				pb.TranslateVertices(pb.SelectedTriangles, offset);
				pb.Refresh();
				if(pb_Editor.instanceIfExists != null)
					pb_Editor.instance.UpdateSelection();
			}
		}
	}

	void OnSceneGUI()
	{
		// #if !PROTOTYPE
		// 	// Event.current.type == EventType.DragUpdated || 
		// 	if(Event.current.type == EventType.DragPerform)
		// 	{
		// 		if( HandleUtility.PickGameObject(Event.current.mousePosition, false) == pb.gameObject )
		// 		{
		// 			Material mat = null;
		// 			foreach(Object t in DragAndDrop.objectReferences)
		// 			{
		// 				if(t is Material)
		// 				{
		// 					mat = (Material)t;
		// 					break;
		// 				}
		// 				/* This works, but throws some bullshit errors. Not creating a material leaks, so disable this functionality. */
		// 				// else
		// 				// if(t is Texture2D)
		// 				// {
		// 				// 	mat = new Material(Shader.Find("Diffuse"));
		// 				// 	mat.mainTexture = (Texture2D)t;

		// 				// 	// string texPath = AssetDatabase.GetAssetPath(mat.mainTexture);
		// 				// 	// int lastDot = texPath.LastIndexOf(".");
		// 				// 	// texPath = texPath.Substring(0, texPath.Length - (texPath.Length-lastDot));
		// 				// 	// texPath = AssetDatabase.GenerateUniqueAssetPath(texPath + ".mat");

		// 				// 	AssetDatabase.CreateAsset(mat, texPath);
		// 				// 	AssetDatabase.Refresh();
					
		// 				// 	break;
		// 				// }
		// 			}
					
		// 			if(mat != null)
		// 			{
		// 				if(	pb_Editor.instanceIfExists != null &&
		// 					pb_Editor.instanceIfExists.editLevel == EditLevel.Geometry)
		// 				{
		// 					// Bugger.Log("Setting SELECT face material - DragAndDrop");
		// 					pbUndo.RecordObjects(pb_Editor.instanceIfExists.selection, "Set Face Materials");
		// 					foreach(pb_Object pbs in pb_Editor.instanceIfExists.selection)
		// 					{
		// 						pbs.SetFaceMaterial(pbs.SelectedFaces.Length < 1 ? pbs.faces : pbs.SelectedFaces, mat);
		// 					}
		// 				}
		// 				else
		// 				{
		// 					// Bugger.Log("Setting ALL face material - DragAndDrop");
		// 					pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Set Face Materials");
		// 					foreach(pb_Object pbs in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		// 					{
		// 						pbs.SetFaceMaterial(pbs.faces, mat);
		// 					}
		// 				}

		// 				pb.GenerateUV2(true);
		// 				Event.current.Use();
		// 			}
		// 		}	
		// 	}
		// #endif
			
		if(EditorApplication.isPlayingOrWillChangePlaymode || pb == null)
			return;

		if(GUIUtility.hotControl < 1 && pb.transform.localScale != Vector3.one)
			pb.FreezeScaleTransform();
	}

	bool HasFrameBounds() 
	{
		if(pb == null)
			pb = (pb_Object)target;

		return pb.SelectedTriangles.Length > 0;
	}

	Bounds OnGetFrameBounds()
	{
		if(OnGetFrameBoundsEvent != null) OnGetFrameBoundsEvent();

		Vector3[] verts = pb.VerticesInWorldSpace();
		
		if(pb.SelectedTriangles.Length < 2)
			return new Bounds(verts[pb.SelectedTriangles[0]], Vector3.one * .2f);

		Vector3 min = verts[pb.SelectedTriangles[0]], max = min;
		
		for(int i = 1; i < pb.SelectedTriangles.Length; i++)
		{
			int j = pb.SelectedTriangles[i];

			min.x = Mathf.Min(verts[j].x, min.x);
			max.x = Mathf.Max(verts[j].x, max.x);
			min.y = Mathf.Min(verts[j].y, min.y);
			max.y = Mathf.Max(verts[j].y, max.y);
			min.z = Mathf.Min(verts[j].z, min.z);
			max.z = Mathf.Max(verts[j].z, max.z);

		}

		return new Bounds( (min+max)/2f, max-min );
	}
}