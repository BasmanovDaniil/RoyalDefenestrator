using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;

    private Transform tr;
    private Vector3 initialPosition;
	
	void Start ()
	{
	    tr = transform;
	}
	
	void LateUpdate ()
    {
        if (target != null)
        {
            tr.position = target.position + cameraPosition;
            tr.rotation = Quaternion.Euler(cameraRotation);
        }
	}
}
