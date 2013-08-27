using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    public Transform target;
    public Camera cam;
    public Transform camTransform;
    public TextMesh back;
    public TextMesh front;

    private Transform tr;
    private Vector3 point;
    private Vector3 newPosition;

	void Start ()
	{
	    tr = transform;
	}
	
	void Update ()
	{
	    if (target == null) return;
        if(camTransform == null) return;
	    if (front.text == "") return;
        tr.position = target.position + Vector3.up*4;
        tr.rotation = Quaternion.LookRotation(tr.position - camTransform.position, Vector3.up);
	}

    public void SetText(string text)
    {
        back.text = text;
        front.text = text;
    }
}
