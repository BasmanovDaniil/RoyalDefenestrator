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
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorEnum;

namespace ProBuilder2.Actions
{
	public class ExtrudeFace : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude %#e", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 0)]
		public static void ExtrudeNoTranslation()
		{
			PerformExtrusion(0f);
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude with Translation %e", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 1)]
		public static void Extrude()
		{
			PerformExtrusion(.25f);
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}

		private static void PerformExtrusion(float dist)
		{
			SelectMode mode = pb_Editor.instance.GetSelectionMode();

			pb_Object[] pbs = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Extrude selected.");

			int extrudedFaceCount = 0;
			foreach(pb_Object pb in pbs)
			{
				switch(mode)
				{
					case SelectMode.Face:
					case SelectMode.Vertex:
						if(pb.SelectedFaces.Length < 1)
							continue;
						
						extrudedFaceCount += pb.SelectedFaces.Length;
						pb.Extrude(pb.SelectedFaces, dist);
						pb.SetSelectedFaces(pb.SelectedFaces);
						break;

					case SelectMode.Edge:
						
						if(pb.SelectedFaces.Length > 0)
							goto case SelectMode.Face;

						if(pb.SelectedEdges.Length < 1)
							continue;
						
						pb_Edge[] newEdges = pb.Extrude(pb.SelectedEdges, dist, pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeExtrusionOnly));

						if(newEdges != null)
						{
							extrudedFaceCount += pb.SelectedEdges.Length;
							pb.SetSelectedEdges(newEdges);
						}

						break;
				}
	
				pb.Refresh();
				pb.GenerateUV2(true);
			}

			if(extrudedFaceCount > 0)
			{
				string val = "";
				if(mode == SelectMode.Edge)
					val = (extrudedFaceCount > 1 ? extrudedFaceCount + " Edges" : "Edge");
				else
					val = (extrudedFaceCount > 1 ? extrudedFaceCount + " Faces" : "Face");
				pb_Editor_Utility.ShowNotification("Extrude " + val, "Extrudes the selected faces / edges.");
			}

			if(pb_Editor.instance)
				pb_Editor.instance.UpdateSelection();
		}
	}
}
