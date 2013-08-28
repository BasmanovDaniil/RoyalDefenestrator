using Pathfinding;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Seeker))]
public class Queen : MonoBehaviour
{
    public Transform[] targetList;
    public float nextWaypointDistance = 2;
    public float targetDistance = 0.5f;
    public int moveSpeed = 300;
    public int turnSpeed = 150;
    public Page page;
    public Guard guardOne;
    public Guard guardTwo;
    public Guard[] guardList;
    public SpeechBubble speechBubble;
    public GameObject indicatorPrefab;
    public bool walking = true;
    public bool grounded = true;
    public bool catKilled;
    public Storyteller storyteller;
    public Transform target;
    public Transform head;

    private Transform tr;
    private Rigidbody rb;
    private Seeker seeker;
    private Path path;
    private int currentTarget;
    private int currentWaypoint;
    private Vector3 direction;
    private bool calculatingPath;
    private Transform victim;
    private bool killed = true;
    private Transform indicator;
    private string[] panicList;
    private string[] okList;
    private System.Random random;
    private Transform admire;
    private bool admireKilled;
    private Vector3 newForward;
    private Vector3 toTarget;

	void Start ()
    {
        random = new System.Random();
        panicList = new[] { "Alarm!", "Guards!", "Help me!", "Aaaaah!", "Whyyyyy?", "Don't throw meeee!", "Put me down!", "Please! Nooo!" };
        okList = new[] { "OK, never mind", "*AHEM*", "Fine, lets go", "...", "Whatever" };
        tr = transform;
	    rb = rigidbody;
        seeker = GetComponent<Seeker>();

        seeker.pathCallback += OnPathComplete;

        if (targetList.Length != 0)
        {
            SetTarget(targetList[0]);
        }
	    StartCoroutine(UpdatePath());
    }

	void FixedUpdate ()
	{
        if (!grounded) return;
        newForward = tr.forward;

	    if (walking)
	    {
            if (path == null) return;
            if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance * targetDistance)
            {
                direction = path.vectorPath[currentWaypoint] - tr.position;
                direction.y = 0;

                if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
                {
                    newForward += direction.normalized;
                }
                rb.AddForce(direction.normalized * moveSpeed * Time.deltaTime);

                if ((tr.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance && currentWaypoint < path.vectorPath.Count - 1)
                {
                    currentWaypoint++;
                }
            }
            else
            {
                path = null;
                direction = Vector3.zero;
                if (currentTarget < targetList.Length - 1)
                {
                    currentTarget++;
                    currentWaypoint = 0;
                    SetTarget(targetList[currentTarget]);
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

    public void SetTarget(Vector3 targetPoint)
    {
        seeker.StartPath(tr.position, targetPoint);
    }

    public void SetTarget(Transform targetTransform)
    {
        SetTarget(targetTransform.position);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
        else
        {
            Debug.Log("Path error");
        }
    }

    void OnDisable()
    {
        seeker.pathCallback -= OnPathComplete;
    }

    public IEnumerator Admire(Transform admireTransform)
    {
        if (!grounded) yield break;

        walking = false;

        admire = admireTransform;
        
        if (!admireKilled)
        {
            speechBubble.SetText("What is this?");
            target = admire;
            guardOne.target = admire;
            guardTwo.target = admire;
            if (indicator == null)
            {
                var clone = Instantiate(indicatorPrefab, admire.position, Quaternion.identity) as GameObject;
                indicator = clone.transform;
                indicator.parent = admire;
            }
            yield return new WaitForSeconds(2);
        }
        else
        {
            speechBubble.SetText("Where is my vase?!");
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
            yield return new WaitForSeconds(2);
            StartCoroutine(Panic());
        }

        if (!grounded) yield break;
        if (!admireKilled)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(1);
        }
        else
        {
            speechBubble.SetText("Where is my vase?!");
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
            yield return new WaitForSeconds(2);
            StartCoroutine(Panic());
        }

        if (!grounded) yield break;
        if (!admireKilled)
        {
            speechBubble.SetText("Oh! My beautiful vase!");
            yield return new WaitForSeconds(3);
        }
        else
        {
            speechBubble.SetText("Where is my vase?!");
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
            yield return new WaitForSeconds(2);
            StartCoroutine(Panic());
        }

        if (!grounded) yield break;
        if (!admireKilled)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);

        }
        else
        {
            speechBubble.SetText("Where is my vase?!");
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
            yield return new WaitForSeconds(2);
            StartCoroutine(Panic());
        }
        if (!grounded) yield break;
        if (!admireKilled)
        {
            speechBubble.SetText("What a nice vase!");
            walking = true;
            target = null;
            guardOne.target = null;
            guardTwo.target = null;
            Destroy(indicator.gameObject);
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
        }
        else
        {
            speechBubble.SetText("Where is my vase?!");
            yield return new WaitForSeconds(2);
            speechBubble.SetText("");
            yield return new WaitForSeconds(2);
            StartCoroutine(Panic());
        }
    }

    public IEnumerator Finish()
    {
        if (!grounded) yield break;

        walking = false;

        target = page.head;
        guardOne.target = page.head;
        guardTwo.target = page.head;
        if (!grounded) yield break;
        speechBubble.SetText("What is this?");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("Oh! My glasses!");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("Now THAT is much better");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        speechBubble.SetText("Where is my cat?");
        yield return new WaitForSeconds(3);
        if (!grounded) yield break;
        if (catKilled)
        {
            StartCoroutine(Panic());
        }
        else
        {
            speechBubble.SetText("There you are!");
            yield return new WaitForSeconds(3);
            storyteller.GoodEnding();
        }
    }

    public void KillAdmire()
    {
        admireKilled = true;
    }

    public void SetVictim(Transform victimTransform)
    {
        if (!grounded) return;
        if (victimTransform == null) return;
        victim = victimTransform;
        walking = false;
        killed = false;

        if (indicator == null)
        {
            var clone = Instantiate(indicatorPrefab, victim.position, Quaternion.identity) as GameObject;
            indicator = clone.transform;
            indicator.parent = victim;
        }
        target = victim;
        guardOne.target = victim;
        guardTwo.target = victim;
        StartCoroutine(Shout());
    }

    public void KillVictim(Transform victimTransform)
    {
        if (victim == victimTransform && grounded)
        {
            killed = true;
            target = null;
            CallGuards();
            StartCoroutine(CalmDown());
        }
    }

    public void Alarm()
    {
        foreach (var guard in guardList)
        {
            guard.followQueen = false;
            guard.walking = true;
            guard.target = page.head;
            guard.victim = page.tr;
        }
    }

    public void CallGuards()
    {
        guardOne.followQueen = true;
        guardOne.walking = true;
        guardOne.target = null;
        guardOne.victim = null;
        guardTwo.followQueen = true;
        guardTwo.walking = true;
        guardTwo.target = null;
        guardTwo.victim = null;
    }

    IEnumerator UpdatePath()
    {
        yield return new WaitForSeconds(Random.value);
        while (true)
        {
            if (walking)
            {
                if (targetList.Length > 0)
                {
                    if (path != null)
                    {
                        seeker.StartPath(path.vectorPath[currentWaypoint], targetList[currentTarget].position);
                    }
                    else
                    {
                        seeker.StartPath(tr.position, targetList[currentTarget].position);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CalmDown()
    {
        speechBubble.SetText("");
        yield return new WaitForSeconds(1);
        speechBubble.SetText(okList[random.Next(okList.Length)]);
        yield return new WaitForSeconds(3);
        speechBubble.SetText("");
        walking = true;
    }

    IEnumerator Shout()
    {
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("What is this?!"); 
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("I said, what is this?!");
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("Tell me!");
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("Enough!");
            yield return new WaitForSeconds(2);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("Guards!");
            yield return new WaitForSeconds(1);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("");
            target = page.head;
            guardOne.followQueen = false;
            guardOne.target = page.head;
            guardOne.victim = page.tr;

            guardTwo.followQueen = false;
            guardTwo.target = page.head;
            guardTwo.victim = page.tr;
            yield return new WaitForSeconds(10);
        }
        else
        {
            walking = true;
            speechBubble.SetText("");
            yield break;
        }
        if (!killed && storyteller.guardCount > 0 && victim != null)
        {
            speechBubble.SetText("Alarm!");
            Alarm();
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
        }
        while (!killed && storyteller.guardCount > 0 && victim != null)
        {
            yield return new WaitForSeconds(1);
        }
        walking = true;
        speechBubble.SetText("");
    }

    public IEnumerator Panic()
    {
        speechBubble.SetText("");
        killed = true;
        yield return new WaitForSeconds(1);
        speechBubble.SetText("Alarm!");
        Alarm();
        yield return new WaitForSeconds(3);
        while (!grounded)
        {
            random = new System.Random();
            speechBubble.SetText(panicList[random.Next(panicList.Length)]);
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
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
