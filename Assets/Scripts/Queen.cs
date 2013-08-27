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
    public Transform page;
    public Guard guardOne;
    public Guard guardTwo;
    public Guard[] guardList;
    public SpeechBubble speechBubble;
    public GameObject indicatorPrefab;
    public bool walking = true;
    public bool grabbed;
    public bool catKilled;
    public Storyteller storyteller;
    [HideInInspector]
    public Head head;

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

	void Start ()
    {
        random = new System.Random();
        panicList = new[] { "Alarm!", "Guards!", "Help me!", "Aaaaah!", "Whyyyyy?", "Don't throw meeee!", "Put me down!", "Please! Nooo!" };
        okList = new[] { "OK, never mind", "*AHEM*", "Fine, lets go", "...", "Whatever" };
        tr = transform;
	    rb = rigidbody;
        seeker = GetComponent<Seeker>();
	    head = GetComponent<Head>();

        seeker.pathCallback += OnPathComplete;

        if (targetList.Length != 0)
        {
            SetTarget(targetList[0]);
        }
	    StartCoroutine(UpdatePath());
    }

	void FixedUpdate ()
	{
        if (grabbed) return;
	    if (!walking) return;
	    if (path == null) return;
	    if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance*targetDistance)
	    {
	        direction = path.vectorPath[currentWaypoint] - tr.position;
	        direction.y = 0;
	        direction = direction.normalized;

            if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
            {
                tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up), 10 * Time.deltaTime);
            }
            rb.AddForce(tr.forward * moveSpeed * Time.deltaTime);

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
        if (grabbed) yield break;

        walking = false;

        admire = admireTransform;
        
        if (!admireKilled)
        {
            speechBubble.SetText("What is this?");
            head.target = admire;
            guardOne.head.target = admire;
            guardTwo.head.target = admire;
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
        if (!admireKilled)
        {
            speechBubble.SetText("What a nice vase!");
            walking = true;
            head.target = null;
            guardOne.head.target = null;
            guardTwo.head.target = null;
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
        if (grabbed) yield break;

        walking = false;

        head.target = page;
        guardOne.head.target = page;
        guardTwo.head.target = page;

        speechBubble.SetText("What is this?");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("Oh! My glasses!");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("Now THAT is much better");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("");
        yield return new WaitForSeconds(3);
        speechBubble.SetText("Where is my cat?");
        yield return new WaitForSeconds(3);
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
        if (grabbed) return;
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
        head.target = victim;
        guardOne.head.target = victim;
        guardTwo.head.target = victim;
        StartCoroutine(Shout());
    }

    public void KillVictim(Transform victimTransform)
    {
        if (victim == victimTransform && !grabbed)
        {
            killed = true;
            head.target = null;
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
            guard.head.target = page;
            guard.victim = page;
        }
    }

    public void CallGuards()
    {
        guardOne.followQueen = true;
        guardOne.walking = true;
        guardOne.head.target = null;
        guardOne.victim = null;
        guardTwo.followQueen = true;
        guardTwo.walking = true;
        guardTwo.head.target = null;
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
        if (!killed)
        {
            speechBubble.SetText("What is this?!"); 
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("I said, what is this?!");
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("Tell me!");
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("Enough!");
            yield return new WaitForSeconds(2);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("Guards!");
            yield return new WaitForSeconds(1);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("");
            head.target = page;
            guardOne.followQueen = false;
            guardOne.head.target = page;
            guardOne.victim = page;

            guardTwo.followQueen = false;
            guardTwo.head.target = page;
            guardTwo.victim = page;
            yield return new WaitForSeconds(10);
        }
        else
        {
            yield break;
        }
        if (!killed)
        {
            speechBubble.SetText("Alarm!");
            Alarm();
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
        }
    }

    public IEnumerator Panic()
    {
        speechBubble.SetText("");
        killed = true;
        yield return new WaitForSeconds(1);
        speechBubble.SetText("Alarm!");
        Alarm();
        yield return new WaitForSeconds(3);
        while (grabbed)
        {
            random = new System.Random();
            speechBubble.SetText(panicList[random.Next(panicList.Length)]);
            yield return new WaitForSeconds(3);
            speechBubble.SetText("");
        }
    }
}
