using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    public Transform target;
    public Transform camTransform;
    public TextMesh back;
    public TextMesh front;

    private Transform tr;
    private Vector3 point;
    private Vector3 newPosition;

    private void Awake()
    {
        tr = transform;
    }

    private void Update()
    {
        if (target == null || camTransform == null) return;
        if (front.text == "") return;
        tr.position = target.position + Vector3.up*4;
        tr.rotation = Quaternion.LookRotation(tr.position - camTransform.position, Vector3.up);
    }

    public void SetText(string text)
    {
        if (target == null || camTransform == null) return;
        tr.position = target.position + Vector3.up*4;
        tr.rotation = Quaternion.LookRotation(tr.position - camTransform.position, Vector3.up);
        back.text = text;
        front.text = text;
    }
}