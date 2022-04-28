using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject pauseSelectedButton;
    public GameObject FinishUI;
    public GameObject finishSelectedButton;
    public Color selectedColor;


    private bool GameIsPaused = false;
    private GameObject current;

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.started)
            if (!GameIsPaused)
                Pause();
            else
                Resume();
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Pause()

    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        EventSystem.current.SetSelectedGameObject(this.pauseSelectedButton);
    }

    public void QuitGame()
    {
        GameManager.Fade.FadeStart("Menu", 1);
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

    public void OnRestart()
    {
        Time.timeScale = 1;
        GameManager.Fade.FadeStart("Combined LD", 1);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1;
        GameManager.Fade.FadeStart("Menu", 1);
    }

    public void OnWin()
    {
        Time.timeScale = 0;
        this.FinishUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(this.finishSelectedButton);
    }
}
