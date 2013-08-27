using UnityEngine;

public class Page : MonoBehaviour
{
    public int moveSpeed = 25000;
    public int throwForce = 3000;
    public LayerMask throwableLayer;
    public LayerMask characterLayer;
    public bool grabbed;

    private Transform tr;
    private Rigidbody rb;
    private Vector3 direction;
    private Collider[] throwables;
    private Collider[] characters;
    private Rigidbody item;
    private bool isCharacter;

	void Start ()
	{
	    tr = transform;
	    rb = rigidbody;
	}

    void Update()
    {
        if (grabbed)
        {
            if (item != null)
            {
                item.useGravity = true;
                item.isKinematic = false;
                item.AddForce(tr.up * throwForce);
                item = null;
            }
            return;
        }
        direction = Input.GetAxis("Vertical") * Vector3.forward + Input.GetAxis("Horizontal") * Vector3.right;

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
                            item = character.rigidbody;
                            item.useGravity = false;
                            item.isKinematic = true;
                            if (item.tag == "Queen")
                            {
                                item.GetComponent<Queen>().grabbed = true;
                                StartCoroutine(item.GetComponent<Queen>().Panic());
                            }
                            if (item.tag == "Guard")
                            {
                                item.GetComponent<Guard>().grabbed = true;
                                StartCoroutine(item.GetComponent<Guard>().Panic());
                            }
                            if (item.tag == "Cat" || item.name == "Cat")
                            {
                                item.GetComponent<Cat>().grabbed = true;
                                StartCoroutine(item.GetComponent<Cat>().Panic());
                            }

                            item.GetComponent<Head>().grabbed = true;
                            item.drag = 0;
                            break;
                        }
                    }
                }
                else if (throwables.Length > 0)
                {
                    isCharacter = false;
                    item = throwables[0].rigidbody;
                    item.useGravity = false;
                    item.isKinematic = true;
                }
            }
            else
            {
                if (isCharacter)
                {
                    item.GetComponent<Head>().grabbed = false;
                    if (item.tag == "Queen")
                    {
                        item.GetComponent<Queen>().grabbed = false;
                    }
                    if (item.tag == "Guard")
                    {
                        item.GetComponent<Guard>().grabbed = false;
                    }
                    if (item.tag == "Cat" || item.name == "Cat")
                    {
                        item.GetComponent<Cat>().grabbed = false;
                    }
                }
                item.useGravity = true;
                item.isKinematic = false;
                item.rotation = Quaternion.identity;
                item.AddForce((tr.forward + tr.up + Vector3.ClampMagnitude(rb.velocity, 1)) * throwForce);
                item = null;
            }
        }
    }

    void FixedUpdate()
	{
        if (grabbed) return;
        if (item != null)
        {
            if (isCharacter)
            {
                if (item.tag == "Cat" || item.name == "Cat")
                {
                    item.position = tr.position + tr.up * 2.5f;
                    item.rotation = tr.rotation;
                }
                else if (item.tag == "Queen")
                {
                    item.position = tr.position + tr.up * 3 - tr.right;
                    item.rotation = tr.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.right);
                }
                else
                {
                    item.position = tr.position + tr.up * 4 - tr.right;
                    item.rotation = tr.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.right);
                }
            }
            else
            {
                item.position = tr.position + tr.up * 3;
                item.rotation = tr.rotation;
            }
        }

        if (direction == Vector3.zero) return;
        
        if (Vector3.Angle(tr.forward, direction) > 5)
        {
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction, Vector3.up), 10 * Time.deltaTime);
        }
        rb.AddForce(tr.forward * moveSpeed * Time.deltaTime);
	}
}
