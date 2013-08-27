using System.Collections;
using UnityEngine;

public class Fireplace : MonoBehaviour
{
    public Queen queen;

    private Transform tr;

	void Start ()
	{
	    tr = transform;
	}

    IEnumerator DestroyOther(GameObject other)
    {
        yield return new WaitForSeconds(2);
        Destroy(other);
    }

    IEnumerator Play(AudioSource source)
    {
        yield return new WaitForSeconds(1);
        source.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trowable")
        {
            other.gameObject.layer = 0;
            StartCoroutine(Play(other.audio));
            StartCoroutine(DestroyOther(other.gameObject));
        }
        if (other.tag == "Victim")
        {
            queen.KillVictim(other.transform);
            StartCoroutine(Play(other.audio));
            StartCoroutine(DestroyOther(other.gameObject));
        }
    }
}
