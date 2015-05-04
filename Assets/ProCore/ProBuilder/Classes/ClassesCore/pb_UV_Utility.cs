using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;

/**
 *	\brief Responsible for mapping UV coordinates.  
 *	Generally should only be called by #pb_Object 
 *	after setting #pb_UV parameters.
 */
public class pb_UV_Utility
{

	/**
	 * wtf is this
	 */
	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings) { return PlanarMap(verts, uvSettings, null); }
	public static Vector2[] PlanarMap(Vector3[] verts, pb_UV uvSettings, Vector3? nrm)
	{
		Vector2[] uvs = PlanarProject(verts, nrm == null ? pb_Math.Normal(verts[0], verts[1], verts[2]) : (Vector3)nrm);
		
		if(!uvSettings.useWorldSpace)
			uvs = ShiftToPositive(uvs);

		uvs = ApplyUVSettings(uvs, uvSettings);
		return uvs;
	}
	
	public static Vector2[] PlanarProject(Vector3[] verts, Vector3 planeNormal)
	{
		if(verts.Length < 3)
		{
			Debug.LogWarning("Attempting to project UVs on a face with < 3 vertices.  This is most often caused by removing or creating Geometry and Undo-ing without selecting a new face.  Try deselecting this object then performing your edits.");
			return new Vector2[verts.Length];
		}

		Vector2[] uvs = new Vector2[verts.Length];
		Vector3 vec = Vector3.zero;

		pb_UV.ProjectionAxis project = pb_Math.GetProjectionAxis(planeNormal);

		switch(project)
		{
			case pb_UV.ProjectionAxis.Planar_X:
				vec = Vector3.up;
				break;

			case pb_UV.ProjectionAxis.Planar_Y:
				vec = Vector3.forward;
				break;
			
			case pb_UV.ProjectionAxis.Planar_Y_Negative:
				vec = -Vector3.forward;
				break;
			
			case pb_UV.ProjectionAxis.Planar_Z:
				vec = Vector3.up;
				break;

			default:
				vec = Vector3.forward;
				break;
		}
		
		/**
		 *	Assign vertices to UV coordinates
		 */
		for(int i = 0; i < verts.Length; i++)
		{
			float u, v;
			Vector3 uAxis, vAxis;
			
			// get U axis
			uAxis = Vector3.Cross(planeNormal, vec);
			uAxis.Normalize();

			// calculate V axis relative to U
			vAxis = Vector3.Cross(uAxis, planeNormal);
			vAxis.Normalize();

			u = Vector3.Dot(uAxis, verts[i]);
			v = Vector3.Dot(vAxis, verts[i]);

			uvs[i] = new Vector2(u, v);
		}

		return uvs;
	}

	private static Vector2[] ApplyUVSettings(Vector2[] uvs, pb_UV uvSettings)
	{
		Vector2 cen = pb_Math.Average(uvs);
		int len = uvs.Length;

		switch(uvSettings.fill)
		{
			case pb_UV.Fill.Tile:
				break;
			case pb_UV.Fill.Normalize:
				uvs = NormalizeUVs(uvs);
				break;
			case pb_UV.Fill.Stretch:
				uvs = StretchUVs(uvs);
				break;
		}

		if(uvSettings.justify != pb_UV.Justify.None)
			uvs = JustifyUVs(uvs, uvSettings.justify);

		// Apply offset last, so that fill and justify don't override it.
		uvs = OffsetUVs(uvs, -uvSettings.offset);

		Vector2 cen2 = Vector2.zero;

		for(int i = 0; i < len; i++)
		{
			Vector2 zeroed = uvs[i]-cen;
			
			if(uvSettings.useWorldSpace)
				uvs[i] = new Vector2(uvs[i].x / uvSettings.scale.x, uvs[i].y / uvSettings.scale.y);
			else
				uvs[i] = new Vector2(zeroed.x / uvSettings.scale.x, zeroed.y / uvSettings.scale.y) + cen;
			
			cen2 += uvs[i];
		}

		cen = cen2/(float)len;
		
		for(int i = 0; i < len; i++)
			uvs[i] = uvs[i].RotateAroundPoint(cen, uvSettings.rotation);
		
		for(int i = 0; i < len; i++)
		{
			float u = uvs[i].x, v = uvs[i].y;
			
			if(uvSettings.flipU)
				u = -u;

			if(uvSettings.flipV)
				v = -v;

			if(!uvSettings.swapUV)
				uvs[i] = new Vector2(u, v);
			else
				uvs[i] = new Vector2(v, u);
		}

		return uvs;
	}

#region UTILITY

	private static Vector2[] StretchUVs(Vector2[] uvs)
	{
		Vector2 smallest = SmallestVector2(uvs);
		Vector2 mag = LargestVector2(uvs);

		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallest;	// zero off
			uvs[i] = new Vector2(uvs[i].x/mag.x, uvs[i].y/mag.y);
		}
		return uvs;
	}

	private static Vector2[] OffsetUVs(Vector2[] uvs, Vector2 offset)
	{
		for(int i = 0; i < uvs.Length; i++)
			uvs[i] += offset;
		return uvs;
	}

	/*
	 *	Returns normalized UV values for a mesh uvs (0,0) - (1,1)
	 */
	private static Vector2[] NormalizeUVs(Vector2[] uvs)
	{
		/*
		 *	how this works -
		 *		- shift uv coordinates such that the lowest value x and y coordinates are zero
		 *		- scale non-zeroed coordinates uniformly to normalized values (0,0) - (1,1)
		 */

		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		float scale = LargestFloatInVector2Array(uvs);

		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] /= scale;
		}

		return uvs;
	}

	private static Vector2[] JustifyUVs(Vector2[] uvs, pb_UV.Justify j)
	{
		Vector2 amt = new Vector2(0f, 0f);
		switch(j)
		{
			case pb_UV.Justify.Left:
				amt = new Vector2(SmallestVector2(uvs).x, 0f);
				break;
			case pb_UV.Justify.Right:
				amt = new Vector2(LargestVector2(uvs).x - 1f, 0f);
				break;
			case pb_UV.Justify.Top:
				amt = new Vector2(0f, LargestVector2(uvs).y - 1f);
				break;
			case pb_UV.Justify.Bottom:
				amt = new Vector2(0f, SmallestVector2(uvs).y);
				break;
			case pb_UV.Justify.Center:
				amt = pb_Math.Average(uvs) - (new Vector2(.5f, .5f));
				break;
		}

		for(int i = 0; i < uvs.Length; i++)
			uvs[i] -= amt;
	
		return uvs;
	}

	private static Vector2[] ShiftToPoint(Vector2[] uvs, Vector2 point)
	{
		Vector2 offset = point - SmallestVector2(uvs);
		return OffsetUVs(uvs, offset);
	}

	private static Vector2[] ShiftToPositive(Vector2[] uvs)
	{
		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		return uvs;
	}

	private static Vector2 SmallestVector2(Vector2[] v)
	{
		Vector2 s = v[0];
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x < s.x)
				s.x = v[i].x;
			if(v[i].y < s.y)
				s.y = v[i].y;
		}
		return s;
	}

	public static Vector2 LargestVector2(Vector2[] v)
	{
		Vector2 l = v[0];
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x > l.x)
				l.x = v[i].x;
			if(v[i].y > l.y)
				l.y = v[i].y;
		}
		return l;
	}

	private static Vector2 RotateUVs(Vector2 originalUVRotation, float angleChange)
	{
		float c = Mathf.Cos(angleChange*Mathf.Deg2Rad);
		float s = Mathf.Sin(angleChange*Mathf.Deg2Rad);
		Vector2 finalUVRotation = new Vector2(originalUVRotation.x*c - originalUVRotation.y*s, originalUVRotation.x*s + originalUVRotation.y*c);
		return finalUVRotation;
	}

	/**
	 * Returns largest value (either X or Y) in an array of Vector2
	 */
	public static float LargestFloatInVector2Array(Vector2[] v)
	{
		float l = v[0].x;
		for(int i = 0; i < v.Length; i++)
		{
			if(v[i].x > l)
				l = v[i].x;
			if(v[i].y > l)
				l = v[i].y;
		}
		return l;
	}
#endregion

}