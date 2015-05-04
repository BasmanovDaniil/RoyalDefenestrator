﻿/**
 *	Used to describe split face actions.
 */
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public struct EdgeConnection : System.IEquatable<EdgeConnection>
	{
		public EdgeConnection(pb_Face face, List<pb_Edge> edges)
		{
			this.face = face;
			this.edges = edges;
		}

		public pb_Face face;
		public List<pb_Edge> edges;	// IMPORTANT - these edges may not be local to the specified face - always use face.edges.IndexOf(edge, sharedIndices) to get the actual edges

		public bool isValid {
			get { return edges != null && edges.Count > 1; }
		}

		public override bool Equals(System.Object b)
		{
			return b is EdgeConnection ? this.face == ((EdgeConnection)b).face : false;
		}

		public bool Equals(EdgeConnection fc)
		{
			return this.face == fc.face;
		}

		public static explicit operator pb_Face(EdgeConnection fc)
		{
			return fc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + edges.ToFormattedString(", ");
		}
	}
}