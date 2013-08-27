using System.Collections;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public int radius = 7;
    public LayerMask characterLayer;
    public bool grabbed;
    public Head head;
    public SpeechBubble speechBubble;

    private Transform tr;
    private Rigidbody rb;
    private Collider[] characters;
    private System.Random random;
    private string[] panicList;
    private string[] purrList;

    void Start ()
	{
	    tr = transform;
        rb = rigidbody;

        random = new System.Random();
        panicList = new[] { "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "HEY YOU! PUT ME DOWN!" };
        purrList = new[] {"Mew", "Purrr"};
        StartCoroutine(Purr());
	}

    public IEnumerator Panic()
    {
        yield return new WaitForSeconds(1);
        while (grabbed)
        {
            speechBubble.SetText(panicList[random.Next(panicList.Length)]);
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
        }
    }

    public IEnumerator Purr()
    {
        while (true)
        {
            if (!grabbed)
            {
                speechBubble.SetText(panicList[random.Next(panicList.Length)]);
                yield return new WaitForSeconds(3);
                speechBubble.SetText("");
            }
            yield return new WaitForSeconds(3);
        }
    }

    void FixedUpdate()
    {
        if (head == null) return;
        if(head.target != null)
        {
            if ((head.target.position - tr.position).sqrMagnitude > 100)
            {
                head.target = null;
            }
        }

        characters = Physics.OverlapSphere(tr.position, radius, characterLayer);
        if(characters.Length > 1)
        {
            head.target = characters[1].transform;
        }
        
    }
}
