using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
public static class pbAppendDelete
{

#region Append Face

/**
	 *	\brief
	 *	param sharedIndex An optional array that sets the new pb_Face indices to use the _sharedIndices array.
	 *	\returns The newly appended pb_Face.
	 */
	public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, pb_Face face)
	{
		int[] shared = new int[v.Length];
		for(int i = 0; i < v.Length; i++)
			shared[i] = -1;
		return pb.AppendFace(v, face, shared);
	}
	
	/**
	 * Append a new face to the pb_Object.
	 */
	public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, pb_Face face, int[] sharedIndex)
	{
		List<Vector3> _verts = new List<Vector3>(pb.vertices);
		List<pb_Face> _faces = new List<pb_Face>(pb.faces);
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		int vertexCount = pb.vertexCount;

		_verts.AddRange(v);
		face.ShiftIndicesToZero();
		face.ShiftIndices(vertexCount);
		face.RebuildCaches();
		_faces.Add(face);

		// Dictionary<int, int> grp = new Dictionary<int, int>();	// this allows append face to add new vertices to a new shared index group
		// 														// if the sharedIndex is negative and less than -1, it will create new gorup
		// 														// that other sharedIndex members can then append themselves to.
		for(int i = 0; i < sharedIndex.Length; i++)
		{
			// if(sharedIndex[i] < -1)
			// {
			// 	if(grp.ContainsKey(sharedIndex[i]))
			// 		AddIndexToSharedIndexArray(grp[sharedIndex[i]], i+vertexCount);
			// 	else
			// 		grp.Add(sharedIndex[i], AddIndexToSharedIndexArray(sharedIndex[i], i+vertexCount));
			// }
			// else
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndex[i], i+vertexCount);
		}

		pb.SetSharedIndices(sharedIndices);
		pb.SetVertices(_verts.ToArray() );
		pb.SetFaces(_faces.ToArray());

		pb.ToMesh();

		return face;
	}

	/**
	 * Append a group of new faces to the pb_Object.  Significantly faster than calling AppendFace multiple times.
	 */
	public static pb_Face[] AppendFaces(this pb_Object pb, Vector3[][] new_Vertices, pb_Face[] new_Faces, int[][] new_SharedIndices)
	{
		List<Vector3> _verts = new List<Vector3>(pb.vertices);
		List<pb_Face> _faces = new List<pb_Face>(pb.faces);
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		int vc = pb.vertexCount;

		// Dictionary<int, int> grp = new Dictionary<int, int>();	// this allows append face to add new vertices to a new shared index group
		// 														// if the sharedIndex is negative and less than -1, it will create new gorup
		// 														// that other sharedIndex members can then append themselves to.
		for(int i = 0; i < new_Faces.Length; i++)
		{
			_verts.AddRange(new_Vertices[i]);
			new_Faces[i].ShiftIndicesToZero();
			new_Faces[i].ShiftIndices(vc);
			_faces.Add(new_Faces[i]);

			if(new_SharedIndices != null && new_Vertices[i].Length != new_SharedIndices[i].Length)
			{
				Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
				return null;
			}

			if(new_SharedIndices != null)
				for(int j = 0; j < new_SharedIndices[i].Length; j++)
				{
					// TODO - FIX ME
					// if(new_SharedIndices[i][j] < -1)
					// {
					// 	if(grp.ContainsKey(new_SharedIndices[i][j]))
					// 		AddValueAtIndex(grp[new_SharedIndices[i][j]], j+vc);
					// 	else
					// 		grp.Add(new_SharedIndices[i][j], AddValueAtIndex(new_SharedIndices[i][j], j+vc));
					// }
					// else
						pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, new_SharedIndices[i][j], j+vc);
				}
			else
				for(int j = 0; j < new_Vertices[i].Length; j++)
				{
					pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, j+vc);
				}
			vc = _verts.Count;
		}

		pb.SetSharedIndices(sharedIndices);
		pb.SetVertices(_verts.ToArray());
		pb.SetFaces(_faces.ToArray());
		pb.ToMesh();

		return new_Faces;
	}
#endregion
#region Delete Face

	/**
	 *	Removes the passed face from this pb_Object.  Handles shifting vertices and triangles, as well as messing with the sharedIndices cache.
	 */
	public static void DeleteFace(this pb_Object pb, pb_Face face)
	{		
		int f_ind = System.Array.IndexOf(pb.faces, face);
		int[] distInd = face.distinctIndices;
		
		Vector3[] verts = pb.vertices.RemoveAt(distInd);
		pb_Face[] nFaces = pb.faces.RemoveAt(f_ind);

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;
			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				for(int d = 0; d < distInd.Length; d++)
				{
					if(tris[n] > distInd[d])
						sub++;
				}
				tris[n] -= sub;
			}
			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		pb_IntArray[] si = pb.sharedIndices;
		pb_IntArrayUtility.RemoveValuesAndShift(ref si, distInd);

		pb.SetSharedIndices(si);		
		pb.SetVertices(verts);
		pb.SetFaces(nFaces);
		pb.RebuildFaceCaches();
		
		pb.ToMesh();
	}


	/**
	 * Removes faces from a pb_Object.  Overrides available for pb_Face[] and int[] faceIndices.  handles
	 * all the sharedIndices moving stuff for you.
	 */
	public static void DeleteFaces(this pb_Object pb, pb_Face[] faces)
	{	
		int[] f_ind = new int[faces.Length];
		for(int i = 0; i < faces.Length; i++)
			f_ind[i] = System.Array.IndexOf(pb.faces, faces[i]);
		
		int[] distInd = pb_Face.AllTrianglesDistinct(faces);

		Vector3[] verts = pb.vertices.RemoveAt(distInd);
		pb_Face[] nFaces = pb.faces.RemoveAt(f_ind);

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;
			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				for(int d = 0; d < distInd.Length; d++)
				{
					if(tris[n] > distInd[d])
						sub++;
				}
				tris[n] -= sub;
			}
			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		pb_IntArray[] si = pb.sharedIndices;
		pb_IntArrayUtility.RemoveValuesAndShift(ref si, distInd);
		
		pb.SetSharedIndices(si);
		pb.SetVertices(verts);
		pb.SetFaces(nFaces);
		pb.RebuildFaceCaches();
		
		pb.ToMesh();
	}
#endregion
}
}