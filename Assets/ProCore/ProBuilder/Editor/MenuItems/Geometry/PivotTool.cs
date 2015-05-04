/**
 *  @ Matt1988
 *  This extension was built by @Matt1988
 */

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
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
public class PivotTool : Editor {

    [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Set Pivot _%j", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
    static void init()
    {
        pb_Editor_Utility.ShowNotification("Set Pivot", "Center pivot around current selection.");

        pb_Object[] pbObjects = pbUtil.GetComponents<pb_Object>(Selection.transforms);
        if (pbObjects.Length > 0)
        {
			pbUndo.RecordObjects(pbObjects, "Set object(s) pivot point.");

            foreach (pb_Object pbo in pbObjects)
            {
                if (pbo.SelectedTriangles.Length > 0)
                {
                    SetPivot(pbo, pbo.SelectedTriangles, false);
                }
                else
                {
                    SetPivot(pbo, pbo.uniqueIndices, true);
                }
            }
        }
        
        EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
    }

    private static void SetPivot(pb_Object pbo, int[] testIndices, bool doSnap)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 vector in pbo.VerticesInWorldSpace(testIndices))
        {
            center += vector;
        }
        center /= testIndices.Length;
            
        if(doSnap)
            center = pbUtil.SnapValue(center, Vector3.one, pbUtil.SharedSnapValue);

        Vector3 dir = (pbo.transform.position - center);

        pbo.transform.position = center;

        // the last bool param force disables snapping vertices
        pbo.TranslateVertices(pbo.uniqueIndices, dir, true);
		
		pbo.Refresh();
    }
}
}