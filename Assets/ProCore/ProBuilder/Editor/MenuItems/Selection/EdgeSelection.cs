using UnityEditor;
using UnityEngine;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class EdgeSelection : Editor
	{

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Edge Ring &r")]
		public static void MenuEdgeLoop()
		{
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pbUndo.RecordObject(pb, "Select Edge Loop");
				pb.SetSelectedEdges( pbMeshUtils.GetEdgeRing(pb, pb.SelectedEdges) );
			}
			
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}

		// [MenuItem("Tools/ProBuilder/Selection/Edge Loop &l")]
		// public static void MenuEdgeRing()
		// {
		// 	foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		// 	{
		// 		pbUndo.RecordObject(pb, "Select Edge Ring");
		// 		pb.SetSelectedEdges( pbMeshUtils.GetEdgeLoop(pb, pb.SelectedEdges));
		// 	}
		// }
	}
}