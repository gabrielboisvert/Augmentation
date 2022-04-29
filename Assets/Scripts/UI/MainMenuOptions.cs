using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuOptions : MonoBehaviour
{
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject credit;
    public Color selectedColor;
    public GameObject MainMenuBackButton;
    public GameObject creditBackButton;


    private GameObject current;

    public void OnStart()
    {
        GameManager.Fade.FadeStart("HowToPlay", 1);
    }

    public void OnCredit()
    {
        this.GetComponent<AudioSource>().Play();

        this.mainMenu.SetActive(false);
        this.credit.SetActive(true);
        EventSystem.current.SetSelectedGameObject(this.creditBackButton);
    }

    public void OnBack()
    {
        this.mainMenu.SetActive(true);
        this.credit.SetActive(false);
        EventSystem.current.SetSelectedGameObject(this.MainMenuBackButton);
    }

    public void OnExit()
    {
        Application.Quit(0);
    }

    public void Update()
    {
        GameObject tmp = EventSystem.current.currentSelectedGameObject;

        if (this.current == null)
        {
            this.current = tmp;
            if (this.current == null)
                return;

            if (this.current.GetComponent<Button>() != null)
                this.current.GetComponentInChildren<Text>().color = selectedColor;
        }

        if (this.current != tmp)
        {
            this.current.GetComponentInChildren<Text>().color = Color.white;
            this.current = tmp;
            if (this.current == null)
                return;

            if (this.current.GetComponent<Button>() != null)
                this.current.GetComponentInChildren<Text>().color = selectedColor;
        }
    }
}
