using Pathfinding;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Seeker))]
public class Guard : MonoBehaviour
{
    public Transform[] targetList;
    public float nextWaypointDistance = 5;
    public float targetDistance = 2f;
    public int moveSpeed = 15000;
    public int throwForce = 3000;
    public Transform page;
    public Transform queen;
    public Vector3 behindQueen;
    public LayerMask characterLayer;
    public LayerMask windowLayer;
    public Guard[] guardList;
    public SpeechBubble speechBubble;
    public Transform target;
    public Transform head;

    public bool walking = false;
    public bool grounded = true;
    public bool followQueen = false;
    public Transform victim;

    private Transform tr;
    private Rigidbody rb;
    private Seeker seeker;
    private Path path;
    private int currentTarget;
    private int currentWaypoint;
    private int collisions;
    private Vector3 direction;
    private Vector3 toTarget;
    private bool calculatingPath;
    private Collider[] throwables;
    private Rigidbody item;
    private Transform window;
    private System.Random random;
    private string[] panicList;
    private Vector3 newForward;

    void Start()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
        seeker = GetComponent<Seeker>();

        random = new System.Random((int) tr.position.x);
        panicList = new[] { "Nooo!", "Put me down!", "Help me!", "Aaaaah!", "Whyyyyy?", "Don't throw meeee!", "Please! Nooo!" };

        seeker.pathCallback += OnPathComplete;

        StartCoroutine(UpdateVictimPath());

        if (targetList.Length != 0)
        {
            SetTarget(targetList[0]);
        }
    }

    void FixedUpdate()
    {
        if (!grounded) return;
        newForward = tr.forward;
        
        if (walking)
        {
            if (followQueen)
            {
                direction = (queen.position + Quaternion.LookRotation(queen.forward, Vector3.up) * behindQueen) - tr.position;
                direction.y = 0;
                if (direction.sqrMagnitude > targetDistance*targetDistance)
                {
                    if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
                    {
                        newForward += direction.normalized;
                    }
                    rb.AddForce(direction.normalized * moveSpeed * Time.deltaTime);
                }
            }
            else if (victim != null && path != null)
            {
                if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance * targetDistance)
                {
                    direction = path.vectorPath[currentWaypoint] - tr.position;
                    direction.y = 0;

                    if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
                    {
                        newForward += direction.normalized;
                    }
                    rb.AddForce(direction.normalized * moveSpeed * Time.deltaTime);

                    if ((tr.position - path.vectorPath[currentWaypoint]).sqrMagnitude <
                        nextWaypointDistance * nextWaypointDistance && currentWaypoint < path.vectorPath.Count - 1)
                    {
                        currentWaypoint++;
                    }
                }
                else
                {
                    path = null;
                    direction = Vector3.zero;
                    throwables = Physics.OverlapSphere(tr.position + tr.forward, 2, characterLayer);
                    foreach (var throwable in throwables)
                    {
                        if (throwable.GetComponent<Rigidbody>().tag == "Page")
                        {
                            item = throwable.GetComponent<Rigidbody>();
                            item.useGravity = false;
                            item.isKinematic = true;
                            item.GetComponent<Page>().grabbed = true;
                            target = null;
                            foreach (var guard in guardList)
                            {
                                guard.victim = null;
                            }
                            item.drag = 0;
                            item.constraints = RigidbodyConstraints.None;
                            var windows = Physics.OverlapSphere(tr.position, 50, windowLayer);
                            foreach (var candidate in windows)
                            {
                                if (window == null)
                                {
                                    window = candidate.transform;
                                }
                                else if ((tr.position - candidate.transform.position).sqrMagnitude <
                                    (tr.position - window.position).sqrMagnitude)
                                {
                                    window = candidate.transform;
                                }
                            }

                            SetTarget(windows[0].transform);
                        }
                    }
                }
            }
            else if (window != null && path != null)
            {
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
                    item.useGravity = true;
                    item.isKinematic = false;
                    item.AddForce((tr.forward + tr.up + Vector3.ClampMagnitude(rb.velocity, 1)) * throwForce);
                    walking = false;
                    item = null;
                    window = null;
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

        if (item != null)
        {
            item.position = tr.position + tr.up * 5 - tr.right;
            item.rotation = tr.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.right);
        }
    }

    public IEnumerator Panic()
    {
        speechBubble.SetText("");
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

    IEnumerator UpdateVictimPath()
    {
        yield return new WaitForSeconds(Random.value);
        while (true)
        {
            if (victim != null)
            {
                if (path != null)
                {
                    seeker.StartPath(path.vectorPath[currentWaypoint], victim.position);
                }
                else
                {
                    seeker.StartPath(tr.position, victim.position);
                }
            }
            yield return new WaitForSeconds(0.5f);
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

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            grounded = true;
            rb.drag = 5;
        }
    }
}
