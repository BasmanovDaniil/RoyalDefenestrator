#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorEnum;

public class pb_Smoothing_Editor : EditorWindow
{
	#if !PROTOTYPE
#region MEMBERS

	pb_Object[] selection;
	List<int> 	smoothGroups = new List<int>();

	const int BUTTON_WIDTH = 28;
	const int pad = 2;

	bool drawNormals = false;
	pb_Texture_Editor textureWindow;

	int oldWidth = 0, oldHeight = 0;
#endregion

#region INITIALIZATION CALLBACKS

	public static pb_Smoothing_Editor Init(pb_Texture_Editor del, pb_Object[] _selection)
	{
		pb_Smoothing_Editor pse = (pb_Smoothing_Editor)EditorWindow.GetWindow(typeof(pb_Smoothing_Editor), true, "Smoothing Groups", true);
		pse.SetDelegate(del);
		pse.UpdateSelection(_selection);
		return pse;
	}

	public void SetDelegate(pb_Texture_Editor _del)
	{
		textureWindow = _del;
	}

	public void SetDrawNormals(bool val)
	{
		if(textureWindow)
		{
			if(pb_Editor.instanceIfExists)
				pb_Editor.instanceIfExists.drawVertexNormals = val;
		}
		SceneView.RepaintAll();
	}

	public void OnEnable()
	{		
		this.autoRepaintOnSceneChange = true;
		this.minSize = new Vector2(332f, 220f);
		// this.maxSize = new Vector2(332f, 220f);
	}

	public void OnFocus()
	{
		if(pb_Editor.instanceIfExists)
			pb_Editor.instanceIfExists.SetSelectionMode(SelectMode.Face);
	}

	public void OnDisable()
	{
		SetDrawNormals(false);
	}

	public void OnWindowResize()
	{
		clearAllRect = new Rect(Screen.width-80-pad, Screen.height-20-pad, 80, 18);
		drawNormalsRect = new Rect(pad, Screen.height-18-pad, 160, 18);
	}
#endregion

#region INTERFACE
	
	Rect smoothLabelRect = new Rect(pad, pad, 200, 18);
	Rect hardLabelRect = new Rect(pad, pad, 200, 18);
	Rect clearAllRect = new Rect(0f, 0f, 0f, 0f);
	Rect drawNormalsRect = new Rect(0f, 0f, 0f, 0f);

	public void OnGUI()
	{
		if(Screen.width != oldWidth || Screen.height != oldHeight)
			OnWindowResize();

		// remove all on object
		if(GUI.Button(clearAllRect, "Clear"))
			SetSmoothingGroup(selection, 0);

		GUI.Label(smoothLabelRect, "Smooth", EditorStyles.boldLabel);

		GUI.changed = false;
		drawNormals = EditorGUI.Toggle(drawNormalsRect, "Show Normals", drawNormals);
		if(GUI.changed)
			SetDrawNormals(drawNormals);

		// smoothingGroup 0 is reserved for 'no group'
		int buttonsPerLine = Screen.width / (BUTTON_WIDTH+pad);
		int row = 0;
		Rect buttonRect = new Rect(pad, smoothLabelRect.y + smoothLabelRect.height + pad, BUTTON_WIDTH, BUTTON_WIDTH);

		// why 25 to limit groups?  because fuck you, that's why.
		for(int i = 1; i < 25; i++)
		{
			if(i - (buttonsPerLine*row) > buttonsPerLine) {
				row++;
				buttonRect = new Rect(pad, buttonRect.y + BUTTON_WIDTH + pad, BUTTON_WIDTH, BUTTON_WIDTH);
			}

			if(smoothGroups.Contains(i))
				GUI.backgroundColor = Color.green;
		
			if(GUI.Button(buttonRect, i.ToString()))
				SetSmoothingGroup(selection, i);

			GUI.backgroundColor = Color.white;

			buttonRect = new Rect(buttonRect.x + BUTTON_WIDTH + pad, buttonRect.y, BUTTON_WIDTH, BUTTON_WIDTH);
		}

		hardLabelRect = new Rect(pad, buttonRect.y + pad + BUTTON_WIDTH + 10, 200, 18);
		GUI.Label(hardLabelRect, "Hard", EditorStyles.boldLabel);
		row = 0;
		buttonRect = new Rect(pad, hardLabelRect.y + hardLabelRect.height + pad, BUTTON_WIDTH, BUTTON_WIDTH);
		for(int i = 25; i < 43; i++)
		{
			if( (i-24) - (buttonsPerLine*row) > buttonsPerLine) {
				row++;
				buttonRect = new Rect(pad, buttonRect.y + BUTTON_WIDTH + pad, BUTTON_WIDTH, BUTTON_WIDTH);
			}

			if(smoothGroups.Contains(i))
				GUI.backgroundColor = Color.green;
		
			if(GUI.Button(buttonRect, i.ToString()))
				SetSmoothingGroup(selection, i);

			GUI.backgroundColor = Color.white;

			buttonRect = new Rect(buttonRect.x + BUTTON_WIDTH + pad, buttonRect.y, BUTTON_WIDTH, BUTTON_WIDTH);
		}
	}
#endregion

#region APPLY
	
	public void SetSmoothingGroup(pb_Object[] _selection, int sg)
	{
		pbUndo.RecordObjects(_selection, "Set Smoothing Groups");

		// If all selected are of the same group, act as a toggle
		if(smoothGroups.Count == 1 && smoothGroups[0] == sg)
			sg = 0;

		foreach(pb_Object pb in _selection)
		{
			foreach(pb_Face face in pb.SelectedFaces)
				face.SetSmoothingGroup(sg);

			pb.RefreshNormals();
		}

		UpdateSelection(selection);
	}

	public void ClearAllSmoothingGroups(pb_Object[] _selection)
	{
		pbUndo.RecordObjects(_selection, "Clear Smoothing Groups");

		foreach(pb_Object pb in _selection)
		{
			foreach(pb_Face face in pb.faces)
			{
				face.SetSmoothingGroup(0);
			}
			pb.RefreshNormals();
		}

		UpdateSelection(selection);
	}
#endregion

#region SELECTION CACHE

	public void UpdateSelection(pb_Object[] _selection)
	{
		selection = _selection;
		smoothGroups.Clear();

		foreach(pb_Object pb in selection)
		{
			foreach(pb_Face face in pb.SelectedFaces)
			{
				if(!smoothGroups.Contains(face.smoothingGroup))
					smoothGroups.Add(face.smoothingGroup);
			}
		}

		Repaint();
	}
#endregion
	#endif
}