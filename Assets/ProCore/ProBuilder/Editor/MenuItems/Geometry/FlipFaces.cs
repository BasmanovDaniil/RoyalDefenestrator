using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

public class FlipFaces : Editor {

	// [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Object Normals")]
	// public static void FlipObjectNormals()
	// {
	// 	foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
	// 		pb.ReverseWindingOrder();
	// }

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Face Normals &n", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
	public static void FlipFaceNormals()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			pb.ReverseWindingOrder(pb.SelectedFaces);
			pb.Refresh();
			pb.GenerateUV2(true);
		}
		
		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}	
}
