/*
 *	UV Settings for ProBuilder Objects
 */
using UnityEngine;

[System.Serializable]
/**
 *	\brief Container for UV mapping parameters per face.
 */
public class pb_UV {

#region ENUM

	public enum ProjectionAxis {
		Planar_X,	// projects on x axis
		Planar_Y,	// projects on y axis 
		Planar_Z,	// projects on z axis
		AUTO,		// uses the plane normal
		Planar_Y_Negative 
	}

	public enum Justify {
		Right,
		Left,
		Top,
		Center,
		Bottom,
		None
	}

	public enum Fill {
		Normalize,
		Tile,
		Stretch
	}

#endregion

#region MEMBERS

	public ProjectionAxis 	projectionAxis;		///< Which axis should be used to project UVs from.
	public bool 			useWorldSpace;		///< If true, UV coordinates are calculated using world points instead of local.
	public bool 			flipU;				///< If true, the U value will be inverted.
	public bool 			flipV;				///< If true, the V value will be inverted.
	public bool 			swapUV;				///< If true, U and V values will switched.
	public Fill 			fill;				///< Which Fill mode to use. 
	public Vector2 			scale;				///< The scale to be applied to U and V coordinates.
	public Vector2 			offset;				///< The offset to be applied to the UV coordinates.
	public float 			rotation;			///< Rotates UV coordinates.
	public Justify 			justify;			///< Aligns UVs to the edges or center.
	public Vector3			projectionValue;	///< If projectionAxis is set to Instance, this is the value that will be used to project UV coordinates.
#endregion

#region INITIALIZATION

	public pb_UV()
	{
		projectionAxis = ProjectionAxis.AUTO;
		useWorldSpace = false;
		justify = Justify.None;
		flipU = false;
		flipV = false;
		swapUV = false;
		fill = Fill.Tile;
		scale = new Vector2(1f, 1f);
		offset = new Vector2(0f, 0f);
		rotation = 0f;
	}

	public pb_UV(pb_UV uvs)
	{
		projectionAxis = uvs.projectionAxis;
		useWorldSpace = uvs.useWorldSpace;
		flipU = uvs.flipU;
		flipV = uvs.flipV;
		swapUV = uvs.swapUV;
		fill = uvs.fill;
		scale = uvs.scale;
		offset = uvs.offset;
		rotation = uvs.rotation;
		justify = uvs.justify;
	}

	public pb_UV(
		ProjectionAxis	_projectionAxis,
		bool 			_useWorldSpace,
		bool 			_flipU,
		bool 			_flipV,
		bool 			_swapUV,
		Fill 			_fill,
		Vector2 		_scale,
		Vector2 		_offset,
		float 			_rotation,
		Justify 		_justify
		)
	{
		projectionAxis 	= _projectionAxis;
		useWorldSpace	= _useWorldSpace;
		flipU			= _flipU;
		flipV			= _flipV;
		swapUV			= _swapUV;
		fill			= _fill;
		scale			= _scale;
		offset			= _offset;
		rotation		= _rotation;
		justify			= _justify;
	}

#endregion

#region CONSTANT
	
	public static pb_UV LightmapUVSettings = new pb_UV(
		ProjectionAxis.AUTO,		// projectionAxis
		true,						// useWorldSpace -- we want to retain relative scale
		false,						// flipU			
		false,						// flipV			
		false,						// swapUV			
		Fill.Normalize,				// fill			
		new Vector2(1f, 1f),		// scale			
		new Vector2(0f, 0f),		// offset			
		0f,							// rotation		
		Justify.None);				// justify			

#endregion

#region DEBUG

	public override string ToString()
	{
		string str = "Axis: " + projectionAxis + "\n" +
			"Use World Space: " + useWorldSpace + "\n" +
			"Flip U: " + flipU + "\n" +
			"Flip V: " + flipV + "\n" +
			"Swap UV: " + swapUV + "\n" +
			"Fill Mode: " + fill + "\n" +
			"Justification: " + justify + "\n" +
			"Scale: " + scale + "\n" +
			"Offset: " + offset + "\n" +
			"Rotation: " + rotation + "\n";
		return str;
	}

#endregion

}
