#if !PROTOTYPE

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class VertexMergeWeld : Editor
	{
		/**
		 *	Collapses all selected vertices to a single central vertex.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Collapse Selected Vertices &c", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 0)]
		public static void CollapseVertices()
		{

			bool success = false;
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				if(pb.SelectedTriangles.Length > 1)
				{
					pbUndo.RecordObject(pb, "Collapse Vertices");
					success = pb.MergeVertices(pb.SelectedTriangles);
					
					pb.RemoveDegenerateTriangles();
					
					pb.Refresh();
					pb.GenerateUV2(true);
				}
			}

			if(success)
				pb_Editor_Utility.ShowNotification("Collapse Vertices", "");

			pb_Editor.instance.UpdateSelection();
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}

		
		/**
		 *	For each vertex within epsilon distance, collapse.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Weld Selected Vertices &v", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 1)]
		public static void WeldVertices()
		{

			bool success = false;
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				if(pb.SelectedTriangles.Length > 1)
				{
					pbUndo.RecordObject(pb, "Weld Vertices");
					success = pb.WeldVertices(pb.SelectedTriangles, Mathf.Epsilon);

					pb.RemoveDegenerateTriangles();

					pb.GenerateUV2(true);
					pb.Refresh();
				}
			}

			if(success)
				pb_Editor_Utility.ShowNotification("Weld Vertices", "");

			pb_Editor.instance.UpdateSelection();
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	
		/**
		 *	Splits the selected vertices into separate vertices.  When faces are selected, they will be detached (meaning only the selected face
		 *	vertex will be removed from the shared index).
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Split Selected Vertices &x", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 2)]
		public static void SplitVertices()
		{
			int splitCount = 0;
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pbUndo.RecordObject(pb, "Split Vertices");
				
				List<int> tris = new List<int>(pb.SelectedTriangles);			// loose verts to split

				if(pb.SelectedFaces.Length > 0)
				{
					pb_IntArray[] sharedIndices = pb.sharedIndices;

					int[] selTrisIndices = new int[pb.SelectedTriangles.Length];

					// Get sharedIndices index for each vert in selection
					for(int i = 0; i < pb.SelectedTriangles.Length; i++)
						selTrisIndices[i] = sharedIndices.IndexOf(pb.SelectedTriangles[i]);

					// cycle through selected faces and remove the tris that compose full faces.
					foreach(pb_Face face in pb.SelectedFaces)
					{
						List<int> faceSharedIndices = new List<int>();

						for(int j = 0; j < face.distinctIndices.Length; j++)
							faceSharedIndices.Add( sharedIndices.IndexOf(face.distinctIndices[j]) );

						List<int> usedTris = new List<int>();
						for(int i = 0; i < selTrisIndices.Length; i++)	
							if( faceSharedIndices.Contains(selTrisIndices[i]) )
								usedTris.Add(pb.SelectedTriangles[i]);

						// This face *is* composed of selected tris.  Remove these tris from the loose index list
						foreach(int i in usedTris)	
							if(tris.Contains(i))
								tris.Remove(i);
					}
				}
	
				// Now split the faces, and any loose vertices
				foreach(pb_Face f in pb.SelectedFaces)
					pb.DetachFace(f);

				splitCount += pb.SelectedTriangles.Length;
				pb.SplitVertices(pb.SelectedTriangles);
		
				// Reattach detached face vertices (if any are to be had)
				if(pb.SelectedFaces.Length > 0)
					pb.WeldVertices( pb_Face.AllTriangles(pb.SelectedFaces), Mathf.Epsilon );
		
				// And set the selected triangles to the newly split
				List<int> newTriSelection = new List<int>(pb_Face.AllTriangles(pb.SelectedFaces));
				newTriSelection.AddRange(tris);
				pb.SetSelectedTriangles(newTriSelection.ToArray());
				
				pb.Refresh();
				pb.GenerateUV2(true);
			}

			pb_Editor_Utility.ShowNotification("Split " + splitCount + " Vertices", "");

			pb_Editor.instance.UpdateSelection();
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}
#endif