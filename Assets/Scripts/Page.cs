using UnityEngine;

public class Page : MonoBehaviour
{
    public int moveSpeed = 25000;
    public int throwForce = 3000;
    public LayerMask throwableLayer;
    public LayerMask characterLayer;
    public bool grabbed;
    public Transform head;

    [HideInInspector] public Transform tr;

    private Rigidbody rb;
    private Vector3 direction;
    private Collider[] throwables;
    private Collider[] characters;
    private Rigidbody item;
    private bool isCharacter;

    private void Start()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (grabbed)
        {
            if (item != null)
            {
                item.useGravity = true;
                item.isKinematic = false;
                item.AddForce(tr.up*throwForce);
                item = null;
            }
            return;
        }
        direction = Input.GetAxis("Vertical")*Vector3.forward + Input.GetAxis("Horizontal")*Vector3.right;

        if (Input.GetKeyDown("space") || Input.GetButtonDown("Jump"))
        {
            if (item == null)
            {
                characters = Physics.OverlapSphere(tr.position + tr.forward, 2, characterLayer);
                throwables = Physics.OverlapSphere(tr.position + tr.forward, 2, throwableLayer);
                if (characters.Length > 1)
                {
                    foreach (var character in characters)
                    {
                        if (character.tag != "Page")
                        {
                            isCharacter = true;
                            item = character.GetComponent<Rigidbody>();
                            item.useGravity = false;
                            item.isKinematic = true;
                            item.drag = 0;
                            if (item.tag == "Queen")
                            {
                                item.GetComponent<Queen>().grounded = false;
                                StartCoroutine(item.GetComponent<Queen>().Panic());
                            }

                            if (item.tag == "Cat" || item.name == "Cat")
                            {
                                item.GetComponent<Cat>().grounded = false;
                                StartCoroutine(item.GetComponent<Cat>().Panic());
                            }
                            if (item.tag == "Guard")
                            {
                                item.GetComponent<Guard>().grounded = false;
                                StartCoroutine(item.GetComponent<Guard>().Panic());
                            }

                            break;
                        }
                    }
                }
                else if (throwables.Length > 0)
                {
                    isCharacter = false;
                    item = throwables[0].GetComponent<Rigidbody>();
                    item.useGravity = false;
                    item.isKinematic = true;
                }
            }
            else
            {
                item.useGravity = true;
                item.isKinematic = false;
                item.AddForce((tr.forward + tr.up + Vector3.ClampMagnitude(rb.velocity, 1))*throwForce);
                item = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if (grabbed) return;
        if (item != null)
        {
            if (isCharacter)
            {
                if (item.tag == "Cat" || item.name == "Cat")
                {
                    item.position = tr.position + tr.up*2.5f;
                    item.rotation = tr.rotation;
                }
                else if (item.tag == "Queen")
                {
                    item.position = tr.position + tr.up*3 - tr.right;
                    item.rotation = tr.rotation*Quaternion.FromToRotation(Vector3.up, Vector3.right);
                }
                else
                {
                    item.position = tr.position + tr.up*4 - tr.right;
                    item.rotation = tr.rotation*Quaternion.FromToRotation(Vector3.up, Vector3.right);
                }
            }
            else
            {
                item.position = tr.position + tr.up*3;
                item.rotation = tr.rotation;
            }
        }

        if (direction == Vector3.zero) return;

        if (Vector3.Angle(tr.forward, direction) > 5)
        {
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up),
                10*Time.deltaTime);
        }
        rb.AddForce(direction.normalized*moveSpeed*Time.deltaTime);
    }
}