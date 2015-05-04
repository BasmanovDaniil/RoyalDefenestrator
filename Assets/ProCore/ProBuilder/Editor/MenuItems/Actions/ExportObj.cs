using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

public class ExportObj : MonoBehaviour {

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Export Selected to OBJ")]
	public static void ExportOBJ()
	{
		pb_Editor_Utility.ExportOBJ(pbUtil.GetComponents<pb_Object>(Selection.transforms)); 
	}
}
