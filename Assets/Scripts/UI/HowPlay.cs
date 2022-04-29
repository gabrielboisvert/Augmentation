using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowPlay : MonoBehaviour
{
    public void continueToGame()
    {
        GameManager.Fade.FadeStart("Combined LD", 1f);
    }
}
