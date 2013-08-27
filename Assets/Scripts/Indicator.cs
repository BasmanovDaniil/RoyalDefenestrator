using UnityEngine;

public class Indicator : MonoBehaviour
{
    private Transform tr;

	void Start ()
	{
	    tr = transform;
	}
	
	void Update ()
	{
	    tr.up = Vector3.up;
	}
}
