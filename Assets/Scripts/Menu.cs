using UnityEngine;

public class Menu : MonoBehaviour
{
    public Storyteller storyteller;

    private TextMesh text;

	void Start ()
	{
	    text = GetComponent<TextMesh>();
	}

    void OnMouseDown()
    {
        if (text.text == "Resume")
        {
            storyteller.Resume();
        }
        if (text.text == "New game")
        {
            storyteller.NewGame();
        }
        if (text.text == "Quit")
        {
            storyteller.Quit();
        }
    }
}
