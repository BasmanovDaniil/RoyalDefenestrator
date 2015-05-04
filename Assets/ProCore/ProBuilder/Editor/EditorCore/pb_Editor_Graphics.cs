using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.EditorEnum;
using ProBuilder2.Common;

public class pb_Editor_Graphics
{
	static float vertexHandleSize = .04f;
	const float SELECTION_MESH_OFFSET = .005f;
	
	public static GameObject 	selectionGameObject;
	static Mesh 		selectionMesh;
	static Material 	selectionMaterial;
	static Color faceSelectionColor = new Color(0f, 1f, 1f, .275f);
	static bool init = false;
	public static SelectMode _selectMode = SelectMode.Face;

	private static void Init()
	{
		if(selectionGameObject == null)
		{
			selectionGameObject = EditorUtility.CreateGameObjectWithHideFlags("ProBuilderSelectionMeshObject", HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable, new System.Type[2]{typeof(MeshFilter), typeof(MeshRenderer)});
			selectionGameObject.GetComponent<MeshRenderer>().enabled = false;
		}

		if(selectionMesh == null)
			selectionMesh = new Mesh();
		
		if(!init)
		{
			faceSelectionColor = pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultFaceColor);
			vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);
			init = true;
		}

		if(selectionMaterial == null)
		{
			SetMaterial(_selectMode);
		}
		
		selectionGameObject.GetComponent<MeshFilter>().sharedMesh = selectionMesh;
		selectionGameObject.GetComponent<MeshRenderer>().sharedMaterial = selectionMaterial;
	}

	static void SetMaterial(SelectMode sm)
	{
		switch(sm)
		{
			case SelectMode.Face:
				selectionMaterial = new Material(Shader.Find("Hidden/ProBuilder/UnlitColor"));
				break;
			case SelectMode.Vertex:
				vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);
				selectionMaterial = new Material(Shader.Find("Hidden/ProBuilder/VertexBillboard"));
				selectionMaterial.SetTexture("_MainTex", (Texture2D)Resources.Load("Textures/VertOff", typeof(Texture2D)));
				break;
		}

		selectionMaterial.SetColor("_Color", faceSelectionColor);	// todo - remove this and use vertex colors
	}

	static internal void OnDisable()
	{
		DestroyTempObjects();
	}

	private static void DestroyTempObjects()
	{
		if(selectionGameObject != null)
		{
			if(selectionMesh != null)
				GameObject.DestroyImmediate(selectionMesh);

			if(selectionMaterial != null)
				GameObject.DestroyImmediate(selectionMaterial, true);
			
			GameObject.DestroyImmediate(selectionGameObject);
		}
	}

	static internal void ClearSelectionMesh()
	{
		if(selectionMesh)
			selectionMesh.Clear();
	}

	static internal void DrawSelectionMesh()
	{
		if(selectionGameObject == null)
			Init();

		selectionMaterial.SetPass(0);
		Graphics.DrawMeshNow(selectionMesh, Vector3.zero, Quaternion.identity/*, selectionMaterial*/, 0);
	}

	public static void DrawVertexHandles(int selectionLength, int[][] indices, Vector3[][] allVerticesInWorldSpace, Color col)
	{
		Color t = Handles.color;
		Handles.color = col;
		for(int i = 0; i < selectionLength; i++)
		{
			foreach(int j in indices[i])
			{
				Vector3 pos = allVerticesInWorldSpace[i][j];
				Handles.DotCap(-1, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * vertexHandleSize);
			}
		}
		Handles.color = t;
	}

	static internal void UpdateSelectionMesh(pb_Object[] _selection, SelectMode selectionMode)
	{
		if(_selection == null || _selection.Length < 1)
			return;

		if(_selectMode != selectionMode)
		{	
			SetMaterial(selectionMode);
			_selectMode = selectionMode;
		}

		if(selectionMesh != null)
			selectionMesh.Clear();
		else
			selectionMesh = new Mesh();

		List<Vector3> verts = new List<Vector3>();
		List<Vector4> tan = new List<Vector4>();
		List<Vector2> uvs 	= new List<Vector2>();
		List<Vector2> uv2s 	= new List<Vector2>();
		List<Color> col = new List<Color>();
		List<int> tris = new List<int>();

		switch( selectionMode )
		{
			case SelectMode.Edge:

				List<Vector3> ve = new List<Vector3>();
				foreach(pb_Object pb in _selection)
				{
					Vector3[] pbverts = pb.vertices;
					pb_IntArray[] sharedIndices = pb.sharedIndices;

					List<pb_Edge> universalEdges = new List<pb_Edge>(pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), sharedIndices));
					
					foreach(pb_Edge e in universalEdges)
					{
						ve.Add( pb.transform.TransformPoint(pbverts[sharedIndices[e.x][0]]) );
						ve.Add( pb.transform.TransformPoint(pbverts[sharedIndices[e.y][0]]) );
					}

					verts.AddRange(ve);
				}

				for(int i = 0; i < verts.Count; i+=2)
				{
					tris.Add(i);
					tris.Add(i+1);
					uvs.Add(Vector2.zero);
					uvs.Add(Vector2.zero);
				}
				break;

			case SelectMode.Vertex:

				int vcount = 0;
				foreach(pb_Object pb in _selection)
				{
					Vector3[] v = new Vector3[pb.uniqueIndices.Length];
					for(int i = 0; i < v.Length; i++)	
						v[i] = pb.vertices[pb.uniqueIndices[i]];

					int[] sel = pb.SelectedTriangles;

					Vector3[] 	t_billboards 		= new Vector3[v.Length*4];
					Vector3[] 	t_nrm 				= new Vector3[v.Length*4];
					Vector2[] 	t_uvs 				= new Vector2[v.Length*4];
					Vector2[] 	t_uv2 				= new Vector2[v.Length*4];
					Vector4[] 	t_tan 				= new Vector4[v.Length*4];
					Color[]   	t_col 				= new Color[v.Length*4];
					int[] 		t_tris 				= new int[v.Length*6];

					int n = 0;
					int t = 0;

					Vector3 up = Vector3.up;// * .1f;
					Vector3 right = Vector3.right;// * .1f;

					for(int i = 0; i < v.Length; i++)
					{
						Vector3 vpoint = pb.transform.TransformPoint(v[i]);
						float handleSize = HandleUtility.GetHandleSize(vpoint) * .04f;
						
						t_billboards[t+0] = vpoint;//-up-right;
						t_billboards[t+1] = vpoint;//-up+right;
						t_billboards[t+2] = vpoint;//+up-right;
						t_billboards[t+3] = vpoint;//+up+right;

						t_uvs[t+0] = Vector3.zero;
						t_uvs[t+1] = Vector3.right;
						t_uvs[t+2] = Vector3.up;
						t_uvs[t+3] = Vector3.one;

						t_tan[t+0] = new Vector4(handleSize, 0f, 0f, 0f);
						t_tan[t+1] = new Vector4(handleSize, 0f, 0f, 0f);
						t_tan[t+2] = new Vector4(handleSize, 0f, 0f, 0f);
						t_tan[t+3] = new Vector4(handleSize, 0f, 0f, 0f);
	
						t_uv2[t+0] = -up-right;
						t_uv2[t+1] = -up+right;
						t_uv2[t+2] =  up-right;
						t_uv2[t+3] =  up+right;
	
						t_nrm[t+0] = Vector3.forward;
						t_nrm[t+1] = Vector3.forward;
						t_nrm[t+2] = Vector3.forward;
						t_nrm[t+3] = Vector3.forward;

						t_tris[n+0] = t+2+vcount;
						t_tris[n+1] = t+1+vcount;
						t_tris[n+2] = t+0+vcount;
						t_tris[n+3] = t+2+vcount;
						t_tris[n+4] = t+3+vcount;
						t_tris[n+5] = t+1+vcount;

						if(System.Array.IndexOf(sel, pb.uniqueIndices[i]) > -1)
						{
							t_col[t+0] = Color.green;
							t_col[t+1] = Color.green;
							t_col[t+2] = Color.green;
							t_col[t+3] = Color.green;
						}
						else
						{
							t_col[t+0] = faceSelectionColor;
							t_col[t+1] = faceSelectionColor;
							t_col[t+2] = faceSelectionColor;
							t_col[t+3] = faceSelectionColor;
						}

						t+=4;
						n+=6;				
					}

					verts.AddRange(t_billboards);
					vcount += t_billboards.Length;
					uvs.AddRange(t_uvs);
					uv2s.AddRange(t_uv2);
					tan.AddRange(t_tan);
					col.AddRange(t_col);
					tris.AddRange(t_tris);
				}

				break;

			default:
			case SelectMode.Face:

				foreach(pb_Object pb in _selection)			
				{
					Mesh mersh = pb.msh;

					int[] selectedTriangles = pb_Face.AllTriangles(pb.SelectedFaces);

					Vector3[] 	v = pb.VerticesInWorldSpace(selectedTriangles);
					Vector2[] 	u = pbUtil.ValuesWithIndices(mersh.uv, selectedTriangles);

					verts.AddRange(v);
					uvs.AddRange  (u);
				}

				tris = new List<int>(verts.Count);			// because ValuesWithIndices returns in wound order, just fill
				tan = new List<Vector4>(verts.Count);
				for(int p = 0; p < verts.Count; p++)		// triangles with 0, 1, 2, 3, etc
				{
					tan.Add(Vector4.one);
					tris.Add(p);
				}
				
				break;
		}

		if(selectionMesh == null) // todo- remove this turd hax
			return;

		selectionMesh.vertices = verts.ToArray();	// it is assigned here because we need to get normals
		selectionMesh.uv = uvs.ToArray();
		selectionMesh.uv2 = uv2s.ToArray();
		selectionMesh.tangents = tan.ToArray();
		selectionMesh.colors = col.ToArray();

		switch(selectionMode)
		{
			#if UNITY_4
			case SelectMode.Edge:
				selectionMesh.subMeshCount = 1;
				selectionMesh.SetIndices(tris, MeshTopology.Lines, 0);				
				break;
			#endif

			default:
				selectionMesh.triangles = tris.ToArray();
				break;
		}

		if(selectionMode == SelectMode.Face)
		{
			selectionMesh.RecalculateNormals();
			Vector3[] nrmls = selectionMesh.normals;
			for(int i = 0; i < verts.Count; i++)
				verts[i] += SELECTION_MESH_OFFSET * nrmls[i].normalized;
			selectionMesh.vertices = verts.ToArray();
		}
	}
}