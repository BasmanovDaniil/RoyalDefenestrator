using UnityEngine;

public class Head : MonoBehaviour
{
    public Transform host;
    public Transform head;
    public Transform target;
    public bool grabbed;

    private Vector3 toTarget;

	void FixedUpdate ()
	{
	    if (grabbed) return;
	    if (head == null) return;
	    if (target != null)
	    {
	        toTarget = target.position - head.position;
	        if (Vector3.Angle(head.forward, target.position - head.position) > 3)
	        {
                toTarget.y = 1;
	            head.forward = Vector3.Slerp(head.forward, toTarget, 10*Time.deltaTime);
	        }
            if (Vector3.Angle(host.forward, target.position - head.position) > 60)
            {
                toTarget.y = 0;
                host.forward = Vector3.Slerp(host.forward, toTarget, 3 * Time.deltaTime);
            }
	    }
	    else
	    {
            if (Vector3.Angle(head.forward, host.forward) > 3)
            {
                head.forward = Vector3.Slerp(head.forward, host.forward, 10 * Time.deltaTime);
            }
	    }
	}
}