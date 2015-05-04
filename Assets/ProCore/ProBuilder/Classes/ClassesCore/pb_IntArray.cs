/*	This exists because Unity can't
 *	serialize jaggaed arrays.  Also, it 
 *	has a couple of handy methods that make
 *	dealing with shared vertex indices easier.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if BUGGER
using Parabox.Bugger;
#endif

namespace ProBuilder2.Common {

[System.Serializable]
/**
 *	\brief Used as a substitute for a jagged int array.  
 *	Also contains some ProBuilder specific extensions for 
 *	dealing with jagged int arrays.  Note that this class
 *	exists because Unity does not serialize jagged arrays.
 */
public class pb_IntArray
{
#region Members

	public int[] array;
#endregion

#region Constructor / Operators

	public List<int> ToList()
	{
		return new List<int>(array);
	}

	public pb_IntArray(int[] intArray)
	{
		array = intArray;
	}

	// Copy constructor
	public pb_IntArray(pb_IntArray intArray)
	{
		array = intArray.array;
	}

	public int this[int i]
	{	
		get { return array[i]; }
		set { array[i] = value; }
	}

	public int Length
	{
		get { return array.Length; }
	}

	public static implicit operator int[](pb_IntArray intArr)
	{
		return intArr.array;
	}

	public static implicit operator pb_IntArray(int[] arr)
	{
		return new pb_IntArray(arr);
	}
#endregion

	public override string ToString()
	{
		string str = "";
		for(int i = 0; i < array.Length - 1; i++)
			str += array[i] + ", ";
		str += array[array.Length-1];

		return str;
	}
	
	public bool IsEmpty()
	{
		return (array == null || array.Length < 1);
	}

	public static void RemoveEmptyOrNull(ref pb_IntArray[] val)
	{
		List<pb_IntArray> valid = new List<pb_IntArray>();
		foreach(pb_IntArray par in val)
		{
			if(par != null && !par.IsEmpty())
				valid.Add(par);
		}
		val = valid.ToArray();
	}
}	

public static class pb_IntArrayUtility
{
	// Returns a jagged int array
	public static int[][] ToArray(this pb_IntArray[] val)
	{
		int[][] arr = new int[val.Length][];
		for(int i = 0; i < arr.Length; i++)
			arr[i] = val[i].array;
		return arr;
	}

	public static List<List<int>> ToList(this pb_IntArray[] val)
	{
		List<List<int>> l = new List<List<int>>();
		for(int i = 0; i < val.Length; i++)
			l.Add( val[i].ToList() );
		return l;
	}

	public static string ToFormattedString(this pb_IntArray[] arr)
	{
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < arr.Length; i++)
			sb.Append( "[" + arr[i].array.ToFormattedString(", ") + "] " );
		
		return sb.ToString();
	}

	/**
	 * Checks if an array contains a value and also compares shared indices using sharedIndices.
	 */
	public static int IndexOf(this int[] array, int val, pb_IntArray[] sharedIndices)
	{
		int indInShared = sharedIndices.IndexOf(val);
		if(indInShared < 0) return -1;

		int[] allValues = sharedIndices[indInShared];

		for(int i = 0; i < array.Length; i++)
			if(System.Array.IndexOf(allValues, array[i]) > -1)
				return i;

		return -1;
	}

	// Scans an array of pb_IntArray and returns the index of that int[] that holds the index.
	// Aids in removing duplicate vertex indices.
	public static int IndexOf(this pb_IntArray[] intArray, int index)
	{
		if(intArray == null) return -1;

		for(int i = 0; i < intArray.Length; i++)
		{
			// for some reason, this is about 2x faster than System.Array.IndexOf
			for(int n = 0; n < intArray[i].Length; n++)
				if(intArray[i][n] == index)
					return i;
		}
		return -1;
	}

	// Returns all shared vertices with input of index array
	public static int[] AllIndicesWithValues(this pb_IntArray[] pbIntArr, int[] indices)
	{
		List<int> used = new List<int>();
		List<int> shared = new List<int>();
		for(int i = 0; i < indices.Length; i++)
		{
			int indx = pbIntArr.IndexOf(indices[i]);
			if(used.Contains(indx))
				continue;
			shared.AddRange(pbIntArr[indx].array);
			used.Add(indx);
		}

		return shared.Distinct().ToArray();
	}

	/**
	 *	Given triangles, this returns a distinct array containing the first value of each sharedIndex array entry.
	 */
	public static int[] UniqueIndicesWithValues(this pb_IntArray[] pbIntArr, int[] values)
	{
		List<int> unique = new List<int>(values);
		
		for(int i = 0; i < unique.Count; i++)
			unique[i] = pbIntArr[pbIntArr.IndexOf(values[i])][0];

		return unique.Distinct().ToArray();
	}

#region ArrayUtil

	/**
	 *	Associates all passed indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public static int MergeSharedIndices(ref pb_IntArray[] sharedIndices, int[] indices)
	{	
		if(indices.Length < 2) return -1;
		if(sharedIndices == null)
		{
			sharedIndices = new pb_IntArray[1] { (pb_IntArray)indices };
			return 0;
		}

		List<int> used = new List<int>();
		List<int> newSharedIndex = new List<int>();

		// Create a new int[] composed of all indices in shared selection
		for(int i = 0; i < indices.Length; i++)
		{
			int si = sharedIndices.IndexOf(indices[i]);
			if(!used.Contains(si))
			{
				if( si > -1 )
				{
					newSharedIndex.AddRange( sharedIndices[si].array );
					used.Add(si);
				}
				else
				{
					newSharedIndex.Add( indices[i] );
				}
				
			}
		}

		// Now remove the old entries
		int rebuiltSharedIndexLength = sharedIndices.Length - used.Count;
		pb_IntArray[] rebuild = new pb_IntArray[rebuiltSharedIndexLength];
		
		int n = 0;
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			if(!used.Contains(i))
				rebuild[n++] = sharedIndices[i];
		}

		sharedIndices = rebuild.Add( new pb_IntArray(newSharedIndex.ToArray()) );
		// SetSharedIndices( rebuild.Add( new pb_IntArray(newSharedIndex.ToArray()) ) );

		return sharedIndices.Length-1;
	}

	/**
	 *	Associates indices with a single shared index.  Does not perfrom any additional operations 
	 *	to repair triangle structure or vertex placement.
	 */
	public static void MergeSharedIndices(ref pb_IntArray[] sharedIndices, int a, int b)
	{
		int aIndex = sharedIndices.IndexOf(a);
		int oldBIndex = sharedIndices.IndexOf(b);
	
		pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, aIndex, b);

		int[] arr = sharedIndices[oldBIndex].array;
		sharedIndices[oldBIndex].array = arr.RemoveAt(System.Array.IndexOf(arr, b));
		pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
	}	

	/**
	 * Add a value to the array at index.
	 */
	public static int AddValueAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int value)
	{
		if(sharedIndex > -1)
			sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.Add(value);
		else
			sharedIndices = (pb_IntArray[])sharedIndices.Add( new pb_IntArray(new int[]{value}) );
		
		return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
	}

	/**
	 * Adds a range of values to the array at index.
	 */
	public static int AddRangeAtIndex(ref pb_IntArray[] sharedIndices, int sharedIndex, int[] indices)
	{
		if(sharedIndex > -1)
			sharedIndices[sharedIndex].array = sharedIndices[sharedIndex].array.AddRange(indices);
		else
			sharedIndices = (pb_IntArray[])sharedIndices.Add( new pb_IntArray(indices) );
		
		return sharedIndex > -1 ? sharedIndex : sharedIndices.Length-1;
	}

	/**
	 * Removes all passed values from the sharedIndices jagged array. Does NOT perform any
	 * index shifting to account for removed vertices.  Use RemoveValuesAndShift
	 * for that purpose.
	 */
	public static void RemoveValues(ref pb_IntArray[] sharedIndices, int[] remove)
	{
		// remove face indices from all shared indices caches
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			for(int n = 0; n < remove.Length; n++)
			{
				int ind = System.Array.IndexOf(sharedIndices[i], remove[n]);

				if(ind > -1)
					sharedIndices[i].array = sharedIndices[i].array.RemoveAt(ind);
			}
		}

		// Remove empty or null entries caused by shifting around all them indices
		pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
	}



	/**
	 *	\brief Removes the specified indices from the array, and shifts all values 
	 *	down to account for removal in the vertex array.  Only use when deleting
	 *	faces or vertices.  For general moving around and modification of shared 
	 *	index array, use #RemoveValuesAtIndex.
	 */
	public static void RemoveValuesAndShift(ref pb_IntArray[] sharedIndices, int[] remove)
	{
		// MUST BE DISTINCT
		remove = remove.ToDistinctArray();

		// remove face indices from all shared indices caches
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			for(int n = 0; n < remove.Length; n++)
			{
				int ind = System.Array.IndexOf(sharedIndices[i], remove[n]);

				if(ind > -1)
					sharedIndices[i].array = sharedIndices[i].array.RemoveAt(ind);
			}
		}

		// Remove empty or null entries caused by shifting around all them indices
		pb_IntArray.RemoveEmptyOrNull(ref sharedIndices);
		
		// now cycle through and shift indices
		for(int i = 0; i < sharedIndices.Length; i++)
		{
			for(int n = 0; n < sharedIndices[i].Length; n++)
			{
				int ind = sharedIndices[i][n];
				int sub = 0;

				// use a count and subtract at end because indices aren't guaranteed to be in order.
				// ex, 9, 8, 7 would only sub 1 if we just did rm < ind; ind--
				foreach(int rm in remove)
				{
					if(rm < ind)
						sub++;
				}
				sharedIndices[i][n] -= sub;
			}
		}
	}	
#endregion
}
}