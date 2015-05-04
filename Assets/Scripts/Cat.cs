using System.Collections;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public int radius = 7;
    public LayerMask characterLayer;
    public bool grounded;
    
    public SpeechBubble speechBubble;
    public Transform target;
    public Transform head;

    private Transform tr;
    private Rigidbody rb;
    private Collider[] characters;
    private System.Random random;
    private string[] panicList;
    private string[] purrList;
    private Vector3 newForward;
    private Vector3 toTarget;

    void Start ()
	{
	    tr = transform;
        rb = GetComponent<Rigidbody>();

        random = new System.Random();
        panicList = new[] { "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "Mew", "Purrr", "HEY YOU! PUT ME DOWN!" };
        purrList = new[] {"Mew", "Purrr"};
        StartCoroutine(Purr());
	}

    public IEnumerator Panic()
    {
        yield return new WaitForSeconds(1);
        while (!grounded)
        {
            random = new System.Random((int)tr.position.x);
            speechBubble.SetText(panicList[random.Next(panicList.Length)]);
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
    }

    public IEnumerator Purr()
    {
        while (true)
        {
            if (grounded)
            {
                random = new System.Random((int)tr.position.x);
                speechBubble.SetText(purrList[random.Next(purrList.Length)]);
                yield return new WaitForSeconds(3);
                speechBubble.SetText("");
            }
            yield return new WaitForSeconds(3);
        }
    }

    void FixedUpdate()
    {
        if (!grounded) return;
        newForward = tr.forward;

        characters = Physics.OverlapSphere(tr.position, radius, characterLayer);
        if(characters.Length > 1)
        {
            foreach (var character in characters)
            {
                if (character.name != "Cat")
                {
                    target = character.transform;
                }
            }
        }

        if (target != null)
        {
            toTarget = target.position - head.position;
            if (Vector3.Angle(head.forward, toTarget) > 3)
            {
                head.forward = Vector3.Slerp(head.forward, toTarget, 10 * Time.deltaTime);
            }
            if (Vector3.Angle(tr.forward, toTarget) > 45)
            {
                newForward += toTarget.normalized;
            }
            if ((target.position - tr.position).sqrMagnitude > 100)
            {
                target = null;
            }
        }
        else
        {
            if (Vector3.Angle(head.forward, tr.forward) > 3)
            {
                head.forward = Vector3.Slerp(head.forward, tr.forward, 10 * Time.deltaTime);
            }
        }

        newForward.y = 0;
        if (newForward != tr.forward || tr.up != Vector3.up)
        {
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(newForward, Vector3.up), 5 * Time.deltaTime);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            grounded = true;
            rb.drag = 5;
        }
    }
}
