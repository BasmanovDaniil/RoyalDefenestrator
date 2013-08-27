using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private bool working = true;
	
	void OnTriggerEnter (Collider other)
	{
	    if (!working) return;
	    if (other.tag == "Queen")
	    {
            StartCoroutine(other.GetComponent<Queen>().Finish());
	        working = false;
	    }
	}
}
