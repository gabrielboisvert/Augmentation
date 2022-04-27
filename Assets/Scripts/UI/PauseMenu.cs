using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    public GameObject FinishUI;

    
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
    }

    

    public void QuitGame()
    {
        GameManager.Fade.LoadAsyncScene("Menu", 0.5f);
    }
}