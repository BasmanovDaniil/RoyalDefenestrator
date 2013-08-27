using Pathfinding;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Seeker))]
public class Guard : MonoBehaviour
{
    public Transform[] targetList;
    public float nextWaypointDistance = 5;
    public float targetDistance = 2f;
    public int moveSpeed = 350;
    public int followSpeed = 300;
    public int throwForce = 3000;
    public Transform page;
    public Transform queen;
    public Vector3 behindQueen;
    public LayerMask characterLayer;
    public LayerMask windowLayer;
    public Guard[] guardList;
    public SpeechBubble speechBubble;

    [HideInInspector]
    public bool walking = false;
    [HideInInspector]
    public bool grabbed = false;
    [HideInInspector]
    public bool followQueen = false;
    [HideInInspector]
    public Transform victim;
    [HideInInspector]
    public Head head;

    private Transform tr;
    private Rigidbody rb;
    private Seeker seeker;
    private Path path;
    private int currentTarget;
    private int currentWaypoint;
    private int collisions;
    private Vector3 direction;
    private bool calculatingPath;
    private Collider[] throwables;
    private Rigidbody item;
    private Transform window;
    private System.Random random;
    private string[] panicList;

    void Start()
    {
        tr = transform;
        rb = rigidbody;
        seeker = GetComponent<Seeker>();
        head = GetComponent<Head>();

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
        if (grabbed) return;
        if (item != null)
        {
            item.position = tr.position + tr.up * 5 - tr.right;
            item.rotation = tr.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.right);
        }
        if (!walking) return;
        if (followQueen)
        {
            direction = (queen.position + Quaternion.LookRotation(queen.forward, Vector3.up)*behindQueen) - tr.position;
            if (direction.sqrMagnitude < targetDistance*targetDistance) return;

            if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
            {
                tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up),
                                                10*Time.deltaTime);
            }
            if (direction.sqrMagnitude > nextWaypointDistance*nextWaypointDistance)
            {
                rb.AddForce(direction.normalized * moveSpeed *Time.deltaTime);
            }
            else
            {
                rb.AddForce(direction.normalized * followSpeed * Time.deltaTime);
            }
        }
        else if (victim != null)
        {
            if (path == null) return;

            if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance*targetDistance)
            {
                direction = path.vectorPath[currentWaypoint] - tr.position;
                direction.y = 0;
                direction = direction.normalized;

                if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
                {
                    tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up),
                                                   10*Time.deltaTime);
                }
                rb.AddForce(direction.normalized*moveSpeed*Time.deltaTime);

                if ((tr.position - path.vectorPath[currentWaypoint]).sqrMagnitude <
                    nextWaypointDistance*nextWaypointDistance && currentWaypoint < path.vectorPath.Count - 1)
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
                    if (throwable.rigidbody.tag == "Page")
                    {
                        item = throwable.rigidbody;
                        item.useGravity = false;
                        item.isKinematic = true;
                        item.GetComponent<Page>().grabbed = true;
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
        else if (window != null)
        {
            if (path == null) return;
            if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance * targetDistance)
            {
                direction = path.vectorPath[currentWaypoint] - tr.position;
                direction.y = 0;
                direction = direction.normalized;

                if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
                {
                    tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up), 10 * Time.deltaTime);
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
        //else
        //{
        //    if (path == null) return;

        //    if ((path.vectorPath[path.vectorPath.Count - 1] - tr.position).sqrMagnitude > targetDistance * targetDistance)
        //    {
        //        direction = path.vectorPath[currentWaypoint] - tr.position;
        //        direction.y = 0;
        //        direction = direction.normalized;

        //        if (Vector3.Angle(tr.forward, direction) > 5 && direction != Vector3.zero)
        //        {
        //            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up), 10 * Time.deltaTime);
        //            rb.AddForce(direction.normalized * turnSpeed * Time.deltaTime);
        //        }
        //        else
        //        {
        //            rb.AddForce(direction.normalized * moveSpeed * Time.deltaTime);
        //        }

        //        if ((tr.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance && currentWaypoint < path.vectorPath.Count - 1)
        //        {
        //            currentWaypoint++;
        //        }
        //    }
        //    else
        //    {
        //        path = null;
        //        direction = Vector3.zero;
        //        if (currentTarget < targetList.Length - 1)
        //        {
        //            currentTarget++;
        //            currentWaypoint = 0;
        //            SetTarget(targetList[currentTarget]);
        //        }
        //        else
        //        {
        //            currentTarget = 0;
        //            currentWaypoint = 0;
        //            SetTarget(targetList[currentTarget]);
        //        }
        //    }
        //}
    }

    public IEnumerator Panic()
    {
        speechBubble.SetText("");
        yield return new WaitForSeconds(1);
        while (grabbed)
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
}
