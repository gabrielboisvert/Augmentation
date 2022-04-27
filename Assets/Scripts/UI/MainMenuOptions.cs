using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuOptions : MonoBehaviour
{
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject credit;

    public void OnStart()
    {
        GameManager.Fade.LoadAsyncScene(1, 1);
    }

    public void OnCredit()
    {
        this.mainMenu.SetActive(false);
        this.credit.SetActive(true);
    }

    public void OnExit()
    {
        Application.Quit(0);
    }
}
