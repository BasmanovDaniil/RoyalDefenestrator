/**
 *	Used to describe split face actions.
 */
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public struct VertexConnection : System.IEquatable<VertexConnection>
	{
		public VertexConnection(pb_Face face, List<int> indices)
		{
			this.face = face;
			this.indices = indices;
		}

		public pb_Face face;
		public List<int> indices;

		public bool isValid {
			get { return indices != null && indices.Count > 1; }
		}

		public override bool Equals(System.Object b)
		{
			return b is VertexConnection ? this.face == ((VertexConnection)b).face : false;
		}

		public bool Equals(VertexConnection vc)
		{
			return this.face == vc.face;
		}

		public static implicit operator pb_Face(VertexConnection vc)
		{
			return vc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + indices.ToFormattedString(", ");
		}
	}
}