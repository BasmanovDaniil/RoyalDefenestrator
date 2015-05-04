using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Math
{

	public static class pb_Math
	{
		// implementation snagged from: http://stackoverflow.com/questions/839899/how-do-i-calculate-a-point-on-a-circles-circumference
		public static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
		{
			// Convert from degrees to radians via multiplication by PI/180        
			float x = (float)(radius * Mathf.Cos( Mathf.Deg2Rad * angleInDegrees)) + origin.x;
			float y = (float)(radius * Mathf.Sin( Mathf.Deg2Rad * angleInDegrees)) + origin.y;

			return new Vector2(x, y);
		}

		public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}

		public static Vector3 Normal(pb_Object pb, pb_Face face)
		{
			Vector3 p0 = pb.vertices[face.indices[0]];
			Vector3 p1 = pb.vertices[face.indices[1]];
			Vector3 p2 = pb.vertices[face.indices[2]];

			Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}

		public static Vector3 Normal(Vector3[] p)
		{
			Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}

		public static Vector3 Normal(List<Vector3> p)
		{
			Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
			if (cross.magnitude < Mathf.Epsilon)
				return new Vector3(0f, 0f, 0f); // bad triangle
			else
			{
				return cross.normalized;
			}
		}

		public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
		{
			float da = Vector3.Distance(a, b);
			float db = Vector3.Distance(b, c);
			float dc = Vector3.Distance(c, a);
			float p = (da + db + dc) / 2f;

			return Mathf.Sqrt( p*(p-da)*(p-db)*(p-dc) );

		}

		public static Vector3 NearestPointOnEdge(Vector3 pt1, Vector3 pt2, Vector3 point) 
		{
			Vector3 A = point - pt1;
			Vector3 u = (pt2-pt1).normalized;

			return pt1 + (Vector3.Dot(A, u)) * u;
		}

		public static float LargestValue(Vector3 v)
		{
			if(v.x > v.y && v.x > v.z) return v.x;
			if(v.y > v.x && v.y > v.z) return v.y;
			return v.z;
		}
		
		public static float LargestValue(Vector2 v)
		{
			return (v.x > v.y) ? v.x :v.y;
		}

		/**
		 *	\brief Gets the center point of the supplied Vector3[] array.
		 *	\returns Average Vector3 of passed vertex array.
		 */
		public static Vector3 Average(List<Vector3> v)
		{
			Vector3 sum = Vector3.zero;
			for(int i = 0; i < v.Count; i++)
				sum += v[i];
			return sum/(float)v.Count;
		}

		public static Vector3 Average(Vector3[] v)
		{
			Vector3 sum = Vector3.zero;
			for(int i = 0; i < v.Length; i++)
				sum += v[i];
			return sum/(float)v.Length;
		}

		public static Vector2 Average(List<Vector2> v)
		{
			Vector2 sum = Vector2.zero;
			for(int i = 0; i < v.Count; i++)
				sum += v[i];
			return sum/(float)v.Count;
		}

		public static Vector2 Average(Vector2[] v)
		{
			Vector2 sum = Vector2.zero;
			for(int i = 0; i < v.Length; i++)
				sum += v[i];
			return sum/(float)v.Length;
		}

		/**
		 *	\brief Compares 2 vector3 objects, allowing for a margin of error.
		 */
		public static bool EqualWithError(this Vector3 v, Vector3 b, float delta)
		{
			return 
				Mathf.Abs(v.x - b.x) < delta &&
				Mathf.Abs(v.y - b.y) < delta &&
				Mathf.Abs(v.z - b.z) < delta;
		}

		/**
		 * Returns a new point by rotating the Vector2 around an origin point.
		 * @param v this - Vector2 original point.
		 * @param origin The origin point to use as a pivot point.
		 * @param theta Angle to rotate in Degrees.
		 */
		public static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
		{
			float cx = origin.x, cy = origin.y;	// origin
			float px = v.x, py = v.y;			// point

			float s = Mathf.Sin(theta * Mathf.Deg2Rad);
			float c = Mathf.Cos(theta * Mathf.Deg2Rad);

			// translate point back to origin:
			px -= cx;
			py -= cy;

			// rotate point
			float xnew = px * c + py * s;
			float ynew = -px * s + py * c;

			// translate point back:
			px = xnew + cx;
			py = ynew + cy;
			
			return new Vector2(px, py);
		}

		/**
		 * Scales a Vector2 using origin as the pivot point.
		 */
		public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
		{
			Vector2 tp = v-origin;
			tp = Vector2.Scale(tp, scale);
			tp += origin;
			
			return tp;
		}

		public static Vector2[] VerticesTo2DPoints(Vector3[] verts, Vector3 planeNormal)
		{	
			Vector2[] projected = new Vector2[verts.Length];

			for(int i = 0; i < verts.Length; i++)
			{
				float u, v;
				Vector3 uAxis, vAxis;
				Vector3 vec = Vector3.zero;

				switch(GetProjectionAxis(planeNormal))
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
				
				// get U axis
				uAxis = Vector3.Cross(planeNormal, vec);
				uAxis.Normalize();

				// calculate V axis relative to U
				vAxis = Vector3.Cross(uAxis, planeNormal);
				vAxis.Normalize();

				u = Vector3.Dot(uAxis, verts[i]);
				v = Vector3.Dot(vAxis, verts[i]);

				projected[i] = new Vector2(u, v);
			}

			return projected;
		}

		/**
		 *	Return the perpindicular direction to a 2d line
		 */
		public static Vector2 Perpendicular(Vector2 a, Vector2 b)
		{
			float x = a.x;
			float y = a.y;

			float x2 = b.x;
			float y2 = b.y;

			return new Vector2( -(y2-y), x2-x ).normalized;
		}

		/**
		 *	Return the perpindicular direction to a unit vector
		 */
		public static Vector2 Perpendicular(Vector2 a)
		{
			return new Vector2(-a.y, a.x).normalized;
		}

		// http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
		// Returns 1 if the lines intersect, otherwise 0. In addition, if the lines 
		// intersect the intersection point may be stored in the floats i.x and i.y.
		public static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
		{
			intersect = Vector2.zero;
			Vector2 s1, s2;
			s1.x = p1.x - p0.x;     s1.y = p1.y - p0.y;
			s2.x = p3.x - p2.x;     s2.y = p3.y - p2.y;

			float s, t;
			s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
			t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);

			if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
			{
				// Collision detected
				intersect.x = p0.x + (t * s1.x);
				intersect.y = p0.y + (t * s1.y);
				return true;
			}

			return false;
		}		

		/*
		 *	Returns a projection axis based on which axis is the largest
		 */
		public static pb_UV.ProjectionAxis GetProjectionAxis(Vector3 plane)
		{
			pb_UV.ProjectionAxis p;
			if(Mathf.Abs(plane.x) > Mathf.Abs(plane.y))
				p = pb_UV.ProjectionAxis.Planar_X;
			else
			{
				if(plane.y > 0)
					p = pb_UV.ProjectionAxis.Planar_Y_Negative;
				else
					p = pb_UV.ProjectionAxis.Planar_Y;
			}

			if(Mathf.Abs(plane.z) > Mathf.Abs(plane.y) &&
				Mathf.Abs(plane.z) > Mathf.Abs(plane.x) )
				p = pb_UV.ProjectionAxis.Planar_Z;

			return p;
		}
	}
}