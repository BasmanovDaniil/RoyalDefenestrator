using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorEnum;

public class pb_Preferences_Internal
{

	/**
	 * Checks if pref key exists in library, and if so return the value.  If not, return the default value.
	 */
	public static bool GetBool(string pref)
	{

		switch(pref)
		{
			case pb_Constant.pbDefaultOpenInDockableWindow:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;
		
			case pb_Constant.pbDefaultHideFaceMask:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : false;

			case pb_Constant.pbShowEditorNotifications:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbDragCheckLimit:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceGridPivot:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceVertexPivot:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceConvex:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : false;

			case pb_Constant.pbPerimeterEdgeExtrusionOnly:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;
			
			case pb_Constant.pbPerimeterEdgeBridgeOnly:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbPBOSelectionOnly:
				return EditorPrefs.HasKey(pref) ? EditorPrefs.GetBool(pref) : true;
			
			// When in doubt, say yes!
			default:
				return true;
		}
	}

	public static float GetFloat(string pref)
	{
		if( EditorPrefs.HasKey(pref) )
			return EditorPrefs.GetFloat(pref);
		else
		switch(pref)
		{
			case pb_Constant.pbVertexHandleSize:
				return .04f;
			
			default:
				return 1f;
		}
	}

	public static Color GetColor(string pref)
	{
		Color col;

		if( !pbUtil.ColorWithString( EditorPrefs.GetString(pref), out col) )
		switch(pref)
		{
			case pb_Constant.pbDefaultFaceColor:
					col = new Color(0f, .86f, 1f, .275f);
				break;
				
			case pb_Constant.pbDefaultVertexColor:
					col = Color.blue;
				break;
			
			case pb_Constant.pbDefaultSelectedVertexColor:
					col = Color.green;
				break;

			default:
				return Color.white;
		}

		return col;
	}

	public static pb_Shortcut[] GetShortcuts()
	{
		return EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ?
			pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts))
			:
			pb_Shortcut.DefaultShortcuts();													// Key not found, return the default
	}


	public static Material GetMaterial(string pref)
	{
		Material mat = null;

		switch(pref)
		{
			case pb_Constant.pbDefaultMaterial:
				if(EditorPrefs.HasKey(pref))
					mat = (Material) Resources.LoadAssetAtPath(pref, typeof(Material));
				break;

			default:
				return pb_Constant.DefaultMaterial;
		}

		if(!mat) mat = pb_Constant.DefaultMaterial;
		return mat;
	}

	/**
	 * Checks for pref key in EditorPrefs, and return stored value or the default.
	 */
	public static T GetEnum<T>(string pref)
	{
		int key = 0;

		switch(pref)
		{
			case pb_Constant.pbDefaultEditLevel:
				key = EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (EditLevel)key, typeof(T));

			case pb_Constant.pbDefaultSelectionMode:
				key = EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (SelectMode)key, typeof(T));

			case pb_Constant.pbHandleAlignment:
				key = EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (HandleAlignment)key , typeof(T));

			case pb_Constant.pbDefaultCollider:
				key = EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)ColliderType.MeshCollider;
				return (T)System.Convert.ChangeType( (ColliderType)key, typeof(T));
			
			default:
				return (T)System.Convert.ChangeType( 0, typeof(T));
		}
	}
}
