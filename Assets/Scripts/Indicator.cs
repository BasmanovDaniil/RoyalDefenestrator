using UnityEngine;

public class Indicator : MonoBehaviour
{
    private Transform tr;

    private void Start()
    {
        tr = transform;
    }

    private void Update()
    {
        tr.up = Vector3.up;
    }
}