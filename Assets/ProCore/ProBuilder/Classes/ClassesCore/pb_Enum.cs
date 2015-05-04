using UnityEngine;
using System.Collections;

/**
 * Contains runtime enumerators.
 */
namespace ProBuilder2.Common
{
	/**
	 * Determines what GameObject flags this object will have.
	 */
	public enum EntityType {
		Detail,
		Occluder,
		Trigger,
		Collider,
		Mover
	}

	/**
	 * Deprecated.
	 */
	public enum ColliderType {
		None,
		BoxCollider,
		MeshCollider
	}

	/**
	 * Used to generate geo.
	 */
	public enum Shape {
		Cube,
		Stair,
		Prism,
		Cylinder,
		Plane,
		Door,
		Pipe,
		Cone,
		Sprite,
		Arch,
		Custom
	}

	// !-- Todo: Replace the various other Axis enums with this
	public enum Axis {
		Right,
		Left,
		Up,
		Down,
		Forward,
		Backward
	}

	/**
	 * Unused.
	 */
	public enum UV2Method {
		Unity,
		BinPack
	}
}