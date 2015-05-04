// #define FREE

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

/**
 *	\internal -- todo Implement a 'preview' mesh mode, allowing pb to skip some of the lengthy pb-specific calculations and making preview supa fast.
 */
public class pb_Geometry_Interface : EditorWindow
{	
	static Color COLOR_GREEN = new Color(0f, .8f, 0f, .8f);
	static Color PREVIEW_COLOR = new Color(.5f, .9f, 1f, .56f);
	public Shape shape = Shape.Cube;

	private pb_Object previewObject;
	private bool showPreview = true;
	private Material _prevMat;

	public Material previewMat
	{
		get
		{
			if(_prevMat == null)
			{
				_prevMat = new Material(Shader.Find("Diffuse"));
				// _prevMat = new Material(Shader.Find("Hidden/ProBuilder/UnlitColor"));
				_prevMat.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");
				_prevMat.SetColor("_Color", PREVIEW_COLOR);
			}
			return _prevMat;
		}
	}
	private bool initPreview = false; // used to toggle preview on and off from class OnGUI

	Material userMaterial = null;
	public void OnEnable()
	{
		#if !PROTOTYPE
			userMaterial = pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);
		#endif

		initPreview = true;
	}

	public void OnDisable()
	{
		DestroyPreviewObject();
	}


	[MenuItem("GameObject/Create Other/" + pb_Constant.PRODUCT_NAME + " Cube _%k")]
	public static void MenuCreateCube()
	{
		pb_Object pb = ProBuilder.CreatePrimitive(Shape.Cube);
		
		#if !PROTOTYPE
		Material mat = null;
		if(EditorPrefs.HasKey(pb_Constant.pbDefaultMaterial))
			mat = (Material)Resources.LoadAssetAtPath(EditorPrefs.GetString(pb_Constant.pbDefaultMaterial), typeof(Material));

		if(mat != null) pb.SetFaceMaterial(pb.faces, mat);
		#endif

		pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
		pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
	}

	Vector2 scroll = Vector2.zero;
	bool doGenerateShape = false;	// sooo lazy :\
	public void OnGUI()
	{	
		GUILayout.BeginHorizontal();
			bool sp = showPreview;
			showPreview = GUILayout.Toggle(showPreview, "Show Preview");
			if(sp != showPreview && !showPreview) DestroyPreviewObject();

			if(GUILayout.Button("Center Preview"))
			{
				if(previewObject == null) return;

				pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
				Selection.activeTransform = previewObject.transform;
				Selection.activeObject = previewObject;
				RegisterPreviewObjectTransform();
			}
		GUILayout.EndHorizontal();

		GUILayout.Space(7);

		GUILayout.Label("Shape Selector", EditorStyles.boldLabel);
		
		Shape oldShape = shape;
		shape = (Shape)EditorGUILayout.EnumPopup(shape);
			
		// GUILayout.Space(14);

		// GUI.Box(new Rect(6, dip, Screen.width-12, Screen.height-dip-6), "");

		if(shape != oldShape) initPreview = true;

		scroll = EditorGUILayout.BeginScrollView(scroll);
		switch(shape)
		{
			case Shape.Cube:
				CubeGUI(doGenerateShape);
				break;
			case Shape.Prism:
				PrismGUI(doGenerateShape);
				break;
			case Shape.Stair:
				StairGUI(doGenerateShape);
				break;
			case Shape.Cylinder:
				CylinderGUI(doGenerateShape);
				break;
			case Shape.Plane:
				PlaneGUI(doGenerateShape);
				break;
			case Shape.Door:
				DoorGUI(doGenerateShape);
				break;
			case Shape.Pipe:
				PipeGUI(doGenerateShape);
				break;
			case Shape.Cone:
				ConeGUI(doGenerateShape);
				break;
			case Shape.Sprite:
				SpriteGUI(doGenerateShape);
				break;
			case Shape.Arch:
				ArchGUI(doGenerateShape);
				break;
			case Shape.Custom:
				CustomGUI(doGenerateShape);
				break;
		}
		EditorGUILayout.EndScrollView();

		Color oldColor = GUI.backgroundColor;
		GUI.backgroundColor = COLOR_GREEN;
		if( GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)) )
			doGenerateShape = true;
		else
			doGenerateShape = false;
		GUI.backgroundColor = oldColor;
	}

	/**
	 *	\brief Creates a cube.
	 *	\returns The cube.
	 */
	static Vector3 cubeSize = Vector3.one;
	public void CubeGUI(bool doGenShape)
	{
		cubeSize = EditorGUILayout.Vector3Field("Dimensions", cubeSize);
		
		if(cubeSize.x <= 0) cubeSize.x = .01f;
		if(cubeSize.y <= 0) cubeSize.y = .01f;
		if(cubeSize.z <= 0) cubeSize.z = .01f;

		if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape_Generator.CubeGenerator(cubeSize));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.CubeGenerator(cubeSize);
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}
	
	/**
	 *	\brief Creates a sprite.
	 *	\returns The sprite.
	 */
	public void SpriteGUI(bool doGenShape)
	{
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Facing Direction");
		plane_axis = (Axis)EditorGUILayout.EnumPopup(plane_axis);
		GUILayout.EndHorizontal();

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape_Generator.PlaneGenerator(
				 	1,
				 	1,
				 	0,
				 	0,
				 	plane_axis,
				 	false));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.PlaneGenerator(
				 	1,
				 	1,
				 	0,
				 	0,
				 	plane_axis,
				 	false);
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
			
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}

	/**
	 *	\brief Creates a prism.
	 *	...that's it.
	 *	\returns The prism.
	 */
	static Vector3 prismSize = Vector3.one;
	public void PrismGUI(bool doGenShape)
	{
		prismSize = EditorGUILayout.Vector3Field("Dimensions", prismSize);
		
		if(prismSize.x < 0) prismSize.x = 0.01f;
		if(prismSize.y < 0) prismSize.y = 0.01f;
		if(prismSize.z < 0) prismSize.z = 0.01f;

		if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape_Generator.PrismGenerator(prismSize));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.PrismGenerator(prismSize);
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
	
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}

	/**** Stair Generator ***/
	static bool extendSidesToFloor = true;
	static bool generateBack = true;
	static int stair_steps = 6;
	static float stair_width = 4f, stair_height = 5f, stair_depth = 8f;
	static bool stair_platformsOnly = false;
	public void StairGUI(bool doGenShape)
	{
		stair_steps = EditorGUILayout.IntField("Steps", stair_steps);
		stair_steps = Clamp(stair_steps, 2, 50);

		stair_width = EditorGUILayout.FloatField("Width", stair_width);
		stair_width = Mathf.Clamp(stair_width, 0.01f, 500f);

		stair_height = EditorGUILayout.FloatField("Height", stair_height);
		stair_height = Mathf.Clamp(stair_height, .01f, 500f);

		stair_depth = EditorGUILayout.FloatField("Depth", stair_depth);
		stair_depth = Mathf.Clamp(stair_depth, .01f, 500f);

		stair_platformsOnly = EditorGUILayout.Toggle("Platforms Only", stair_platformsOnly);
		if(stair_platformsOnly) { GUI.enabled = false; extendSidesToFloor = false; generateBack = false; }
		extendSidesToFloor = EditorGUILayout.Toggle("Extend sides to floor", extendSidesToFloor);
		generateBack = EditorGUILayout.Toggle("Generate Back", generateBack);
		GUI.enabled = true;

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(pb_Shape_Generator.StairGenerator(
				stair_steps, 
				stair_width,
				stair_height,
				stair_depth,
				extendSidesToFloor,
				generateBack,
				stair_platformsOnly));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.StairGenerator(stair_steps, stair_width, stair_height, stair_depth, extendSidesToFloor, generateBack, stair_platformsOnly);

			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
			
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;		
		}
	}

	/**** Cylinder Generator ***/
	static int cyl_axisCuts = 6;
	static float cyl_radius = 1.5f;
	static float cyl_height = 4f;
	static int cyl_heightCuts = 2;
	public void CylinderGUI(bool doGenShape)
	{
		#if FREE || TORNADO_TWINS
		GUI.enabled = false;
		#endif

		// Store old values	
		cyl_radius = EditorGUILayout.FloatField("Radius", cyl_radius);
		cyl_radius = Mathf.Clamp(cyl_radius, .01f, Mathf.Infinity);

		cyl_axisCuts = EditorGUILayout.IntField("Axis Divisions", cyl_axisCuts);
		cyl_axisCuts = Clamp(cyl_axisCuts, 2, 48);

		cyl_height = EditorGUILayout.FloatField("Height", cyl_height);

		cyl_heightCuts = EditorGUILayout.IntField("Height Cuts", cyl_heightCuts);
		cyl_heightCuts = Clamp(cyl_heightCuts, 0, 48);

		if(cyl_axisCuts % 2 != 0)
			cyl_axisCuts++;

		if(cyl_heightCuts < 0)
			cyl_heightCuts = 0;

		if( showPreview && (GUI.changed || initPreview) ) 
		{
			SetPreviewObject(
				pb_Shape_Generator.CylinderGenerator(
				cyl_axisCuts,
				cyl_radius,
				cyl_height,
				cyl_heightCuts),
				new int[1] { (cyl_axisCuts*(cyl_heightCuts+1)*4)+1 } );
		}

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.CylinderGenerator(cyl_axisCuts, cyl_radius, cyl_height, cyl_heightCuts);
			
			int centerIndex = (cyl_axisCuts*(cyl_heightCuts+1)*4)+1;
			
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, new int[1] {centerIndex});

			AlignWithPreviewObject(pb.gameObject);
			
			DestroyPreviewObject();
			showPreview = false;			
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	/**** Door Generator ***/
	static float door_totalWidth  = 4.0f;
	static float door_totalHeight = 4.0f;
	static float door_ledgeHeight = 1.0f;
	static float door_legWidth	  = 1.0f;
	static float door_depth		  = 0.5f;
	public void DoorGUI(bool doGenShape)
	{

		door_totalWidth = EditorGUILayout.FloatField("Total Width", door_totalWidth);
		door_totalWidth = Mathf.Clamp(door_totalWidth, 1.0f, 500.0f);

		door_totalHeight = EditorGUILayout.FloatField("Total Height", door_totalHeight);
		door_totalHeight = Mathf.Clamp(door_totalHeight, 1.0f, 500.0f);

		door_ledgeHeight = EditorGUILayout.FloatField("Ledge Height", door_ledgeHeight);
		door_ledgeHeight = Mathf.Clamp(door_ledgeHeight, 0.01f, 500.0f);

		door_legWidth = EditorGUILayout.FloatField("Leg Width", door_legWidth);
		door_legWidth = Mathf.Clamp(door_legWidth, 0.01f, 2.0f);

		door_depth = EditorGUILayout.FloatField("Door Depth", door_depth);
		door_depth = Mathf.Clamp(door_depth, 0.01f, 500.0f);

		if (showPreview && (GUI.changed || initPreview))
			SetPreviewObject(pb_Shape_Generator.DoorGenerator(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.DoorGenerator(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth);
			 
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;			
		}
	}

	static float plane_width = 10, plane_height = 10;
	static int plane_widthCuts = 3, plane_heightCuts = 3;
	static Axis plane_axis = Axis.Up;
	static bool plane_smooth = false;
	public void PlaneGUI(bool doGenShape)
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif

		plane_axis = (Axis)EditorGUILayout.EnumPopup("Normal Axis", plane_axis);

		plane_width = EditorGUILayout.FloatField("Width", plane_width);
		plane_height = EditorGUILayout.FloatField("Height", plane_height);

		if(plane_width < 1f)
			plane_width = 1f;

		if(plane_height < 1f)
			plane_height = 1f;

		plane_widthCuts = EditorGUILayout.IntField("Cuts Width", plane_widthCuts);
		
		if(plane_widthCuts < 0)
			plane_widthCuts = 0;

		plane_heightCuts = EditorGUILayout.IntField("Cuts Height", plane_heightCuts);
		
		if(plane_heightCuts < 0)
			plane_heightCuts = 0;

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape_Generator.PlaneGenerator(
				 	plane_width,
				 	plane_height,
				 	plane_widthCuts,
				 	plane_heightCuts,
				 	plane_axis,
				 	plane_smooth));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.PlaneGenerator(plane_width, plane_height, plane_widthCuts, plane_heightCuts, plane_axis, plane_smooth);
			
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
			
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static float pipe_radius = 1f;
	static float pipe_height = 2f;
	static float pipe_thickness = .2f;
	static int pipe_subdivAxis = 6;
	static int pipe_subdivHeight = 1;
	void PipeGUI(bool doGenShape)
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif
		pipe_radius = EditorGUILayout.FloatField("Radius", pipe_radius);
		pipe_height = EditorGUILayout.FloatField("Height", pipe_height);
		pipe_thickness = EditorGUILayout.FloatField("Thickness", pipe_thickness);
		pipe_subdivAxis = EditorGUILayout.IntField("Subdivisions Axis", pipe_subdivAxis);
		pipe_subdivHeight = EditorGUILayout.IntField("Subdivisions Height", pipe_subdivHeight);
		
		if(pipe_radius < .1f)
			pipe_radius = .1f;

		if(pipe_height < .1f)
			pipe_height = .1f;

		pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 0f, 32f);
		pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, pipe_radius-.01f);
		pipe_subdivAxis = (int)Mathf.Clamp(pipe_subdivAxis, 3f, 32f);		

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape_Generator.PipeGenerator(	
				 	pipe_radius,
					pipe_height,
					pipe_thickness,
					pipe_subdivAxis,
					pipe_subdivHeight
				 	));	 	

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.PipeGenerator(	
				 	pipe_radius,
					pipe_height,
					pipe_thickness,
					pipe_subdivAxis,
					pipe_subdivHeight
				 	);

			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	static float 	cone_radius = 1f;
	static float 	cone_height = 2f;
	static int 		cone_subdivAxis = 6;
	void ConeGUI(bool doGenShape)
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif
		cone_radius = EditorGUILayout.FloatField("Radius", cone_radius);
		cone_height = EditorGUILayout.FloatField("Height", cone_height);
		cone_subdivAxis = EditorGUILayout.IntField("Subdivisions Axis", cone_subdivAxis);
		
		if(cone_radius < .1f)
			cone_radius = .1f;

		if(cone_height < .1f)
			cone_height = .1f;

		pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 1f, 32f);
		pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, cone_radius-.01f);
		cone_subdivAxis = (int)Mathf.Clamp(cone_subdivAxis, 3f, 32f);		

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(
				 pb_Shape_Generator.ConeGenerator(	
				 	cone_radius,
					cone_height,
					cone_subdivAxis
				 	));	 	

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.ConeGenerator(	
				 	cone_radius,
					cone_height,
					cone_subdivAxis
				 	);

			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	/**** Arch Generator ***/
	static float arch_angle		= 180.0f;
	static float arch_radius	= 4.0f;
	static float arch_width		= 0.50f;
	static float arch_depth		= 0.5f;
	static int arch_radialCuts	= 8;
	static bool arch_insideFaces = true;
	static bool arch_outsideFaces = true;
	static bool arch_frontFaces = true;
	static bool arch_backFaces = true;
	public void ArchGUI(bool doGenShape)
	{

		arch_radius = EditorGUILayout.FloatField("Radius", arch_radius);
		arch_radius = arch_radius <= 0f ? .01f : arch_radius;

		arch_width = EditorGUILayout.FloatField("Width", arch_width);
		arch_width = Mathf.Clamp(arch_width, 0.01f, arch_radius);

		arch_depth = EditorGUILayout.FloatField("Depth", arch_depth);
		arch_depth = Mathf.Clamp(arch_depth, 0.1f, 500.0f);

		arch_radialCuts = EditorGUILayout.IntField("Radial Cuts", arch_radialCuts);
		arch_radialCuts = Mathf.Clamp(arch_radialCuts, 3, 200);

		arch_angle = EditorGUILayout.FloatField("Arch", arch_angle);
		arch_angle = Mathf.Clamp(arch_angle, 0.0f, 360.0f);

		arch_insideFaces = EditorGUILayout.Toggle("Inside Faces", arch_insideFaces);

		arch_outsideFaces = EditorGUILayout.Toggle("Outside Faces", arch_outsideFaces);

		arch_frontFaces = EditorGUILayout.Toggle("Front Faces", arch_frontFaces);

		arch_backFaces = EditorGUILayout.Toggle("Back Faces", arch_backFaces);

	  	if (showPreview && (GUI.changed || initPreview))
			SetPreviewObject(pb_Shape_Generator.ArchGenerator(arch_angle, arch_radius, arch_width, arch_depth, arch_radialCuts, arch_insideFaces, arch_outsideFaces, arch_frontFaces, arch_backFaces));

		if(doGenShape)
		{
			pb_Object pb = pb_Shape_Generator.ArchGenerator(arch_angle, arch_radius, arch_width, arch_depth, arch_radialCuts, arch_insideFaces, arch_outsideFaces, arch_frontFaces, arch_backFaces);

			if (userMaterial) pb.SetFaceMaterial(pb.faces,userMaterial);

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}
	}

	static string verts = "//Vertical Plane\n0, 0, 0\n1, 0, 0\n0, 1, 0\n1, 1, 0\n";
	static Vector2 scrollbar = new Vector2(0f, 0f);
	public void CustomGUI(bool doGenShape)
	{
		#if FREE || TORNADO_TWINS
			GUI.enabled = false;
		#endif

		GUILayout.Label("Custom Geometry", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Vertices must be wound in faces, and counter-clockwise.\n(Think horizontally reversed Z)", MessageType.Info);
			
		scrollbar = GUILayout.BeginScrollView(scrollbar);
			verts = EditorGUILayout.TextArea(verts, GUILayout.MinHeight(160));
		GUILayout.EndScrollView();

		if( showPreview && (GUI.changed || initPreview) ) 
			SetPreviewObject(ProBuilder.CreateObjectWithPoints(pbUtil.StringToVector3Array(verts)));

		if(doGenShape)
		{
			if(verts.Length > 256)
				Debug.Log("Whoa!  Did you seriously type all those points!?");
			pb_Object pb = ProBuilder.CreateObjectWithPoints(pbUtil.StringToVector3Array(verts));
			
			if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);

			AlignWithPreviewObject(pb.gameObject);
			DestroyPreviewObject();
			showPreview = false;
		}

		#if FREE || TORNADO_TWINS
			GUI.enabled = true;
		#endif
	}

	private int Clamp(int val, int min, int max)
	{
		if(val > max) val = max;
		if(val < min) val = min;
		return val;
	}

#region PREVIEW OBJECT

	public void DestroyPreviewObject()
	{
		if(previewObject != null) GameObject.DestroyImmediate(previewObject.gameObject);
		if(_prevMat != null) DestroyImmediate(_prevMat);
	}

	private void SetPreviewObject(pb_Object pb)
	{
		SetPreviewObject(pb, null);
	}

	private void SetPreviewObject(pb_Object pb, int[] indicesToCenterPivotOn)
	{
		pb.isSelectable = false;

		initPreview = false;
		bool prevTransform = false;

		if(previewObject != null)
		{
			prevTransform = true;
			RegisterPreviewObjectTransform();
		}
		
		DestroyPreviewObject();

		previewObject = pb;
		previewObject.SetName("Preview");
		previewObject.SetFaceMaterial(previewObject.faces, previewMat);

		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot))
			previewObject.CenterPivot(indicesToCenterPivotOn == null ? new int[1]{0} : indicesToCenterPivotOn);

		if(prevTransform)
		{
			previewObject.transform.position = m_pos;
			previewObject.transform.rotation = m_rot;
			previewObject.transform.localScale = m_scale;
		}
		else
		{
			pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
		}

		if(pbUtil.SharedSnapEnabled)
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, pbUtil.SharedSnapValue);
		else
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);
			
		Selection.activeTransform = pb.transform;
	}

	Vector3 m_pos = Vector3.zero;
	Quaternion m_rot = Quaternion.identity;
	Vector3 m_scale = Vector3.zero;
	private void RegisterPreviewObjectTransform()
	{
		m_pos 	= previewObject.transform.position;
		m_rot 	= previewObject.transform.rotation;
		m_scale = previewObject.transform.localScale;
	}	

	private bool PreviewObjectHasMoved()
	{
		if(m_pos != previewObject.transform.position)
			return true;
		if(m_rot != previewObject.transform.rotation)
			return true;
		if(m_scale != previewObject.transform.localScale)
			return true;	
		return false;
	}

	private void AlignWithPreviewObject(GameObject go)
	{
		if(go == null || previewObject == null) return;
		go.transform.position 	= previewObject.transform.position;
		go.transform.rotation 	= previewObject.transform.rotation;
		go.transform.localScale = previewObject.transform.localScale;
		go.GetComponent<pb_Object>().FreezeScaleTransform();
	}
#endregion
}
