using System.Collections;
using UnityEngine;

public class Storyteller : MonoBehaviour
{
    public Doors doors;
    public Queen queen;
    public Camera menuCam;
    public Camera followCam;
    public Camera goodCam;
    public Camera goodCamAlt;
    public Camera badCam;
    public TextMesh resume;
    public TextMesh newGame;
    public TextMesh quit;
    public TextMesh copyright;
    public TextMesh royal;
    public AstarPath astar;
    public GameObject memory;
    public Guard guardOne;
    public Guard[] guards;
    public int guardCount = 13;
    public SpeechBubble guardOneSpeechBubble;
    public Transform firstVictim;
    public GameObject indicatorPrefab;
    public Transform goodAltCat;
    [HideInInspector]
    public bool firstVictimDead;
    public bool catKilled = false;

    private bool inMenu;
    private RaycastHit rayHit;
    private TextMesh hit;
    private bool firstTime = true;
    private bool end = false;

    void Awake()
    {
        DontDestroyOnLoad(memory);
    }

	void Start ()
	{
	    var memories = GameObject.FindGameObjectsWithTag("Respawn");
        if (memories.Length > 1)
        {
            firstTime = false;
            CloseMenu();
            StartScenario();
        }
        else
        {
            firstTime = true;
            OpenMenu();
        }
        InvokeRepeating("UpdateGraph", 5, 5);
	}

    void UpdateGraph()
    {
        if (AstarPath.active)
        {
            astar.Scan();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("escape") && !inMenu)
        {
            OpenMenu();
        }
    }

    public void Resume()
    {
        CloseMenu();
    }

    public void NewGame()
    {
        if (firstTime && !end)
        {
            CloseMenu();
            firstTime = false;
            StartCoroutine(StartTutorial());
        }
        else
        {
            Application.LoadLevel(0);
        }
    }

    void StartScenario()
    {
        Invoke("OpenDoors", 2);
        Invoke("CallGuards", 4);
    }

    public void GoodEnding()
    {
        goodCam.enabled = true;
        menuCam.enabled = false;
        followCam.enabled = false;
        goodCamAlt.enabled = false;
        badCam.enabled = false;
        end = true;
        foreach (var guard in guards)
        {
            guard.walking = false;
        }
        Invoke("OpenMenu", 2);
    }

    public IEnumerator GoodEndingAlt()
    {
        while (guardCount > 1)
        {
            yield return new WaitForSeconds(1);
        }
        if (catKilled)
        {
            goodAltCat.position += Vector3.forward*100;
        }
        goodCamAlt.enabled = true;
        goodCam.enabled = false;
        menuCam.enabled = false;
        followCam.enabled = false;
        badCam.enabled = false;
        end = true;
        foreach (var guard in guards)
        {
            guard.walking = false;
        }
        Invoke("OpenMenu", 2);
    }

    public void BadEnding()
    {
        badCam.enabled = true;
        goodCamAlt.enabled = false;
        goodCam.enabled = false;
        menuCam.enabled = false;
        followCam.enabled = false;
        end = true;
        foreach (var guard in guards)
        {
            guard.walking = false;
        }
        Invoke("OpenMenu", 2);
    }

    IEnumerator StartTutorial()
    {
        firstVictim.tag = "FirstVictim";
        yield return new WaitForSeconds(2);
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else
        {
            guardOneSpeechBubble.SetText("Looks like our queen is in a bad mood");
            yield return new WaitForSeconds(4);
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else
        {
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(1);
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else
        {
            guardOneSpeechBubble.SetText("*SIGH*");
            yield return new WaitForSeconds(3);
        }
        if (!guardOne.grounded)
        {
            StartScenario();
            yield break;
        }
        else
        {
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(1);
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("Hey! What is this?!");
            guardOne.target = firstVictim;
            var clone = Instantiate(indicatorPrefab, firstVictim.position, Quaternion.identity) as GameObject;
            clone.transform.parent = firstVictim;
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            guardOneSpeechBubble.SetText("Queen!");
            yield return new WaitForSeconds(2);
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("Oh, No-no-no-no!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            guardOneSpeechBubble.SetText("Queen!");
            yield return new WaitForSeconds(2);
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("Quick! Hide that! Throw it to window!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            guardOneSpeechBubble.SetText("Queen!");
            yield return new WaitForSeconds(2);
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("There is no time! Throw it!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            guardOneSpeechBubble.SetText("Good!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("Throw it!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            yield return new WaitForSeconds(3);
        }
        else
        {
            guardOneSpeechBubble.SetText("Good!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        if (!guardOne.grounded)
        {
            guardOneSpeechBubble.SetText("");
            StartScenario();
            yield break;
        }
        else if (!firstVictimDead)
        {
            guardOneSpeechBubble.SetText("Ok, your choice");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            StartScenario();
        }
        else
        {
            guardOneSpeechBubble.SetText("Good!");
            yield return new WaitForSeconds(3);
            guardOneSpeechBubble.SetText("");
            StartScenario();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenMenu()
    {
        menuCam.enabled = true;
        followCam.enabled = false;
        goodCam.enabled = false;
        goodCamAlt.enabled = false;
        badCam.enabled = false;
        inMenu = true;
        royal.text = "Royal Defenestrator";
        copyright.text = "by Daniil Basmanov for LD27";
        if (!firstTime && !end)
        {
            resume.text = "Resume";
        }
        else
        {
            resume.text = "";
        }
        newGame.text = "New game";
        quit.text = "Quit";
    }

    void CloseMenu()
    {
        followCam.enabled = true;
        menuCam.enabled = false;
        goodCam.enabled = false;
        goodCamAlt.enabled = false;
        badCam.enabled = false;
        inMenu = false;
        royal.text = "";
        copyright.text = "";
        resume.text = "";
        newGame.text = "";
        quit.text = "";
    }

    void OpenDoors()
    {
        doors.Open();
        queen.walking = true;
    }

    void CallGuards()
    {
        queen.CallGuards();
    }
}
