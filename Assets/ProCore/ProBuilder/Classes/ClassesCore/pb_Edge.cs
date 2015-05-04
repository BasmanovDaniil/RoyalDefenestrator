using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.Common {
[System.Serializable]
public class pb_Edge : System.IEquatable<pb_Edge>
{
	public int x, y;

	public pb_Edge(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public override string ToString()
	{
		return "[" + x + ", " + y + "]";
	}

	public bool Equals(pb_Edge edge)
	{
		return (this.x == edge.x && this.y == edge.y) || (this.x == edge.y && this.y == edge.x);
	}

	public override bool Equals(System.Object b)
	{
		return b is pb_Edge && (this.x == ((pb_Edge)b).x || this.x == ((pb_Edge)b).y) && (this.y == ((pb_Edge)b).x || this.y == ((pb_Edge)b).y);
	}

	public override int GetHashCode()
	{
		// return base.GetHashCode();
		int hashX;
		int hashY;

		if(x < y)
		{
			hashX = x.GetHashCode();
			hashY = y.GetHashCode();	
		}
		else
		{
			hashX = y.GetHashCode();
			hashY = x.GetHashCode();
		}

		//Calculate the hash code for the product. 
		return hashX ^ hashY;
	}

	public int[] ToArray()
	{
		return new int[2] {x, y};
	}

	/**
	 * \brief Compares edges and takes shared triangles into account.
	 * @param a First edge to compare.
	 * @param b Second edge to compare against.
	 * @param sharedIndices A pb_IntArray[] containing int[] of triangles that share a vertex.
	 * \returns True or false if edge a is equal to b.
	 */
	public bool Equals(pb_Edge b, pb_IntArray[] sharedIndices)
	{
		int index = -1;

		index = sharedIndices.IndexOf(x);
		int[] ax = (index > -1) ? sharedIndices[index].array : new int[1]{x};
		
		index = sharedIndices.IndexOf(y);
		int[] ay = (index > -1) ? sharedIndices[index].array : new int[1]{y};

		index = sharedIndices.IndexOf(b.x);
		int[] bx = (index > -1) ? sharedIndices[index].array : new int[1]{b.x};
		
		index = sharedIndices.IndexOf(b.y);
		int[] by = (index > -1) ? sharedIndices[index].array : new int[1]{b.y};

		if( (ax.ContainsMatch(bx) > -1 || ax.ContainsMatch(by) > -1) && (ay.ContainsMatch(bx) > -1 || ay.ContainsMatch(by) > -1) ) 
			return true;
		else
			return false;
	}

	public bool Contains(int a)
	{
		return (x == a || y == a);
	}

	public bool Contains(int a, pb_IntArray[] sharedIndices)
	{
		int ind = sharedIndices.IndexOf(a);
		return ( System.Array.IndexOf(sharedIndices[ind], x) > -1 || System.Array.IndexOf(sharedIndices[ind], y) > -1);
	}

#region static methods

	/**
	 *	Returns new edges where each edge is composed not of vertex indices, but rather the index in pb.sharedIndices of each
	 *	vertex.
	 */
	public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, pb_IntArray[] sharedIndices)
	{
		int len = edges.Length;
		pb_Edge[] uniEdges = new pb_Edge[len];
		for(int i = 0; i < len; i++)
			uniEdges[i] = new pb_Edge(sharedIndices.IndexOf(edges[i].x), sharedIndices.IndexOf(edges[i].y));
		return uniEdges.Distinct().ToArray();
	}

	public static pb_Edge[] AllEdges(pb_Face[] faces)
	{
		List<pb_Edge> edges = new List<pb_Edge>();
		foreach(pb_Face f in faces)
			edges.AddRange(f.edges);
		return edges.ToArray();
	}

	/**
	 *	Simple contains duplicate.  Does NOT account for shared indices
	 */
	public static bool ContainsDuplicateFast(pb_Edge[] edges, pb_Edge edge)
	{
		int c = 0;
		for(int i = 0; i < edges.Length; i++)
		{
			if(edges[i].Equals(edge))
				c++;
		}
		return (c > 1) ? true : false;
	}

	public static Vector3[] VerticesWithEdges(pb_Edge[] edges, Vector3[] vertices)
	{
		Vector3[] v = new Vector3[edges.Length * 2];
		int n = 0;
		for(int i = 0; i < edges.Length; i++)
		{
			v[n++] = vertices[edges[i].x];
			v[n++] = vertices[edges[i].y];
		}
		return v;
	}

#endregion
}

public static class EdgeExtensions
{
	/**
	 *	Checks for duplicates taking sharedIndices into account
	 */
	public static bool ContainsDuplicate(this List<pb_Edge> edges, pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		int c = 0;

		for(int i = 0; i < edges.Count; i++)
		{
			if(edges[i].Equals(edge, sharedIndices))
				if(++c > 1) return true;
		}

		return false;
	}

	public static List<pb_Edge> GetPerimeterEdges(this pb_Object pb, List<pb_Face> faces)
	{
		List<pb_Edge> edges = new List<pb_Edge>();

		for(int i = 0; i < faces.Count; i++)
			edges.AddRange(faces[i].edges);	

		List<pb_Edge> perimeterEdges = new List<pb_Edge>();
		
		for(int i = 0; i < edges.Count; i++)	
		{
			if(edges.ContainsDuplicate(edges[i], pb.sharedIndices))
				continue;
			else
				perimeterEdges.Add(edges[i]);
		}

		return perimeterEdges;
	}

	public static pb_Edge[] GetPerimeterEdges(this pb_Object pb, pb_Face[] faces)
	{
		List<pb_Edge> edges = new List<pb_Edge>();

		for(int i = 0; i < faces.Length; i++)
			edges.AddRange(faces[i].edges);

		List<pb_Edge> perimeterEdges = new List<pb_Edge>();
		
		for(int i = 0; i < edges.Count; i++)	
		{
			if(edges.ContainsDuplicate(edges[i], pb.sharedIndices))
				continue;
			else
				perimeterEdges.Add(edges[i]);
		}

		return perimeterEdges.ToArray();
	}

	public static pb_Edge[] GetPerimeterEdges(this pb_Face face)
	{
		pb_Edge[] edges = face.GetEdges();
		List<pb_Edge> perimeterEdges = new List<pb_Edge>();
		
		for(int i = 0; i < edges.Length; i++)	
		{
			if(pb_Edge.ContainsDuplicateFast(edges, edges[i]))
				continue;
			else
				perimeterEdges.Add(edges[i]);
		}

		return perimeterEdges.ToArray();
	}

	/**
	 *	Returns edges where each edge is guaranteed to be a face perimeter edge, and
	 *	no duplicate edges (checked against sharedIndex array)
	 */
	// todo - this is a farce!
	public static pb_Edge[] GetUniqueEdges(this pb_Object pb, pb_Face[] faces)
	{
		List<pb_Edge> edges = new List<pb_Edge>();

		foreach(pb_Face face in faces)
		{
			foreach(pb_Edge edge in face.edges)
				edges.Add(edge);
		}

		return edges.ToArray();
	}

	/**
	 *	Fast contains - doens't account for shared indices
	 */
	public static bool Contains(this pb_Edge[] edges, pb_Edge edge)
	{
		for(int i = 0; i < edges.Length; i++)
		{
			if(edges[i].Equals(edge))
				return true;
		}

		return false;	
	}

	/**
	 * Slow IndexOf - takes sharedIndices into account when searching the List.
	 */
	public static int IndexOf(this List<pb_Edge> edges, pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		for(int i = 0; i < edges.Count; i++)
		{
			if(edges[i].Equals(edge, sharedIndices))
				return i;
		}

		return -1;	
	}

	public static int IndexOf(this pb_Edge[] edges, pb_Edge edge, pb_IntArray[] sharedIndices)
	{
		for(int i = 0; i < edges.Length; i++)
		{
			if(edges[i].Equals(edge, sharedIndices))
				return i;
		}

		return -1;
	}

	public static List<int> ToIntList(this List<pb_Edge> edges)
	{
		List<int> arr = new List<int>();
		foreach(pb_Edge edge in edges)
		{
			arr.Add( edge.x );
			arr.Add( edge.y );
		}
		return arr;
	}

	public static int[] ToIntArray(this pb_Edge[] edges)
	{
		int[] arr = new int[edges.Length*2];
		int n = 0;

		foreach(pb_Edge edge in edges)
		{
			arr[n++] = edge.x;
			arr[n++] = edge.y;
		}
		return arr;
	}
}
}