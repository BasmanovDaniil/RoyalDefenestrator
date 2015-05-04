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
using ProBuilder2.EditorEnum;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{

public class Bridge : Editor
{
	// [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Bridge Edges &b", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
	// public static bool VerifyEdgeMenuItem()
	// {
	// 	return pb_Editor.instanceIfExists != null && pb_Editor.instanceIfExists.selectionMode == SelectMode.Edge;
	// }
	
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Bridge Edges &b", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
	public static void BridgeEdges()
	{
		pbUndo.RecordObjects( pbUtil.GetComponents<pb_Object>(Selection.transforms), "Bridge Edges");

		bool success = false;
		bool limitToPerimeterEdges = pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			if(pb.SelectedEdges.Length == 2)
				if(pb.Bridge(pb.SelectedEdges[0], pb.SelectedEdges[1], limitToPerimeterEdges))
				{
					success = true;
					pb.GenerateUV2(true);
					pb.Refresh();
				}
		}

		if(success)
		{
			pb_Editor.instance.UpdateSelection();
			pb_Editor_Utility.ShowNotification("Bridge Edges", "");
		}
		
		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}
}
}
#endif