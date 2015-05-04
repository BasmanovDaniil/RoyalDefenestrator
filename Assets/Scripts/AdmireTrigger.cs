using UnityEngine;

public class AdmireTrigger : MonoBehaviour
{
    public Transform victim;

    private bool working = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!working) return;
        if (other.tag == "Queen")
        {
            victim.tag = "AdmireVictim";
            StartCoroutine(other.GetComponent<Queen>().Admire(victim));
            working = false;
        }
    }
}