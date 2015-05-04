using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Transform[] victimList;

    private bool working = true;
    private System.Random random;

    private void Start()
    {
        random = new System.Random();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!working) return;
        if (other.tag == "Queen")
        {
            var victim = victimList[random.Next(victimList.Length)];
            victim.tag = "Victim";
            other.GetComponent<Queen>().SetVictim(victim);
            working = false;
        }
    }
}