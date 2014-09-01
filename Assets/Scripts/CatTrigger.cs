using UnityEngine;

public class CatTrigger : MonoBehaviour
{
    public Transform cat;

    private bool working = true;
	
	void OnTriggerEnter (Collider other)
	{
	    if (!working) return;
	    if (other.tag == "Queen")
	    {
            if (Vector3.Distance(transform.position, cat.position) < 15)
            {
                StartCoroutine(other.GetComponent<Queen>().LookAtCat());
            }
	        working = false;
	    }
	}
}
