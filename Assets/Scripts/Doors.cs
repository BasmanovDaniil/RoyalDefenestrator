using UnityEngine;

public class Doors : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;

    public void Open()
    {
        leftDoor.position -= leftDoor.right*3;
        rightDoor.position += rightDoor.right*3;
        GetComponent<AudioSource>().Play();
    }

    public void Close()
    {
        leftDoor.position += leftDoor.right*3;
        rightDoor.position -= rightDoor.right*3;
        GetComponent<AudioSource>().Play();
    }
}