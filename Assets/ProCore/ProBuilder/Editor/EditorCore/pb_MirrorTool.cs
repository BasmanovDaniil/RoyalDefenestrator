using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

public class pb_MirrorTool : EditorWindow 
{
	#if !PROTOTYPE

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Mirror Tool")]
	public static void InitMirrorTool()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(pb_MirrorTool), true, "Mirror Tool", true);
		win.Show();
	}

	bool scaleX = false, scaleY = false, scaleZ = true;
	public void OnGUI()
	{
		GUILayout.Label("Mirror Axis", EditorStyles.boldLabel);
		scaleX = EditorGUILayout.Toggle("X", scaleX);
		scaleY = EditorGUILayout.Toggle("Y", scaleY);
		scaleZ = EditorGUILayout.Toggle("Z", scaleZ);

		if(GUILayout.Button("Mirror"))
		{
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pb_MirrorTool.Mirror(pb, new Vector3(
					(scaleX) ? -1f : 1f,
					(scaleY) ? -1f : 1f,
					(scaleZ) ? -1f : 1f
					));
			}
			SceneView.RepaintAll();
		}
	}

	/**
	 *	\brief Duplicates and mirrors the passed pb_Object.
	 *	@param pb The donor pb_Object.
	 *	@param axe The axis to mirror the object on.
	 *	\returns The newly duplicated pb_Object.
	 *	\sa ProBuilder.Axis
	 */
	public static pb_Object Mirror(pb_Object pb, Vector3 scale)
	{
		pb_Object p = ProBuilder.CreateObjectWithObject(pb);
		p.MakeUnique();

		p.transform.parent = pb.transform.parent;

		p.transform.position = pb.transform.position;
		p.transform.localRotation = pb.transform.localRotation;

		Vector3 lScale = p.gameObject.transform.localScale;

		p.transform.localScale = new Vector3(lScale.x * scale.x, lScale.y * scale.y, lScale.z * scale.z);

		// if flipping on an odd number of axes, flip winding order
		if( (scale.x * scale.y * scale.z) < 0)
			p.ReverseWindingOrder(p.faces);

		p.FreezeScaleTransform();
		
		p.Refresh();
		p.GenerateUV2(true);

		pb_Editor_Utility.InitObjectFlags(p, ColliderType.MeshCollider, pb.entity.entityType);
		return p;
	}

	#endif
}
