using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.Triangulator;
using ProBuilder2.Triangulator.Geometry;

#if BUGGER
using Parabox.Bugger;
#endif

namespace ProBuilder2.MeshOperations
{
public static class pbSubdivideSplit
{
#if !PROTOTYPE

	/**
	 * Insert an point at the center of each face and split every edge.
	 */
	public static bool Subdivide(this pb_Object pb)
	{
		try
		{
			List<EdgeConnection> ec = new List<EdgeConnection>();
			foreach(pb_Face f in pb.faces)
				ec.Add(new EdgeConnection(f, new List<pb_Edge>(f.edges)));

			pb_Face[] faces;
			ConnectEdges(pb, ec, out faces);
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Subdivide failed: \n" + e.ToString());
			return false;
		}
		return true;
	}

	public static bool SubdivideFace(this pb_Object pb, pb_Face[] faces, out pb_Face[] splitFaces)
	{
		List<EdgeConnection> split = new List<EdgeConnection>();
		foreach(pb_Face face in pb.SelectedFaces)
			split.Add(new EdgeConnection(face, new List<pb_Edge>(face.edges)));

		return pb.ConnectEdges(split, out splitFaces);
	}

	/**
	 *	
	 */
	public static bool ConnectEdges(this pb_Object pb, List<EdgeConnection> edgeConnectionsUnfiltered, out pb_Face[] faces)
	{
		// first, remove any junk connections.  faces with less than two edges confuse this method.
		List<EdgeConnection> edgeConnections = new List<EdgeConnection>();
		foreach(EdgeConnection ec in edgeConnectionsUnfiltered)
			if(ec.isValid)
				edgeConnections.Add(ec);

		int len = edgeConnections.Count;

		if(len < 1)
		{
			Debug.LogWarning("No valid split paths found.  This is most likely because you are attempting to split edges that do belong to the same face, or do not have more than one edge selected.  This is not currently supported, sorry!");
			faces = null;
			return false;
		}


		Vector3[] vertices = pb.vertices;

		List<pb_Face> successfullySplitFaces = new List<pb_Face>();

		List<pb_Face> all_splitFaces = new List<pb_Face>();
		List<Vector3[]> all_splitVertices = new List<Vector3[]>();
		List<int[]> all_splitSharedIndices = new List<int[]>();
		bool[] success = new bool[len];

		// use a nullable type because in order for the adjacent face triangulation
		// code to work, it needs to know what dangling vert belongs to which edge, 
		// if we out a vector3[] with each index corresponding to the passed edges
		// in EdgeConnection, it's easy to maintain the relationship.
		Vector3?[][] danglingVertices = new Vector3?[len][];	

		int i = 0;
		foreach(EdgeConnection fc in edgeConnections)
		{	
			pb_Face[] splitFaces = null;
			Vector3[][] splitVertices = null;
			int[][] splitSharedIndices = null;
	
			if( fc.edges.Count < 3 )
			{
				Vector3 edgeACen = (vertices[fc.edges[0].x] + vertices[fc.edges[0].y]) / 2f; 
				Vector3 edgeBCen = (vertices[fc.edges[1].x] + vertices[fc.edges[1].y]) / 2f;
				danglingVertices[i] = new Vector3?[2] { edgeACen, edgeBCen };
				success[i] = SplitFace_Internal(new SplitSelection(pb, fc.face, edgeACen, edgeBCen, false, false, -1, -1),
					out splitFaces,
					out splitVertices, 
					out splitSharedIndices);

				if(success[i])
					successfullySplitFaces.Add(fc.face);
			}
			else
			{
				Vector3?[] appendedVertices = null;
				success[i] = SubdivideFace_Internal(pb, fc,
					out appendedVertices,
					out splitFaces,
					out splitVertices,
					out splitSharedIndices);
	
				if(success[i])
					successfullySplitFaces.Add(fc.face);
	
				danglingVertices[i] = appendedVertices;
			}

			if(success[i])
			{
				int texGroup = fc.face.textureGroup < 0 ? pb.UnusedTextureGroup(i+1) : fc.face.textureGroup;
				
				for(int j = 0; j < splitFaces.Length; j++)
				{
					splitFaces[j].textureGroup = texGroup;
					all_splitFaces.Add(splitFaces[j]);
					all_splitVertices.Add(splitVertices[j]);
					all_splitSharedIndices.Add(splitSharedIndices[j]);
				}
			}

			i++;
		}

		/**
		 *	Figure out which faces need to be re-triangulated
		 */
		pb_Edge[][] tedges = new pb_Edge[edgeConnections.Count][];
		int n = 0;
		for(i = 0; i < edgeConnections.Count; i++)
			tedges[n++] = edgeConnections[i].edges.ToArray();

		List<pb_Face>[][] allConnects = pbMeshUtils.GetConnectedFacesJagged(pb, tedges);		

		Dictionary<pb_Face, List<Vector3>> addVertex = new Dictionary<pb_Face, List<Vector3>>();
		List<pb_Face> temp = new List<pb_Face>();
		for(int j = 0; j < edgeConnections.Count; j++)
		{
			if(!success[j]) continue;

			// check that this edge has a buddy that it welded it's new vertex to, and if not,
			// create one
			for(i = 0; i < edgeConnections[j].edges.Count; i++)
			{
				if(danglingVertices[j][i] == null) 
					continue;

				List<pb_Face> connected = allConnects[j][i];

				foreach(pb_Face face in connected)
				{
					int ind = successfullySplitFaces.IndexOf(face);

					if(ind < 0)
					{
						if(addVertex.ContainsKey(face))
							addVertex[face].Add((Vector3)danglingVertices[j][i]);
						else
						{
							temp.Add(face);
							addVertex.Add(face, new List<Vector3>(1) { (Vector3)danglingVertices[j][i] });
						}
					}
				}
			}
		}
		
		pb_Face[] appendedFaces = pb.AppendFaces(all_splitVertices.ToArray(), all_splitFaces.ToArray(), all_splitSharedIndices.ToArray());
		
		List<pb_Face> triangulatedFaces = new List<pb_Face>();
		foreach(KeyValuePair<pb_Face, List<Vector3>> add in addVertex)
		{
			pb_Face newFace;
			if( pb.AppendVerticesToFace(add.Key, add.Value, out newFace) )
				triangulatedFaces.Add(newFace);
			else
				Debug.LogError("Mesh re-triangulation failed.  Specifically, AppendVerticesToFace(" + add.Key + " : " + add.Value.ToFormattedString(", "));
		}

		// Re-triangulate any faces left with dangling verts at edges
		// Weld verts, including those added in re-triangu
		int[] splitFaceTris = pb_Face.AllTriangles(appendedFaces);
		int[] triangulatedFaceTris = pb_Face.AllTriangles(triangulatedFaces);
		int[] allModifiedTris = new int[splitFaceTris.Length + triangulatedFaceTris.Length];
		System.Array.Copy(splitFaceTris, 0, allModifiedTris, 0, splitFaceTris.Length);
		System.Array.Copy(triangulatedFaceTris, 0, allModifiedTris, splitFaceTris.Length, triangulatedFaceTris.Length);
		
		pb.WeldVertices(allModifiedTris, Mathf.Epsilon);
		
		// Now that we're done screwing with geo, delete all the old faces (that were successfully split)		
		pb.DeleteFaces( successfullySplitFaces.ToArray() );
		faces = appendedFaces;
		return true;
	}

	/**
	 *	Splits face per vertex.
	 *	Todo - Could implement more sanity checks - namely testing for edges before sending to Split_Internal.  However,
	 *	the split method is smart enough to fail on those cases, so ignore for now.
	 */
	public static bool ConnectVertices(this pb_Object pb, List<VertexConnection> vertexConnectionsUnfiltered, out pb_Face[] faces)
	{
		List<VertexConnection> vertexConnections = new List<VertexConnection>();
		List<int> inds = new List<int>();

		int i = 0;

		for(i = 0; i < vertexConnectionsUnfiltered.Count; i++)
		{
			VertexConnection vc = vertexConnectionsUnfiltered[i];
			vc.indices = vc.indices.Distinct().ToList();

			if(vc.isValid) 
			{
				inds.AddRange(vc.indices);
				vertexConnections.Add(vc);
			}
		}

		if(vertexConnections.Count < 1)
		{
			faces = null;
			return false;
		}
		
		int len = vertexConnections.Count;
		List<pb_Face> successfullySplitFaces = new List<pb_Face>();
		List<pb_Face> all_splitFaces = new List<pb_Face>();
		List<Vector3[]> all_splitVertices = new List<Vector3[]>();
		List<int[]> all_splitSharedIndices = new List<int[]>();
		bool[] success = new bool[len];

		pb_IntArray[] sharedIndices = pb.sharedIndices;

		i = 0;
		foreach(VertexConnection vc in vertexConnections)
		{
			pb_Face[] splitFaces = null;
			Vector3[][] splitVertices = null;
			int[][] splitSharedIndices = null;
	
			if( vc.indices.Count < 3 )
			{
				int indA = vc.face.indices.IndexOf(vc.indices[0], sharedIndices);
				int indB = vc.face.indices.IndexOf(vc.indices[1], sharedIndices);
				
				if(indA < 0 || indB < 0)
				{
					success[i] = false;
					continue;
				}

				indA = vc.face.indices[indA];
				indB = vc.face.indices[indB];

				success[i] = SplitFace_Internal(new SplitSelection(pb, vc.face, pb.vertices[indA], pb.vertices[indB], true, true, indA, indB),
					out splitFaces,
					out splitVertices, 
					out splitSharedIndices);

				if(success[i])
					successfullySplitFaces.Add(vc.face);
			}
			else
			{
				success[i] = PokeFace_Internal(pb, vc.face, vc.indices.ToArray(),
					out splitFaces,
					out splitVertices, 
					out splitSharedIndices);

				if(success[i])
					successfullySplitFaces.Add(vc.face);
			}

			if(success[i])
			{
				int texGroup = pb.UnusedTextureGroup(i+1);

				for(int j = 0; j < splitFaces.Length; j++)
				{
					splitFaces[j].textureGroup = texGroup;
					all_splitFaces.Add(splitFaces[j]);
					all_splitVertices.Add(splitVertices[j]);
					all_splitSharedIndices.Add(splitSharedIndices[j]);
				}
			}

			i++;
		}

		if(all_splitFaces.Count < 1)
		{
			faces = null;
			return false;
		}

		pb_Face[] appendedFaces = pb.AppendFaces(all_splitVertices.ToArray(), all_splitFaces.ToArray(), all_splitSharedIndices.ToArray());
		inds.AddRange(pb_Face.AllTriangles(appendedFaces));
		
		pb.WeldVertices(inds.ToArray(), Mathf.Epsilon);

		pb.DeleteFaces(successfullySplitFaces.ToArray());

		faces = appendedFaces;
		return true;
	}

	/**
	 *	Store information about a point on a face when to be used when splitting. ProBuilder
	 *	allows splitting faces from 2 points that can either land on any mix of vertex or edge,
	 *	so we need to tell the split face function that information.
	 */
	private class SplitSelection
	{
		public pb_Object pb;
		public pb_Face face;

		public Vector3 pointA;
		public Vector3 pointB;

		public bool aIsVertex;
		public bool bIsVertex;

		public int indexA;	// face index - cannot be a sharedIndex
		public int indexB;	// face index - cannot be a sharedIndex

		/**
		 *	Constructor
		 */
		public SplitSelection(pb_Object pb, pb_Face face, Vector3 pointA, Vector3 pointB, bool aIsVertex, bool bIsVertex, int indexA, int indexB)
		{
			this.pb = pb;
			this.face = face;
			this.pointA = pointA;
			this.pointB = pointB;
			this.aIsVertex = aIsVertex;
			this.bIsVertex = bIsVertex;
			this.indexA = indexA;
			this.indexB = indexB;
		}

		public override string ToString()
		{
			return "face: " + face.ToString() + "\n" + "a is vertex: " + aIsVertex + "\nb is vertex: " + bIsVertex + "\nind a, b: "+ indexA + ", " + indexB;
		}
	}

	/**
	 *	This method assumes that the split selection edges share a common face and have already been sanity checked.  Will return 
	 *	the variables necessary to compose a new face from the split, or null if the split is invalid.
	 */
	private static bool SplitFace_Internal(SplitSelection splitSelection,
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out int[][] splitSharedIndices) 
	{
		splitFaces = null;
		splitVertices = null;
		splitSharedIndices = null;

		pb_Object pb = splitSelection.pb;	// we'll be using this a lot
		pb_Face face = splitSelection.face;	// likewise

		int[] indices = face.distinctIndices;
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		int[] sharedIndex = new int[indices.Length];
		for(int i = 0; i < indices.Length; i++)
			sharedIndex[i] = sharedIndices.IndexOf(indices[i]);

		// First order of business is to translate the face to 2D plane.
		Vector3[] verts = pb.GetVertices(face.distinctIndices);

		Vector3 projAxis = pb_Math.GetProjectionAxis( pb_Math.Normal(pb.GetVertices(face.indices))).ToVector3();
		Vector2[] plane = pb_Math.VerticesTo2DPoints(verts, projAxis);

		// Split points
 		Vector3 splitPointA_3d = splitSelection.pointA;
 		Vector3 splitPointB_3d = splitSelection.pointB;

		Vector2 splitPointA_2d = pb_Math.VerticesTo2DPoints( new Vector3[1] { splitPointA_3d }, projAxis )[0];
		Vector2 splitPointB_2d = pb_Math.VerticesTo2DPoints( new Vector3[1] { splitPointB_3d }, projAxis )[0];

		List<Vector3> v_polyA = new List<Vector3>();	// point in object space
		List<Vector2> v_polyA_2d = new List<Vector2>();	// point in 2d space - used to triangulate
		List<Vector3> v_polyB = new List<Vector3>();	// point in object space
		List<Vector2> v_polyB_2d = new List<Vector2>();	// point in 2d space - used to triangulate
		List<int> i_polyA = new List<int>();			// sharedIndices array index
		List<int> i_polyB = new List<int>();			// sharedIndices array index

		List<int> nedgeA = new List<int>();
		List<int> nedgeB = new List<int>();

		// Sort points into two separate polygons
		for(int i = 0; i < indices.Length; i++)
		{
			// is this point (a) a vertex to split or (b) on the negative or positive side of this split line
			if( (splitSelection.aIsVertex && splitSelection.indexA == indices[i]) ||  (splitSelection.bIsVertex && splitSelection.indexB == indices[i]) )
			{
				v_polyA.Add( verts[i] );
				v_polyB.Add( verts[i] );

				v_polyA_2d.Add( plane[i] );
				v_polyB_2d.Add( plane[i] );

				i_polyA.Add( sharedIndex[i] );
				i_polyB.Add( sharedIndex[i] );
			}
			else
			{
				// split points across the division line
				Vector2 perp = pb_Math.Perpendicular(splitPointB_2d, splitPointA_2d);
				Vector2 origin = (splitPointA_2d + splitPointB_2d) / 2f;
				
				if( Vector2.Dot(perp, plane[i]-origin) > 0 )
				{
					v_polyA.Add(verts[i]);
					v_polyA_2d.Add(plane[i]);
					i_polyA.Add(sharedIndex[i]);
				}
				else
				{
					v_polyB.Add(verts[i]);
					v_polyB_2d.Add(plane[i]);
					i_polyB.Add(sharedIndex[i]);
				}
			}
		}

		if(!splitSelection.aIsVertex)
		{
			v_polyA.Add( splitPointA_3d );
			v_polyA_2d.Add( splitPointA_2d );
			v_polyB.Add( splitPointA_3d );
			v_polyB_2d.Add( splitPointA_2d );
			i_polyA.Add(-1);
			i_polyB.Add(-1);	//	neg 1 because it's a new vertex point

			nedgeA.Add(v_polyA.Count);
			nedgeB.Add(v_polyB.Count);
		}

		if(!splitSelection.bIsVertex)
		{
			v_polyA.Add( splitPointB_3d );
			v_polyA_2d.Add( splitPointB_2d );
			v_polyB.Add( splitPointB_3d );
			v_polyB_2d.Add( splitPointB_2d );
			i_polyA.Add(-1);
			i_polyB.Add(-1);	//	neg 1 because it's a new vertex point
		
			nedgeA.Add(v_polyA.Count);
			nedgeB.Add(v_polyB.Count);
		}

		if(v_polyA_2d.Count < 3 || v_polyB_2d.Count < 3)
		{
			splitFaces = null;
			splitVertices = null;
			splitSharedIndices = null;
			return false;
		}

		// triangulate new polygons
		int[] t_polyA = Delauney.Triangulate(v_polyA_2d).ToIntArray();
		int[] t_polyB = Delauney.Triangulate(v_polyB_2d).ToIntArray();

		if(t_polyA.Length < 3 || t_polyB.Length < 3)
			return false;

		// figure out the face normals for the new faces and check to make sure they match the original face
		Vector2[] pln = pb_Math.VerticesTo2DPoints( pb.GetVertices(face.indices), projAxis );
		Vector3 nrm = Vector3.Cross( pln[2] - pln[0], pln[1] - pln[0]);
		Vector3 nrmA = Vector3.Cross( v_polyA_2d[ t_polyA[2] ]-v_polyA_2d[ t_polyA[0] ], v_polyA_2d[ t_polyA[1] ]-v_polyA_2d[ t_polyA[0] ] );
		Vector3 nrmB = Vector3.Cross( v_polyB_2d[ t_polyB[2] ]-v_polyB_2d[ t_polyB[0] ], v_polyB_2d[ t_polyB[1] ]-v_polyB_2d[ t_polyB[0] ] );

		if(Vector3.Dot(nrm, nrmA) < 0) System.Array.Reverse(t_polyA);
		if(Vector3.Dot(nrm, nrmB) < 0) System.Array.Reverse(t_polyB);

		// triangles, material, pb_UV, smoothing group, shared index
		pb_Face faceA = new pb_Face( t_polyA, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.color);
		pb_Face faceB = new pb_Face( t_polyB, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.color);

		splitFaces = new pb_Face[2] { faceA, faceB };
		splitVertices = new Vector3[2][] { v_polyA.ToArray(), v_polyB.ToArray() };
		splitSharedIndices = new int[2][] { i_polyA.ToArray(), i_polyB.ToArray() };

		return true;
	}

	// todo - there's a lot of duplicate code between this and poke face.
	/**
	 *	Inserts a vertex at the center of each edge, then connects the new vertices to another new
	 *	vertex placed at the center of the face.
	 */
	// internal method - so it's allow to be messy, right?
	private static bool SubdivideFace_Internal(pb_Object pb, EdgeConnection edgeConnection, 
		out Vector3?[] appendedVertices,	
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out int[][] splitSharedIndices)
	{
		splitFaces = null;
		splitVertices = null;
		splitSharedIndices = null;
		appendedVertices = new Vector3?[edgeConnection.edges.Count];

		// cache all the things
		pb_Face face = edgeConnection.face;
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		Vector3[] vertices = pb.vertices;

		List<Vector3> edgeCenters3d = new List<Vector3>();//pb.GetVertices(edgeConnection.face));
		
		// filter duplicate edges
		int u = 0;
		List<int> usedEdgeIndices = new List<int>();
		foreach(pb_Edge edge in edgeConnection.edges)
		{
			int ind = face.edges.IndexOf(edge, sharedIndices);
			if(!usedEdgeIndices.Contains(ind))
			{
				Vector3 cen = (vertices[edge.x] + vertices[edge.y]) / 2f;
				edgeCenters3d.Add(cen);
				usedEdgeIndices.Add(ind);
				appendedVertices[u] = cen;
			}
			else
				appendedVertices[u] = null;

			u++;
		}

		// now we have all the vertices of the old face, plus the new edge center vertices

		Vector3[] verts3d = pb.GetVertices(face.distinctIndices);
		Vector3 nrm = pb_Math.Normal(pb.GetVertices(face.indices));

		Vector2[] verts2d = pb_Math.VerticesTo2DPoints(verts3d, nrm);
		Vector2[] edgeCenters2d = pb_Math.VerticesTo2DPoints(edgeCenters3d.ToArray(), nrm);
		
		Vector3 cen3d = pb_Math.Average(verts3d);
		Vector2 cen2d = pb_Math.VerticesTo2DPoints( new Vector3[1] { cen3d }, nrm)[0];

		// Get the directions from which to segment this face
		Vector2[] dividers = new Vector2[edgeCenters2d.Length];
		for(int i = 0; i < edgeCenters2d.Length; i++)
			dividers[i] = (edgeCenters2d[i] - cen2d).normalized;

		List<Vector2>[] quadrants2d = new List<Vector2>[edgeCenters2d.Length];
		List<Vector3>[] quadrants3d = new List<Vector3>[edgeCenters2d.Length];
		List<int>[]		sharedIndex = new List<int>[edgeCenters2d.Length];

		for(int i = 0; i < quadrants2d.Length; i++)
		{
			quadrants2d[i] = new List<Vector2>(1) { cen2d };
			quadrants3d[i] = new List<Vector3>(1) { cen3d };
			sharedIndex[i] = new List<int>(1) { -2 };		// any negative value less than -1 will be treated as a new group
		}

		// add the divisors
		for(int i = 0; i < edgeCenters2d.Length; i++)
		{
			quadrants2d[i].Add(edgeCenters2d[i]);
			quadrants3d[i].Add(edgeCenters3d[i]);
			sharedIndex[i].Add(-1);	// -(i+2) to group new vertices in AppendFace

			// and add closest in the counterclockwise direction
			Vector2 dir = (edgeCenters2d[i]-cen2d).normalized;
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				if(j == i) continue;	// this is a dividing vertex - ignore

				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(edgeCenters2d[i]);
			quadrants3d[quad].Add(edgeCenters3d[i]);
			sharedIndex[quad].Add(-1);
		}

		// distribute the existing vertices
		for(int i = 0; i < face.distinctIndices.Length; i++)
		{
			Vector2 dir = (verts2d[i]-cen2d).normalized;	// plane corresponds to distinctIndices
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(verts2d[i]);
			quadrants3d[quad].Add(verts3d[i]);
			sharedIndex[quad].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));
		}

		int len = quadrants2d.Length;

		// Triangulate
		int[][] tris = new int[len][];
		for(int i = 0; i < len; i++)
		{
			if(quadrants2d[i].Count < 3)
			{
				Debug.LogError("Insufficient points to triangulate - bailing on subdivide operation.  This is probably due to a concave face, or maybe the compiler just doesn't like you today.  50/50 odds really.");
				return false;
			}
		
			tris[i] = Delauney.Triangulate(quadrants2d[i]).ToIntArray();
			
			Vector3[] nrm_check = new Vector3[3]
			{
				quadrants3d[i][tris[i][0]],
				quadrants3d[i][tris[i][1]],
				quadrants3d[i][tris[i][2]]
			};

			if( Vector3.Dot(nrm, pb_Math.Normal(nrm_check)) < 0 )
				System.Array.Reverse(tris[i]);
		}

		splitFaces 		= new pb_Face[len];
		splitVertices 	= new Vector3[len][];
		splitSharedIndices 	= new int[len][];

		for(int i = 0; i < len; i++)
		{
			// triangles, material, pb_UV, smoothing group, shared index
			splitFaces[i] = new pb_Face(tris[i], face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.color);
			splitVertices[i] = quadrants3d[i].ToArray();
			splitSharedIndices[i] = sharedIndex[i].ToArray();
		}

		return true;
	}

	/**
	 *	Inserts a split from each selected vertex to the center of the face
	 */
	private static bool PokeFace_Internal(pb_Object pb, pb_Face face, int[] indices_nonFaceSpecific,
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out int[][] splitSharedIndices)
	{
		splitFaces = null;
		splitVertices = null;
		splitSharedIndices = null;

		pb_IntArray[] sharedIndices = pb.sharedIndices;

		///** Sort index array such that it only uses indices local to the passed face
		int[] indices = new int[indices_nonFaceSpecific.Length];
		int[] dist_ind_si = new int[face.distinctIndices.Length];

		// figure out sharedIndices index of distinct Indices
		for(int i = 0; i < face.distinctIndices.Length; i++)
			dist_ind_si[i] = sharedIndices.IndexOf(face.distinctIndices[i]);

		// now do the same for non-face specific indices, assigning matching groups
		for(int i = 0; i < indices.Length; i++)
		{
			int ind = System.Array.IndexOf(dist_ind_si, sharedIndices.IndexOf(indices_nonFaceSpecific[i]));
			if(ind < 0) return false;

			indices[i] = face.distinctIndices[ind];
		}
		///** Sort index array such that it only uses indices local to the passed face

		Vector3 cen3d = pb_Math.Average(pb.GetVertices(face));
		
		Vector3[] verts 	= pb.GetVertices(face.distinctIndices);
		Vector3 nrm 		= pb_Math.Normal(pb.GetVertices(face.indices));
		Vector2[] plane 	= pb_Math.VerticesTo2DPoints(verts, nrm);
		Vector2[] indPlane 	= pb_Math.VerticesTo2DPoints(pb.GetVertices(indices), nrm);
		Vector2 cen2d 		= pb_Math.VerticesTo2DPoints( new Vector3[1] { cen3d }, nrm)[0];

		// Get the directions from which to segment this face
		Vector2[] dividers = new Vector2[indices.Length];
		for(int i = 0; i < indices.Length; i++)
			dividers[i] = (indPlane[i] - cen2d).normalized;

		List<Vector2>[] quadrants2d = new List<Vector2>[indices.Length];
		List<Vector3>[] quadrants3d = new List<Vector3>[indices.Length];
		List<int>[]		sharedIndex = new List<int>[indices.Length];

		for(int i = 0; i < quadrants2d.Length; i++)
		{
			quadrants2d[i] = new List<Vector2>(1) { cen2d };
			quadrants3d[i] = new List<Vector3>(1) { cen3d };
			sharedIndex[i] = new List<int>(1) { -2 };		// any negative value less than -1 will be treated as a new group
		}

		for(int i = 0; i < face.distinctIndices.Length; i++)
		{
			// if this index is a divider, it needs to belong to the leftmost and 
			// rightmost quadrant
			int indexInPokeVerts = System.Array.IndexOf(indices, face.distinctIndices[i]);
			int ignore = -1;
			if( indexInPokeVerts > -1)
			{	
				// Add vert to this quadrant
				quadrants2d[indexInPokeVerts].Add(plane[i]);
				quadrants3d[indexInPokeVerts].Add(verts[i]);
				sharedIndex[indexInPokeVerts].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));

				// And also the one closest counter clockwise
				ignore = indexInPokeVerts;
			}

			Vector2 dir = (plane[i]-cen2d).normalized;	// plane corresponds to distinctIndices
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				if(j == ignore) continue;	// this is a dividing vertex - ignore

				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(plane[i]);
			quadrants3d[quad].Add(verts[i]);
			sharedIndex[quad].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));
		}

		int len = quadrants2d.Length;

		// Triangulate
		int[][] tris = new int[len][];
		for(int i = 0; i < len; i++)
		{
			tris[i] = Delauney.Triangulate(quadrants2d[i]).ToIntArray();
			
			if(tris[i].Length < 3)
				return false;

			// todo - check that face normal is correct
		}

		splitFaces 		= new pb_Face[len];
		splitVertices 	= new Vector3[len][];
		splitSharedIndices 	= new int[len][];

		for(int i = 0; i < len; i++)
		{
			// triangles, material, pb_UV, smoothing group, shared index
			splitFaces[i] = new pb_Face(tris[i], face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.color);
			splitVertices[i] = quadrants3d[i].ToArray();
			splitSharedIndices[i] = sharedIndex[i].ToArray();
		}

		return true;
	}
#endif
}
}