using UnityEngine;
using UnityEditor;
using System.Collections;

public class SetProjectionAxis : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Reset UV Projection Axis", false, pb_Constant.MENU_REPAIR)]
	public static void InitProjAxisUV()
	{
		foreach(pb_Object pb in Resources.FindObjectsOfTypeAll(typeof(pb_Object)))
		{
			foreach(pb_Face face in pb.faces)
			{
				pb_UV uv = face.uv;
				uv.projectionAxis = pb_UV.ProjectionAxis.AUTO;
			}
			pb.RefreshUV();
		}
	}
}
