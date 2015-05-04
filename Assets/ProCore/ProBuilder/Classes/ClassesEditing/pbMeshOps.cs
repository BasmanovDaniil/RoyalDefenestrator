// #undef PROFILE_TIMES

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;


namespace ProBuilder2.MeshOperations
{
	public static class pbMeshOps
	{
#region Pivot Operations (Center, Freeze Transform)

	/**
	 * Center the mesh pivot at the average of passed indices.
	 */
	public static void CenterPivot(this pb_Object pb, int[] indices)
	{	
		Vector3[] verts = pb.VerticesInWorldSpace(indices == null ? pb.uniqueIndices : indices);

		Vector3 center = Vector3.zero;
		foreach (Vector3 v in verts)
			center += v;
	
		center /= (float)verts.Length;

		// if(pbUtil.SharedSnapEnabled)
		// 	center = pbUtil.SnapValue(center, pbUtil.SharedSnapValue);

		Vector3 dir = (pb.transform.position - center);

		pb.transform.position = center;

		// the last bool param force disables snapping vertices
		pb.TranslateVertices(pb.uniqueIndices, dir, true);

		pb.Refresh();
	}

	/**
	 *	\brief Scale vertices and set transform.localScale to Vector3.one.
	 */
	public static void FreezeScaleTransform(this pb_Object pb)
	{
		Vector3[] v = pb.vertices;
		for(int i = 0; i < v.Length; i++)
			v[i] = Vector3.Scale(v[i], pb.transform.localScale);

		pb.SetVertices(v);
		pb.ToMesh();
		pb.transform.localScale = new Vector3(1f, 1f, 1f);
		pb.Refresh();
	}
#endregion

#region Extrusion

	const float EXTRUDE_DISTANCE = .25f;
	public static void Extrude(this pb_Object pb, pb_Face[] faces)
	{
		pb.Extrude(faces, EXTRUDE_DISTANCE);
	}

	public static void Extrude(this pb_Object pb, pb_Face[] faces, float extrudeDistance)
	{
		if(faces == null || faces.Length < 1)
			return;

		pb_IntArray[] sharedIndices = pb.GetSharedIndices();

		Vector3[] localVerts = pb.vertices;
		Vector3[] oNormals = pb.msh.normals;

		pb_Edge[] perimeterEdges = pb.GetPerimeterEdges(faces);

		if(perimeterEdges == null || perimeterEdges.Length < 3)
		{
			Debug.LogWarning("No perimeter edges found.  Try deselecting and reselecting this object and trying again.");
			return;
		}

		pb_Face[] edgeFaces = new pb_Face[perimeterEdges.Length];	// can't assume faces and perimiter edges will be 1:1 - so calculate perimeters then extrace face information
		int[] allEdgeIndices = new int[perimeterEdges.Length * 2];
		int c = 0;
		for(int i = 0; i < perimeterEdges.Length; i++)
		{
			// wtf does this do
			edgeFaces[i] = faces[0];
			foreach(pb_Face face in faces)
				if(face.edges.Contains(perimeterEdges[i]))
					edgeFaces[i] = face;

			allEdgeIndices[c++] = perimeterEdges[i].x;
			allEdgeIndices[c++] = perimeterEdges[i].y;
		}

		List<pb_Edge> extrudedIndices = new List<pb_Edge>();

		/// build out new faces around edges
		for(int i = 0; i < perimeterEdges.Length; i++)
		{
			pb_Edge edge = perimeterEdges[i];
			pb_Face face = edgeFaces[i];

			// Averages the normals using only vertices that are on the edge
			Vector3 xnorm = Vector3.zero;
			Vector3 ynorm = Vector3.zero;

			// don't bother getting vertex normals if not auto-extruding
			if(extrudeDistance > Mathf.Epsilon)
			{
				xnorm = Norm( edge.x, sharedIndices, allEdgeIndices, oNormals );
				ynorm = Norm( edge.y, sharedIndices, allEdgeIndices, oNormals );
			}

			int x_sharedIndex = sharedIndices.IndexOf(edge.x);
			int y_sharedIndex = sharedIndices.IndexOf(edge.y);

			pb_Face newFace = pb.AppendFace(
				new Vector3[4]
				{
					localVerts [ edge.x ],
					localVerts [ edge.y ],
					localVerts [ edge.x ] + xnorm.normalized * extrudeDistance,
					localVerts [ edge.y ] + ynorm.normalized * extrudeDistance
				},
				new pb_Face( 
					new int[6] {0, 1, 2, 1, 3, 2 },			// indices
					face.material,							// material
					new pb_UV(face.uv),						// UV material
					face.smoothingGroup,					// smoothing group
					-1,										// texture group
					-1,										// uv element group
					face.colors[0] ),						// colors
				new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

			extrudedIndices.Add(new pb_Edge(x_sharedIndex, newFace.indices[2]));
			extrudedIndices.Add(new pb_Edge(y_sharedIndex, newFace.indices[4]));
		}

		// merge extruded vertex indices with each other
		pb_IntArray[] si = pb.sharedIndices;	// leave the sharedIndices copy alone since we need the un-altered version later
		for(int i = 0; i < extrudedIndices.Count; i++)
		{
			int val = extrudedIndices[i].x;
			for(int n = 0; n < extrudedIndices.Count; n++)
			{
				if(n == i)
					continue;

				if(extrudedIndices[n].x == val)
				{
					pb_IntArrayUtility.MergeSharedIndices(ref si, extrudedIndices[n].y, extrudedIndices[i].y);
					break;
				}
			}
		}

		// Move extruded faces to top
		localVerts = pb.vertices;
		Dictionary<int, int> remappedTexGroups = new Dictionary<int, int>();
		foreach(pb_Face f in faces)
		{
			// Remap texture groups
			if(f.textureGroup > 0)
			{
				if(remappedTexGroups.ContainsKey(f.textureGroup))
				{
					f.textureGroup = remappedTexGroups[f.textureGroup];
				}
				else
				{
					int newTexGroup = pb.UnusedTextureGroup();
					remappedTexGroups.Add(f.textureGroup, newTexGroup);
					f.textureGroup = newTexGroup;
				}
			}

			int[] distinctIndices = f.distinctIndices;

			foreach(int ind in distinctIndices)
			{
				int oldIndex = si.IndexOf(ind);
				for(int i = 0; i < extrudedIndices.Count; i++)
				{
					if(oldIndex == extrudedIndices[i].x)
					{
						pb_IntArrayUtility.MergeSharedIndices(ref si, extrudedIndices[i].y, ind);
						break;
					}
				}
			}
		}

		// this is a separate loop cause the one above this must completely merge all sharedindices prior to 
		// checking the normal averages
		foreach(pb_Face f in faces)
		{
			foreach(int ind in f.distinctIndices)
			{
				Vector3 norm = Norm( ind, si, allEdgeIndices, oNormals );
				localVerts[ind] += norm.normalized * extrudeDistance;
			}
		}

		pb.SetSharedIndices(si);
		pb.SetVertices(localVerts);
		pb.RebuildFaceCaches();
	}

	/**
	 *	\brief Averages shared normals with the mask of 'all' (indices contained in perimeter edge)
	 */
	private static Vector3 Norm( int tri, pb_IntArray[] shared, int[] all, Vector3[] norm )
	{
		int sind = shared.IndexOf(tri);
		
		if(sind < 0)
			return Vector3.zero;

		int[] triGroup = shared[sind];

		Vector3 n = Vector3.zero;
		int count = 0;
		for(int i = 0; i < all.Length; i++)
		{
			// this is a point in the perimeter, add it to the average
			if( System.Array.IndexOf(triGroup, all[i]) > -1 )
			{
				n += norm[all[i]];
				count++;
			}
		}
		return n / (float)count;
	}

	/**
	 *	Edge extrusion override
	 */
	public static pb_Edge[] Extrude(this pb_Object pb, pb_Edge[] edges, float extrudeDistance, bool enforcePerimiterEdgesOnly)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		List<pb_Edge> validEdges = new List<pb_Edge>();
		List<pb_Face> edgeFaces = new List<pb_Face>();
		
		foreach(pb_Edge e in edges)
		{
			int faceCount = 0;
			pb_Face fa = null;
			foreach(pb_Face f in pb.faces)
			{
				if(f.edges.IndexOf(e, sharedIndices) > -1)
				{
					fa = f;
					if(++faceCount > 1)
						break;
				}

			}

			if(!enforcePerimiterEdgesOnly || faceCount < 2)
			{
				validEdges.Add(e);
				edgeFaces.Add(fa);
			}
		}

		if(validEdges.Count < 1)
			return null;

		Vector3[] localVerts = pb.vertices;
		Vector3[] oNormals = pb.msh.normals;

		int[] allEdgeIndices = new int[validEdges.Count * 2];
		int c = 0;	// har har har
		for(int i = 0; i < validEdges.Count; i++)
		{
			allEdgeIndices[c++] = validEdges[i].x;
			allEdgeIndices[c++] = validEdges[i].y;
		}

		List<pb_Edge> extrudedIndices = new List<pb_Edge>();
		List<pb_Edge> newEdges = new List<pb_Edge>();		// used to set the editor selection to the newly created edges

		/// build out new faces around validEdges

		for(int i = 0; i < validEdges.Count; i++)
		{
			pb_Edge edge = validEdges[i];
			pb_Face face = edgeFaces[i];

			// Averages the normals using only vertices that are on the edge
			Vector3 xnorm = Norm( edge.x, sharedIndices, allEdgeIndices, oNormals );
			Vector3 ynorm = Norm( edge.y, sharedIndices, allEdgeIndices, oNormals );

			int x_sharedIndex = sharedIndices.IndexOf(edge.x);
			int y_sharedIndex = sharedIndices.IndexOf(edge.y);

			pb_Face newFace = pb.AppendFace(
				new Vector3[4]
				{
					localVerts [ edge.x ],
					localVerts [ edge.y ],
					localVerts [ edge.x ] + xnorm.normalized * extrudeDistance,
					localVerts [ edge.y ] + ynorm.normalized * extrudeDistance
				},
				new pb_Face( new int[6] {2, 1, 0, 2, 3, 1 }, face.material, new pb_UV(), 0, -1, -1, face.colors[0] ),
				new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

			newEdges.Add(new pb_Edge(newFace.indices[3], newFace.indices[4]));

			extrudedIndices.Add(new pb_Edge(x_sharedIndex, newFace.indices[3]));
			extrudedIndices.Add(new pb_Edge(y_sharedIndex, newFace.indices[4]));
		}

		sharedIndices = pb.sharedIndices;

		// merge extruded vertex indices with each other
		for(int i = 0; i < extrudedIndices.Count; i++)
		{
			int val = extrudedIndices[i].x;
			for(int n = 0; n < extrudedIndices.Count; n++)
			{
				if(n == i)
					continue;

				if(extrudedIndices[n].x == val)
				{
					pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, extrudedIndices[n].y, extrudedIndices[i].y);
					break;
				}
			}
		}

		pb.SetSharedIndices(sharedIndices);
		pb.RebuildFaceCaches();
		
		return newEdges.ToArray();
	}
#endregion

#region Detach

	/**
	 * Removes the vertex associations so that this face may be moved independently of the main object.
	 */
	public static void DetachFace(this pb_Object pb, pb_Face face)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		pb_IntArrayUtility.RemoveValues(ref sharedIndices, face.indices);

		// Add these vertices back into the sharedIndices array under it's own entry
		for(int i = 0; i < face.distinctIndices.Length; i++)
		{			
			int[] arr = new int[1] { face.distinctIndices[i] };
			sharedIndices = pbUtil.Add(sharedIndices, new pb_IntArray(arr));
		}

		pb.SetSharedIndices(sharedIndices);
	}
#endregion

#region Bridge
#if !PROTOTYPE
		public static bool Bridge(this pb_Object pb, pb_Edge a, pb_Edge b) { return pb.Bridge(a, b, true); }
		public static bool Bridge(this pb_Object pb, pb_Edge a, pb_Edge b, bool enforcePerimiterEdgesOnly)
		{
			pb_IntArray[] sharedIndices = pb.GetSharedIndices();

			// Check to see if a face already exists
			if(enforcePerimiterEdgesOnly)
			{
				if( pbMeshUtils.GetConnectedFaces(pb, a).Count > 1 || pbMeshUtils.GetConnectedFaces(pb, b).Count > 1 )
				{
					Debug.LogWarning("Both edges are not on perimeter!  You may turn off this Bridging restriction in Preferences/ProBuilder/Bridge Perimiter Edges Only");
					return false;
				}
			}
			else
			{
				foreach(pb_Face face in pb.faces)
				{
					if(face.edges.IndexOf(a, sharedIndices) >= 0 && face.edges.IndexOf(b, sharedIndices) >= 0)
					{
						Debug.LogWarning("Face already exists between these two edges!");
						return false;
					}
				}
			}

			Vector3[] verts = pb.vertices;
			Vector3[] v;
			int[] s;
			pb_UV uvs = new pb_UV();
			Color32 color = (Color32)Color.white;
			Material mat = pb_Constant.DefaultMaterial;

			// Get material and UV stuff from the first edge face 
			foreach(pb_Face face in pb.faces)
			{
				if(face.edges.Contains(a))	
				{
					uvs = new pb_UV(face.uv);
					mat = face.material;
					color = face.colors[0];
					break;
				}
			}

			// Bridge will form a triangle
			if( a.Contains(b.x, sharedIndices) || a.Contains(b.y, sharedIndices) )
			{
				v = new Vector3[3];
				s = new int[3];

				bool axbx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.x) > -1;
				bool axby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.y) > -1;
				
				bool aybx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.x) > -1;
				bool ayby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.y) > -1;
				
				if(axbx)
				{	
					v[0] = verts[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(axby)
				{
					v[0] = verts[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}
				else
				if(aybx)
				{
					v[0] = verts[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(ayby)
				{
					v[0] = verts[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}

				pb.AppendFace(
					v,
					new pb_Face( axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, mat, uvs, 0, -1, -1, color ),
					s);

				pb.RebuildFaceCaches();
				pb.Refresh();

				return true;
			}

			// Else, bridge will form a quad

			v = new Vector3[4];
			s = new int[4]; // shared indices index to add to

			v[0] = verts[a.x];
			s[0] = sharedIndices.IndexOf(a.x);
			v[1] = verts[a.y];
			s[1] = sharedIndices.IndexOf(a.y);

			Vector3 nrm = Vector3.Cross( verts[b.x]-verts[a.x], verts[a.y]-verts[a.x] ).normalized;
			Vector2[] planed = pb_Math.VerticesTo2DPoints( new Vector3[4] {verts[a.x], verts[a.y], verts[b.x], verts[b.y] }, nrm );

			Vector2 ipoint = Vector2.zero;
			bool interescts = pb_Math.GetLineSegmentIntersect(planed[0], planed[2], planed[1], planed[3], ref ipoint);

			if(!interescts)
			{
				v[2] = verts[b.x];
				s[2] = sharedIndices.IndexOf(b.x);
				v[3] = verts[b.y];
				s[3] = sharedIndices.IndexOf(b.y);
			}
			else
			{
				v[2] = verts[b.y];
				s[2] = sharedIndices.IndexOf(b.y);
				v[3] = verts[b.x];
				s[3] = sharedIndices.IndexOf(b.x);
			}

			pb.AppendFace(
				v,
				new pb_Face( new int[6] {2, 1, 0, 2, 3, 1 }, mat, uvs, 0, -1, -1, color ),
				s);

			pb.RebuildFaceCaches();

			return true;
		}
#endif
#endregion

#region Combine

	/**
	 *	\brief Given an array of "donors", this method returns a merged #pb_Object.
	 */
	 public static bool CombineObjects(pb_Object[] pbs, out pb_Object combined)
	 {
	 	combined = null;

	 	if(pbs.Length < 1) return false;

	 	List<Vector3> v = new List<Vector3>();
	 	List<pb_Face> f = new List<pb_Face>();
	 	List<pb_IntArray> s = new List<pb_IntArray>();

	 	foreach(pb_Object pb in pbs)
	 	{
	 		int vertexCount = v.Count;

	 		// Vertices
	 		{
		 		v.AddRange(pb.VerticesInWorldSpace());
			}

			// Faces
		 	{
		 		pb_Face[] faces = new pb_Face[pb.faces.Length];
		 		for(int i = 0; i < faces.Length; i++)
		 		{
		 			faces[i] = new pb_Face(pb.faces[i]);
		 			faces[i].ShiftIndices(vertexCount);
		 			faces[i].RebuildCaches();
		 		}

		 		f.AddRange(faces);
	 		}

	 		// Shared Indices
	 		{
		 		pb_IntArray[] si = pb.GetSharedIndices();
		 		for(int i = 0; i < si.Length; i++)
		 		{
		 			for(int n = 0; n < si[i].Length; n++)
		 				si[i][n] += vertexCount;
		 		}

		 		s.AddRange(si);
		 	}
	 	}

	 	combined = pb_Object.CreateInstanceWithVerticesFacesSharedIndices(v.ToArray(), f.ToArray(), s.ToArray());
	 	
	 	combined.CenterPivot(new int[1]{0});

	 	return true;
	 }
#endregion

#region Faces

	/**
	 *	Iterates through all triangles in a pb_Object and removes triangles with area <= 0 and 
	 *	tris with indices that point to the same vertex.
	 */
	public static int[] RemoveDegenerateTriangles(this pb_Object pb)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		Vector3[] v = pb.vertices;
		List<pb_Face> del = new List<pb_Face>();

		int[] removedIndices;

		List<pb_Face> f = new List<pb_Face>();

		foreach(pb_Face face in pb.faces)
		{
			List<int> tris = new List<int>();
	
			int[] ind = face.indices;
			for(int i = 0; i < ind.Length; i+=3)
			{
				int[] s = new int[3]
				{
					sharedIndices.IndexOf(ind[i+0]),
					sharedIndices.IndexOf(ind[i+1]),
					sharedIndices.IndexOf(ind[i+2])
				};

				float area = pb_Math.TriangleArea(v[ind[i+0]], v[ind[i+1]], v[ind[i+2]]);

				if( (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) || area <= 0 )
				{
					// don't include this face in the reconstruct
					;
				}
				else
				{
					tris.Add(ind[i+0]);
					tris.Add(ind[i+1]);
					tris.Add(ind[i+2]);
				}
			}

			if(tris.Count > 0)
			{
				face.SetIndices(tris.ToArray());
				face.RebuildCaches();

				f.Add(face);
			}
			else
			{
				del.Add(face);
			}
		}

		pb.SetFaces(f.ToArray());

		removedIndices = pb.RemoveUnusedVertices();

		return removedIndices;
	}
		
	/**
	 *	Removes triangles that occupy the same space, and point to the same vertices.
	 */
	public static int[] RemoveDuplicateTriangles(this pb_Object pb)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		Vector3[] v = pb.vertices;
		List<pb_Face> del = new List<pb_Face>();

		int[] removedIndices;

		List<pb_Face> f = new List<pb_Face>();

		foreach(pb_Face face in pb.faces)
		{
			List<int> tris = new List<int>();
	
			int[] ind = face.indices;
			for(int i = 0; i < ind.Length; i+=3)
			{
				int[] s = new int[3]
				{
					sharedIndices.IndexOf(ind[i+0]),
					sharedIndices.IndexOf(ind[i+1]),
					sharedIndices.IndexOf(ind[i+2])
				};

				float area = pb_Math.TriangleArea(v[ind[i+0]], v[ind[i+1]], v[ind[i+2]]);

				if( (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) || area <= 0 )
				{
					// don't include this face in the reconstruct
					;
				}
				else
				{
					tris.Add(ind[i+0]);
					tris.Add(ind[i+1]);
					tris.Add(ind[i+2]);
				}
			}

			if(tris.Count > 0)
			{
				face.SetIndices(tris.ToArray());
				face.RebuildCaches();

				f.Add(face);
			}
			else
			{
				del.Add(face);
			}
		}

		pb.SetFaces(f.ToArray());

		removedIndices = pb.RemoveUnusedVertices();

		return removedIndices;
	}
#endregion
	}
}