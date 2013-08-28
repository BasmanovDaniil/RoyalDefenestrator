using System.Collections;
using UnityEngine;

public class Shredder : MonoBehaviour
{
    public Queen queen;
    public Storyteller storyteller;

    IEnumerator DestroyOther(GameObject other)
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(other);
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
            if (other.name == "Guard")
            {
                storyteller.guardCount--;
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
        if (other.tag == "Guard" || other.name == "Guard")
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
}
