using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;

#if BUGGER
using Parabox.Bugger;
#endif

namespace ProBuilder2.MeshOperations {

public static class pbUVOps
{

#region Sew / Split

	/**
	 * Sews a UV seam using delta to determine which UVs are close enough to be merged.
	 * \sa pbVertexOps::WeldVertices
	 */
	public static bool Sew(this pb_Object pb, int[] indices, float delta)
	{
		int[] si = new int[indices.Length];
		Vector2[] uvs = pb.msh.uv;

		// set the shared indices cache to a unique non-used index
		for(int i = 0; i < indices.Length; i++)
			si[i] = -(i+1);
		
		pb_IntArray[] sharedIndices = pb.sharedIndicesUV;

		for(int i = 0; i < indices.Length-1; i++)
		{
			for(int n = i+1; n < indices.Length; n++)
			{
				if(si[i] == si[n])
					continue;	// they already share a vertex
				
				// Note that this will not take into account 
				if(Vector2.Distance(uvs[indices[i]], uvs[indices[n]]) < delta)
				{
					Vector3 cen = (uvs[indices[i]] + uvs[indices[n]]) / 2f;
					uvs[indices[i]] = cen;
					uvs[indices[n]] = cen;
					int newIndex = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, new int[2] {indices[i], indices[n]});
					si[i] = newIndex;
					si[n] = newIndex;
				}
			}
		}

		pb.SetUV(uvs);
		pb.SetSharedIndicesUV(sharedIndices);

		// pb.Refresh();

		return true;
	}

	/**
	 * Creates separate entries in sharedIndices cache for all passed indices.
	 * If indices are not present in pb_IntArray[], don't do anything with them.
	 */
	public static bool SplitUVs(this pb_Object pb, int[] indices)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndicesUV;

		List<int> usedIndex = new List<int>();
		List<int> splits = new List<int>();

		for(int i = 0; i < indices.Length; i++)
		{
			int index = sharedIndices.IndexOf(indices[i]);
			
			if(index > -1)
			{
				if(!usedIndex.Contains(index))
				{
					usedIndex.Add(index);
					splits.AddRange(sharedIndices[index].array);
				}
			}
		}

		pb_IntArrayUtility.RemoveValues(ref sharedIndices, splits.ToArray());

		foreach(int i in splits)
			pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, i);

		pb.SetSharedIndicesUV(sharedIndices);

		return true;
	}
#endregion

#region Projection

	/**
	 * Projects UVs on all passed faces, automatically updating the sharedIndicesUV table
	 * as required (only associates vertices that share a seam).
	 */
	public static void ProjectFacesAuto(pb_Object pb, pb_Face[] faces)
	{
		int[] ind = pb_Face.AllTrianglesDistinct(faces);
		Vector3[] verts = pbUtil.ValuesWithIndices(pb.vertices, ind);
		
		/* get average face normal */
		Vector3 nrm = Vector3.zero;
		foreach(pb_Face face in faces)
			nrm += pb_Math.Normal(pb, face);
		nrm /= (float)faces.Length;

		/* project uv coordinates */
		Vector2[] uvs = pb_UV_Utility.PlanarProject(verts, nrm);

		/* re-assign new projected coords back into full uv array */
		Vector2[] rebuiltUVs = pb.msh.uv;
		for(int i = 0; i < ind.Length; i++)
			rebuiltUVs[ind[i]] = uvs[i];

		/* and set the msh uv array using the new coordintaes */
		pb.SetUV(rebuiltUVs);	
		
		/* now go trhough and set all adjacent face groups to use matching element groups */
		
	}
#endregion
}
}