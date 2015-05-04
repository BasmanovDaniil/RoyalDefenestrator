using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

public class MaterialSelection : Editor
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select All Faces With Material", false, pb_Constant.MENU_SELECTION + 2)]
	public static void MenuSelectFacesWithMaterial()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			List<Material> mat = new List<Material>();
			foreach(pb_Face f in pb.SelectedFaces)
			{
				mat.Add(f.material);
			}

			List<pb_Face> faces = new List<pb_Face>();
			foreach(pb_Face f in pb.faces)
			{
				if(mat.Contains(f.material))
				{
					faces.Add(f);
				}
			}

			pb.SetSelectedFaces(faces.ToArray());
			pb_Editor.instance.UpdateSelection();
			
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}
