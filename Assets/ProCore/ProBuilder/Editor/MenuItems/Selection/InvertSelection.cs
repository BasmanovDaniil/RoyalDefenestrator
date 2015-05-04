// Thanks to forum member @Igmon for this feature suggestion:
// http://www.sixbysevenstudio.com/forum/viewtopic.php?f=14&t=2374&p=4351#p4351

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class InvertSelection : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Invert Face Selection %#i", false, pb_Constant.MENU_SELECTION + 0)]
		public static void InvertFaceSelection()
		{
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				List<pb_Face> unselectedFaces = new List<pb_Face>();
				foreach(pb_Face face in pb.faces)
				{
					if(!pb.SelectedFaces.Contains(face))
						unselectedFaces.Add(face);
				}

				pb.SetSelectedFaces(unselectedFaces.ToArray());
				pb_Editor.instance.UpdateSelection();
			}

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}
