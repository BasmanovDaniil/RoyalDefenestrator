using UnityEngine;
using System.Collections;

namespace ProBuilder2.MeshOperations
{
public static class pbTriangleOps
{

	/**
	 * \brief Flips the winding order for the entire mesh. 
	 */
	// public static void ReverseWindingOrder(this pb_Object pb)
	// {
	// 	for(int i = 0; i < pb.faces.Length; i++)
	// 		pb.faces[i].ReverseIndices();
	
	// 	pb.ToMesh();
	// 	pb.Refresh();
	// }	

	/**
	 *	\brief Reverse the winding order for each passed #pb_Face.
	 *	@param faces The faces to apply normal flippin' to.
	 *	\returns Nothing.  No soup for you.
	 *	\sa SelectedFaces pb_Face
	 */
	public static void ReverseWindingOrder(this pb_Object pb, pb_Face[] faces)
	{
		for(int i = 0; i < faces.Length; i++)
			faces[i].ReverseIndices();

		pb.ToMesh();
		pb.Refresh();
	}	

}
}