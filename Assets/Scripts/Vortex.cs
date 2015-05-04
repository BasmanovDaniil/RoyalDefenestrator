using UnityEngine;

public class Vortex : MonoBehaviour
{
    public float force = 3000;

    private Transform tr;

    private void Start()
    {
        tr = transform;
    }

    private void OnTriggerStay(Collider other)
    {
        other.GetComponent<Rigidbody>().AddForce(-tr.forward*force);
    }
}