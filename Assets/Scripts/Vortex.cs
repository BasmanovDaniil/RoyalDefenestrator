using System.Collections;
using UnityEngine;

public class Vortex : MonoBehaviour
{
    public float force = 1000;
    public Queen queen;
    public Storyteller storyteller;
    public Guard[] guardList;

    private Transform tr;

	void Start ()
	{
	    tr = transform;
	}

    IEnumerator DestroyOther(GameObject other)
    {
        yield return new WaitForSeconds(3);
        Destroy(other);
    }

    IEnumerator Play(AudioSource source)
    {
        yield return new WaitForSeconds(1);
        source.Play();
    }

    IEnumerator BadEnding()
    {
        yield return new WaitForSeconds(1);
        storyteller.BadEnding();
    }

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(DestroyOther(other.gameObject));
        if (other.tag == "Victim")
        {
            queen.KillVictim(other.transform);
            if (other.name == "Cat")
            {
                queen.catKilled = true;
                storyteller.catKilled = true;
            }
        }
        if (other.tag == "Queen")
        {
            StartCoroutine(storyteller.GoodEndingAlt());
        }

        if (other.tag == "Cat" || other.name == "Cat")
        {
            queen.catKilled = true;
            storyteller.catKilled = true;
        }
        if (other.tag == "Guard")
        {
            storyteller.guardCount--;
        }
        if (other.tag == "Page")
        {
            StartCoroutine(BadEnding());
        }
        if (other.tag == "FirstVictim")
        {
            storyteller.firstVictimDead = true;
        }
        if (other.tag == "AdmireVictim")
        {
            queen.KillAdmire();
        }
    }

    void OnTriggerStay(Collider other)
    {
        other.rigidbody.AddForce(-tr.forward * force);
    }
}
