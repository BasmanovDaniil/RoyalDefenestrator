using UnityEngine;

public class Vortex : MonoBehaviour
{
    public float force = 3000;

    private Transform tr;

	void Start ()
	{
	    tr = transform;
	}

    void OnTriggerStay(Collider other)
    {
        other.GetComponent<Rigidbody>().AddForce(-tr.forward * force);
    }
}
